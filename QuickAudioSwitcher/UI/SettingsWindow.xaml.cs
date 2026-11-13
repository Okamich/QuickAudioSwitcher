using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using QuickAudioSwitcher.Core;

namespace QuickAudioSwitcher.UI
{
    public partial class SettingsWindow : Window
    {
        private readonly AudioEngine _audioEngine;
        private readonly AppConfig _currentConfig;
        private readonly Action<AppConfig> _onConfigSaved;

        private bool _isRecordingHotkey;
        private bool _curIsMouse;
        private int _curMouseButton;
        private uint _curModifiers;
        private uint _curKey;
        private string _curDisplayText = "Ctrl + Alt + A";

        private string _curLanguage = "ru";
        private string _curTheme = "Dark";

        public class DeviceRowModel
        {
            public AudioDevice Device { get; set; } = null!;
            public Border RowBorder { get; set; } = null!;
            public CheckBox ChkFavorite { get; set; } = null!;
            public TextBlock TxtSysName { get; set; } = null!;
            public TextBlock TxtAdapter { get; set; } = null!;
            public TextBox TxtCustomName { get; set; } = null!;
            public Button BtnTest { get; set; } = null!;
        }

        private readonly List<DeviceRowModel> _rowModels = new();

        public SettingsWindow(AudioEngine audioEngine, AppConfig currentConfig, Action<AppConfig> onConfigSaved)
        {
            InitializeComponent();
            _audioEngine = audioEngine;
            _currentConfig = currentConfig;
            _onConfigSaved = onConfigSaved;

            LoadSettingsIntoUi();
            PopulateDevices();
        }

        private void LoadSettingsIntoUi()
        {
            ChkAutostart.IsChecked = _currentConfig.AutostartEnabled;
            ChkShowOsd.IsChecked = _currentConfig.ShowOsd;
            ChkEnableHotkey.IsChecked = _currentConfig.HotkeyEnabled;

            _curIsMouse = _currentConfig.HotkeyIsMouse;
            _curMouseButton = _currentConfig.HotkeyMouseButton;
            _curModifiers = _currentConfig.HotkeyModifiers;
            _curKey = _currentConfig.HotkeyKey;
            _curDisplayText = !string.IsNullOrWhiteSpace(_currentConfig.HotkeyDisplayText)
                ? _currentConfig.HotkeyDisplayText
                : "Ctrl + Alt + A";

            _curLanguage = !string.IsNullOrWhiteSpace(_currentConfig.Language) ? _currentConfig.Language : "ru";
            _curTheme = !string.IsNullOrWhiteSpace(_currentConfig.Theme) ? _currentConfig.Theme : "Dark";

            UpdateHotkeyButtonUi();
            UpdatePreferencesButtonsUi();
            UpdateLocalization();
        }

        private void UpdateLocalization()
        {
            Title = Loc.T("SettingsWindowTitle");
            TxtHeaderTitle.Text = Loc.T("SettingsHeaderTitle");
            TxtHeaderSubtitle.Text = Loc.T("SettingsHeaderSubtitle");

            TxtColRotation.Text = Loc.T("ColRotation");
            TxtColDevice.Text = Loc.T("ColDevice");
            TxtColCustomName.Text = Loc.T("ColCustomName");
            TxtColTest.Text = Loc.T("ColTest");

            TxtHotkeyTitle.Text = Loc.T("HotkeyTitle");
            ChkEnableHotkey.Content = Loc.T("ChkEnableHotkey");
            BtnResetHotkey.Content = Loc.T("BtnReset");
            BtnResetHotkey.ToolTip = Loc.T("BtnResetTooltip");
            TxtHotkeyHint.Text = Loc.T("TxtHotkeyHint");

            TxtSystemTitle.Text = Loc.T("SystemTitle");
            ChkAutostart.Content = Loc.T("ChkAutostart");
            ChkShowOsd.Content = Loc.T("ChkShowOsd");
            TxtTrayNotice.Text = Loc.T("TrayNotice");

            TxtPrefsTitle.Text = Loc.T("PreferencesTitle");
            TxtLabelLanguage.Text = Loc.T("LabelLanguage");
            TxtLabelTheme.Text = Loc.T("LabelTheme");
            BtnThemeDark.Content = "🌙 " + Loc.T("ThemeDark");
            BtnThemeLight.Content = "☀ " + Loc.T("ThemeLight");

            BtnRefresh.Content = Loc.T("BtnRefresh");
            BtnCancel.Content = Loc.T("BtnCancel");
            BtnSave.Content = Loc.T("BtnSave");

            foreach (var row in _rowModels)
            {
                row.ChkFavorite.Content = Loc.T("FavCheckbox");
                row.TxtCustomName.ToolTip = Loc.T("CustomNameTooltip");
                row.BtnTest.ToolTip = Loc.T("ColTest");
            }
        }

        private void UpdatePreferencesButtonsUi()
        {
            bool isDark = ThemeManager.IsDark;

            // Highlight Active Language
            HighlightPreferenceButton(BtnLangRu, string.Equals(_curLanguage, "ru", StringComparison.OrdinalIgnoreCase), isDark);
            HighlightPreferenceButton(BtnLangEn, string.Equals(_curLanguage, "en", StringComparison.OrdinalIgnoreCase), isDark);

            // Highlight Active Theme
            HighlightPreferenceButton(BtnThemeDark, string.Equals(_curTheme, "Dark", StringComparison.OrdinalIgnoreCase), isDark);
            HighlightPreferenceButton(BtnThemeLight, string.Equals(_curTheme, "Light", StringComparison.OrdinalIgnoreCase), isDark);
        }

        private static void HighlightPreferenceButton(Button btn, bool isActive, bool isDark)
        {
            if (isActive)
            {
                btn.Background = isDark ? new SolidColorBrush(Color.FromRgb(20, 42, 60)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                btn.BorderBrush = isDark ? new SolidColorBrush(Color.FromRgb(0, 229, 255)) : new SolidColorBrush(Color.FromRgb(2, 132, 199));
                btn.Foreground = isDark ? new SolidColorBrush(Color.FromRgb(0, 229, 255)) : new SolidColorBrush(Color.FromRgb(2, 132, 199));
                btn.FontWeight = FontWeights.Bold;
            }
            else
            {
                btn.Background = isDark ? new SolidColorBrush(Color.FromRgb(26, 30, 39)) : new SolidColorBrush(Color.FromRgb(255, 255, 255));
                btn.BorderBrush = isDark ? new SolidColorBrush(Color.FromRgb(47, 54, 70)) : new SolidColorBrush(Color.FromRgb(203, 213, 225));
                btn.Foreground = isDark ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(51, 65, 85));
                btn.FontWeight = FontWeights.Normal;
            }
        }

        private void BtnLangRu_Click(object sender, RoutedEventArgs e)
        {
            _curLanguage = "ru";
            Loc.SetLanguage(_curLanguage);
            UpdatePreferencesButtonsUi();
            UpdateLocalization();
        }

        private void BtnLangEn_Click(object sender, RoutedEventArgs e)
        {
            _curLanguage = "en";
            Loc.SetLanguage(_curLanguage);
            UpdatePreferencesButtonsUi();
            UpdateLocalization();
        }

        private void BtnThemeDark_Click(object sender, RoutedEventArgs e)
        {
            _curTheme = "Dark";
            ThemeManager.ApplyTheme(_curTheme);
            UpdatePreferencesButtonsUi();
            UpdateHotkeyButtonUi();
            RefreshDeviceRowBrushes();
        }

        private void BtnThemeLight_Click(object sender, RoutedEventArgs e)
        {
            _curTheme = "Light";
            ThemeManager.ApplyTheme(_curTheme);
            UpdatePreferencesButtonsUi();
            UpdateHotkeyButtonUi();
            RefreshDeviceRowBrushes();
        }

        private void UpdateHotkeyButtonUi()
        {
            bool isDark = ThemeManager.IsDark;
            if (_isRecordingHotkey)
            {
                BtnRecordHotkey.Content = Loc.T("ListeningHotkey");
                BtnRecordHotkey.Background = isDark ? new SolidColorBrush(Color.FromRgb(40, 18, 24)) : new SolidColorBrush(Color.FromRgb(254, 242, 242));
                BtnRecordHotkey.BorderBrush = isDark ? new SolidColorBrush(Color.FromRgb(255, 77, 77)) : new SolidColorBrush(Color.FromRgb(239, 68, 68));
                BtnRecordHotkey.Foreground = isDark ? new SolidColorBrush(Color.FromRgb(255, 110, 110)) : new SolidColorBrush(Color.FromRgb(185, 28, 28));
            }
            else
            {
                BtnRecordHotkey.Content = $"⌨ {_curDisplayText}";
                BtnRecordHotkey.Background = isDark ? new SolidColorBrush(Color.FromRgb(26, 30, 39)) : new SolidColorBrush(Color.FromRgb(255, 255, 255));
                BtnRecordHotkey.BorderBrush = isDark ? new SolidColorBrush(Color.FromRgb(0, 229, 255)) : new SolidColorBrush(Color.FromRgb(2, 132, 199));
                BtnRecordHotkey.Foreground = isDark ? new SolidColorBrush(Color.FromRgb(0, 229, 255)) : new SolidColorBrush(Color.FromRgb(2, 132, 199));
            }
        }

        private void BtnRecordHotkey_Click(object sender, RoutedEventArgs e)
        {
            if (_isRecordingHotkey)
            {
                StopRecording(cancel: true);
            }
            else
            {
                _isRecordingHotkey = true;
                Mouse.Capture(this, CaptureMode.SubTree);
                Keyboard.Focus(this);
                UpdateHotkeyButtonUi();
            }
        }

        private void BtnResetHotkey_Click(object sender, RoutedEventArgs e)
        {
            _curIsMouse = false;
            _curMouseButton = 0;
            _curModifiers = HotkeyManager.MOD_CONTROL | HotkeyManager.MOD_ALT;
            _curKey = 0x41; // 'A'
            _curDisplayText = "Ctrl + Alt + A";

            StopRecording(cancel: true);
            UpdateHotkeyButtonUi();
        }

        private void StopRecording(bool cancel)
        {
            _isRecordingHotkey = false;
            Mouse.Capture(null);
            UpdateHotkeyButtonUi();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (_isRecordingHotkey)
            {
                if (e.Key == Key.Escape)
                {
                    StopRecording(cancel: true);
                    e.Handled = true;
                    return;
                }

                if (e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin or Key.System)
                {
                    return;
                }

                uint mods = 0;
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) mods |= HotkeyManager.MOD_CONTROL;
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) mods |= HotkeyManager.MOD_ALT;
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) mods |= HotkeyManager.MOD_SHIFT;
                if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin)) mods |= HotkeyManager.MOD_WIN;

                Key targetKey = e.Key == Key.System ? e.SystemKey : e.Key;
                uint vk = (uint)KeyInterop.VirtualKeyFromKey(targetKey);

                _curIsMouse = false;
                _curMouseButton = 0;
                _curModifiers = mods;
                _curKey = vk;
                _curDisplayText = FormatHotkeyText(mods, targetKey.ToString());

                StopRecording(cancel: false);
                e.Handled = true;
                return;
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (_isRecordingHotkey)
            {
                if (e.ChangedButton is MouseButton.XButton1 or MouseButton.XButton2 or MouseButton.Middle)
                {
                    uint mods = 0;
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) mods |= HotkeyManager.MOD_CONTROL;
                    if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) mods |= HotkeyManager.MOD_ALT;
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) mods |= HotkeyManager.MOD_SHIFT;
                    if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin)) mods |= HotkeyManager.MOD_WIN;

                    int buttonCode = e.ChangedButton switch
                    {
                        MouseButton.XButton1 => 4,
                        MouseButton.XButton2 => 5,
                        MouseButton.Middle => 3,
                        _ => 0
                    };

                    string buttonName = e.ChangedButton switch
                    {
                        MouseButton.XButton1 => "Mouse 4 (Back)",
                        MouseButton.XButton2 => "Mouse 5 (Forward)",
                        MouseButton.Middle => "Middle Click",
                        _ => e.ChangedButton.ToString()
                    };

                    _curIsMouse = true;
                    _curMouseButton = buttonCode;
                    _curModifiers = mods;
                    _curKey = 0;
                    _curDisplayText = FormatHotkeyText(mods, buttonName);

                    StopRecording(cancel: false);
                    e.Handled = true;
                    return;
                }
            }

            base.OnPreviewMouseDown(e);
        }

        private static string FormatHotkeyText(uint mods, string keyName)
        {
            var parts = new List<string>();
            if ((mods & HotkeyManager.MOD_CONTROL) != 0) parts.Add("Ctrl");
            if ((mods & HotkeyManager.MOD_ALT) != 0) parts.Add("Alt");
            if ((mods & HotkeyManager.MOD_SHIFT) != 0) parts.Add("Shift");
            if ((mods & HotkeyManager.MOD_WIN) != 0) parts.Add("Win");
            parts.Add(keyName);
            return string.Join(" + ", parts);
        }

        private void PopulateDevices()
        {
            PnlDeviceRows.Children.Clear();
            _rowModels.Clear();

            var devices = _audioEngine.GetPlaybackDevices();

            foreach (var dev in devices)
            {
                var pref = _currentConfig.Devices.FirstOrDefault(p => string.Equals(p.Id, dev.Id, StringComparison.OrdinalIgnoreCase));
                if (pref != null)
                {
                    dev.IsFavorite = pref.IsFavorite;
                    dev.CustomName = pref.CustomName;
                }
            }

            var sorted = devices
                .OrderByDescending(d => d.IsFavorite)
                .ThenBy(d => d.DisplayName)
                .ToList();

            foreach (var dev in sorted)
            {
                var rowModel = CreateDeviceRow(dev);
                _rowModels.Add(rowModel);
            }
        }

        private void RefreshDeviceRowBrushes()
        {
            foreach (var row in _rowModels)
            {
                row.RowBorder.SetResourceReference(Border.BackgroundProperty, "StudioCardBgBrush");
                row.RowBorder.SetResourceReference(Border.BorderBrushProperty, "StudioBorderBrush");
                row.ChkFavorite.SetResourceReference(Control.ForegroundProperty, "StudioTextPrimaryBrush");
                row.TxtSysName.SetResourceReference(TextBlock.ForegroundProperty, "StudioTextPrimaryBrush");
                row.TxtAdapter.SetResourceReference(TextBlock.ForegroundProperty, "StudioTextSecondaryBrush");
            }
        }

        private DeviceRowModel CreateDeviceRow(AudioDevice dev)
        {
            var border = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 0, 8),
                Padding = new Thickness(12, 10, 12, 10),
                SnapsToDevicePixels = true
            };
            border.SetResourceReference(Border.BackgroundProperty, "StudioCardBgBrush");
            border.SetResourceReference(Border.BorderBrushProperty, "StudioBorderBrush");

            border.MouseEnter += (s, e) =>
            {
                border.SetResourceReference(Border.BackgroundProperty, "StudioCardHoverBrush");
                border.SetResourceReference(Border.BorderBrushProperty, "StudioBorderHighlightBrush");
            };
            border.MouseLeave += (s, e) =>
            {
                border.SetResourceReference(Border.BackgroundProperty, "StudioCardBgBrush");
                border.SetResourceReference(Border.BorderBrushProperty, "StudioBorderBrush");
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) }); // Favorite checkbox
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // System name & info
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) }); // Custom name box
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Test button

            // 1. Favorite Checkbox
            var chkFav = new CheckBox
            {
                Content = Loc.T("FavCheckbox"),
                IsChecked = dev.IsFavorite,
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };
            chkFav.SetResourceReference(Control.ForegroundProperty, "StudioTextPrimaryBrush");
            Grid.SetColumn(chkFav, 0);
            grid.Children.Add(chkFav);

            // 2. Info stack
            var infoStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 10, 0) };
            var txtSysName = new TextBlock
            {
                Text = dev.SystemName,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            txtSysName.SetResourceReference(TextBlock.ForegroundProperty, "StudioTextPrimaryBrush");

            var txtAdapter = new TextBlock
            {
                Text = dev.AdapterName,
                FontSize = 11,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 2, 0, 0)
            };
            txtAdapter.SetResourceReference(TextBlock.ForegroundProperty, "StudioTextSecondaryBrush");

            infoStack.Children.Add(txtSysName);
            infoStack.Children.Add(txtAdapter);
            Grid.SetColumn(infoStack, 1);
            grid.Children.Add(infoStack);

            // 3. Custom Name TextBox
            var txtCustom = new TextBox
            {
                Text = dev.CustomName,
                Style = (Style)FindResource("ModernTextBoxStyle"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
                ToolTip = Loc.T("CustomNameTooltip")
            };
            Grid.SetColumn(txtCustom, 2);
            grid.Children.Add(txtCustom);

            // 4. Test button
            var btnTest = new Button
            {
                Content = Loc.T("BtnTest"),
                Style = (Style)FindResource("StudioTestButtonStyle"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 92,
                ToolTip = Loc.T("ColTest")
            };

            btnTest.Click += async (s, e) =>
            {
                btnTest.IsEnabled = false;
                btnTest.Content = Loc.T("BtnTesting");
                try
                {
                    await AudioTester.PlayTestChimeAsync(dev.Id);
                }
                finally
                {
                    btnTest.Content = Loc.T("BtnTest");
                    btnTest.IsEnabled = true;
                }
            };
            Grid.SetColumn(btnTest, 3);
            grid.Children.Add(btnTest);

            border.Child = grid;
            PnlDeviceRows.Children.Add(border);

            return new DeviceRowModel
            {
                Device = dev,
                RowBorder = border,
                ChkFavorite = chkFav,
                TxtSysName = txtSysName,
                TxtAdapter = txtAdapter,
                TxtCustomName = txtCustom,
                BtnTest = btnTest
            };
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            PopulateDevices();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Revert unsaved preview of language and theme
            Loc.SetLanguage(_currentConfig.Language);
            ThemeManager.ApplyTheme(_currentConfig.Theme);
            Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Collect device preferences
            var updatedPreferences = new List<DevicePreference>();
            int order = 0;

            foreach (var row in _rowModels)
            {
                bool isFav = row.ChkFavorite.IsChecked == true;
                string custom = row.TxtCustomName.Text.Trim();

                if (isFav || !string.IsNullOrEmpty(custom))
                {
                    updatedPreferences.Add(new DevicePreference
                    {
                        Id = row.Device.Id,
                        CustomName = custom,
                        IsFavorite = isFav,
                        Order = order++
                    });
                }
            }

            _currentConfig.Devices = updatedPreferences;
            _currentConfig.AutostartEnabled = ChkAutostart.IsChecked == true;
            _currentConfig.ShowOsd = ChkShowOsd.IsChecked == true;
            _currentConfig.HotkeyEnabled = ChkEnableHotkey.IsChecked == true;

            // Hotkey settings
            _currentConfig.HotkeyIsMouse = _curIsMouse;
            _currentConfig.HotkeyMouseButton = _curMouseButton;
            _currentConfig.HotkeyModifiers = _curModifiers;
            _currentConfig.HotkeyKey = _curKey;
            _currentConfig.HotkeyDisplayText = _curDisplayText;

            // Preferences
            _currentConfig.Language = _curLanguage;
            _currentConfig.Theme = _curTheme;

            ConfigManager.Save(_currentConfig);
            _onConfigSaved(_currentConfig);

            Close();
        }
    }
}
