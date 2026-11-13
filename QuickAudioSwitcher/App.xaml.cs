using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using QuickAudioSwitcher.Core;
using QuickAudioSwitcher.UI;

namespace QuickAudioSwitcher
{
    public partial class App : System.Windows.Application
    {
        private static Mutex? _singleInstanceMutex;
        private AudioEngine? _audioEngine;
        private HotkeyManager? _hotkeyManager;
        private TrayController? _trayController;
        private FlyoutWindow? _flyoutWindow;
        private OsdWindow? _osdWindow;
        private SettingsWindow? _settingsWindow;
        private AppConfig _config = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Single instance protection
            bool createdNew;
            _singleInstanceMutex = new Mutex(true, "QuickAudioSwitcher_SingleInstance_Mutex_2026", out createdNew);
            if (!createdNew)
            {
                System.Windows.MessageBox.Show(
                    "QuickAudioSwitcher уже запущен и находится в системном трее (возле часов).",
                    "QuickAudioSwitcher",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Shutdown();
                return;
            }

            string debugLog = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickAudioSwitcher_debug.log");
            System.IO.File.WriteAllText(debugLog, $"[{DateTime.Now}] Starting...\n");

            try
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                System.IO.File.AppendAllText(debugLog, "Loading config...\n");
                _config = ConfigManager.Load();
                Loc.SetLanguage(_config.Language);
                ThemeManager.ApplyTheme(_config.Theme);
                System.IO.File.AppendAllText(debugLog, "Init AudioEngine...\n");
                _audioEngine = new AudioEngine();
                System.IO.File.AppendAllText(debugLog, "Init HotkeyManager...\n");
                _hotkeyManager = new HotkeyManager();
                System.IO.File.AppendAllText(debugLog, "Init OsdWindow...\n");
                _osdWindow = new OsdWindow();

                System.IO.File.AppendAllText(debugLog, "Init FlyoutWindow...\n");
                _flyoutWindow = new FlyoutWindow(
                    _audioEngine,
                    () => _config,
                    OpenSettingsWindow);

                System.IO.File.AppendAllText(debugLog, "Init TrayController...\n");
                _trayController = new TrayController(
                    onLeftClick: pt => _flyoutWindow.ShowNear(pt),
                    onOpenSettings: OpenSettingsWindow,
                    onCycleNext: CycleNextFavoriteDevice,
                    onExit: ExitApplication);

                System.IO.File.AppendAllText(debugLog, "Wiring events...\n");
                _audioEngine.DefaultDeviceChanged += OnDefaultDeviceChanged;

                System.IO.File.AppendAllText(debugLog, "Applying hotkey...\n");
                ApplyHotkeyConfig();
                _hotkeyManager.HotkeyPressed += CycleNextFavoriteDevice;

                System.IO.File.AppendAllText(debugLog, "Updating tooltip...\n");
                UpdateCurrentTooltip();

                bool hasFavorites = _config.Devices.Any(d => d.IsFavorite);
                System.IO.File.AppendAllText(debugLog, $"hasFavorites: {hasFavorites}\n");
                if (!hasFavorites)
                {
                    OpenSettingsWindow();
                }

                System.IO.File.AppendAllText(debugLog, "OnStartup completed successfully!\n");
            }
            catch (Exception ex)
            {
                try
                {
                    string logPath = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "QuickAudioSwitcher_crash.log");
                    System.IO.File.WriteAllText(logPath, ex.ToString());
                }
                catch { }

                System.Windows.MessageBox.Show(
                    $"Ошибка запуска QuickAudioSwitcher:\n{ex.Message}\n\nStack:\n{ex.StackTrace}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void ApplyHotkeyConfig()
        {
            if (_hotkeyManager == null) return;

            _hotkeyManager.Unregister();
            if (_config.HotkeyEnabled)
            {
                if (_config.HotkeyIsMouse)
                {
                    _hotkeyManager.RegisterMouse(_config.HotkeyMouseButton, _config.HotkeyModifiers);
                }
                else
                {
                    _hotkeyManager.RegisterKeyboard(_config.HotkeyModifiers, _config.HotkeyKey);
                }
            }
        }

        public void CycleNextFavoriteDevice()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.InvokeAsync(CycleNextFavoriteDevice);
                return;
            }

            if (_audioEngine == null) return;

            var allDevices = _audioEngine.GetPlaybackDevices();
            var favoritePrefs = _config.Devices.Where(d => d.IsFavorite).ToList();

            if (favoritePrefs.Count == 0)
            {
                OpenSettingsWindow();
                return;
            }

            // Filter and match active devices
            var activeFavorites = new List<AudioDevice>();
            foreach (var pref in favoritePrefs)
            {
                var matched = allDevices.FirstOrDefault(d => string.Equals(d.Id, pref.Id, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    matched.CustomName = pref.CustomName;
                    activeFavorites.Add(matched);
                }
            }

            if (activeFavorites.Count == 0) return;

            string? currentDefaultId = _audioEngine.GetDefaultDeviceId();
            int currentIndex = activeFavorites.FindIndex(d => string.Equals(d.Id, currentDefaultId, StringComparison.OrdinalIgnoreCase));

            // Select next device in cycle
            int nextIndex = (currentIndex + 1) % activeFavorites.Count;
            var nextDevice = activeFavorites[nextIndex];

            try
            {
                // Query target device's volume directly!
                float currentVol = _audioEngine.GetDeviceVolume(nextDevice.Id);
                int volPercent = (int)Math.Round(currentVol * 100);

                if (_config.ShowOsd && _osdWindow != null)
                {
                    _osdWindow.ShowDevice(nextDevice.DisplayName, volPercent);
                }

                _trayController?.UpdateTooltip(nextDevice.DisplayName);

                // Run device switch asynchronously so UI never hangs
                _ = _audioEngine.SetDefaultDeviceAsync(nextDevice.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error switching device: {ex.Message}");
            }
        }

        private void OnDefaultDeviceChanged(string deviceId)
        {
            Dispatcher.InvokeAsync(() =>
            {
                UpdateCurrentTooltip(deviceId);
            });
        }

        private void UpdateCurrentTooltip(string? deviceId = null)
        {
            if (_audioEngine == null || _trayController == null) return;

            string? targetId = deviceId ?? _audioEngine.GetDefaultDeviceId();
            if (!string.IsNullOrEmpty(targetId))
            {
                var pref = _config.Devices.FirstOrDefault(p => string.Equals(p.Id, targetId, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(pref?.CustomName))
                {
                    _trayController.UpdateTooltip(pref.CustomName);
                    return;
                }
            }
            _trayController.UpdateTooltip("QuickAudioSwitcher");
        }

        private void OpenSettingsWindow()
        {
            if (_settingsWindow != null && _settingsWindow.IsLoaded)
            {
                _settingsWindow.Activate();
                return;
            }

            if (_audioEngine == null) return;

            _settingsWindow = new SettingsWindow(
                _audioEngine,
                _config,
                onConfigSaved: updatedConfig =>
                {
                    _config = updatedConfig;
                    ApplyHotkeyConfig();
                    UpdateCurrentTooltip();
                    _flyoutWindow?.RefreshDevices();
                });

            _settingsWindow.Closed += (s, e) => _settingsWindow = null;
            _settingsWindow.Show();
            _settingsWindow.Activate();
        }

        private void ExitApplication()
        {
            _trayController?.Dispose();
            _hotkeyManager?.Dispose();
            _audioEngine?.Dispose();
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();

            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                string debugLog = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickAudioSwitcher_debug.log");
                System.IO.File.AppendAllText(debugLog, $"[{DateTime.Now}] OnExit called! ExitCode: {e.ApplicationExitCode}\n");
                _trayController?.Dispose();
                _hotkeyManager?.Dispose();
                _audioEngine?.Dispose();
                _singleInstanceMutex?.Dispose();
            }
            catch
            {
                // Suppress on exit
            }
            base.OnExit(e);
        }
    }
}
