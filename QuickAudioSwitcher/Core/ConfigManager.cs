using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;

namespace QuickAudioSwitcher.Core
{
    public class DevicePreference
    {
        public string Id { get; set; } = string.Empty;
        public string CustomName { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        public int Order { get; set; }
    }

    public class AppConfig
    {
        public List<DevicePreference> Devices { get; set; } = new();
        public bool HotkeyEnabled { get; set; } = true;
        public bool HotkeyIsMouse { get; set; } = false;
        public int HotkeyMouseButton { get; set; } = 4; // 3 = Middle, 4 = Mouse 4, 5 = Mouse 5
        public uint HotkeyModifiers { get; set; } = 0x0001 | 0x0002; // Ctrl + Alt
        public uint HotkeyKey { get; set; } = 0x41; // 'A'
        public string HotkeyDisplayText { get; set; } = "Ctrl + Alt + A";
        public bool AutostartEnabled { get; set; }
        public bool ShowOsd { get; set; } = true;
        public string Language { get; set; } = "ru";
        public string Theme { get; set; } = "Dark";
    }

    public static class ConfigManager
    {
        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "QuickAudioSwitcher");

        private static readonly string ConfigFilePath = Path.Combine(AppDataFolder, "config.json");
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "QuickAudioSwitcher";

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    if (config != null)
                    {
                        config.AutostartEnabled = IsAutostartEnabled();
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load config: {ex.Message}");
            }

            var defaultCfg = new AppConfig();
            defaultCfg.AutostartEnabled = IsAutostartEnabled();
            return defaultCfg;
        }

        public static void Save(AppConfig config)
        {
            try
            {
                if (!Directory.Exists(AppDataFolder))
                {
                    Directory.CreateDirectory(AppDataFolder);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigFilePath, json);

                SetAutostart(config.AutostartEnabled);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
            }
        }

        public static bool IsAutostartEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }

        public static void SetAutostart(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
                if (key == null) return;

                if (enable)
                {
                    string? exePath = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue(AppName, $"\"{exePath}\" --minimized");
                    }
                }
                else
                {
                    if (key.GetValue(AppName) != null)
                    {
                        key.DeleteValue(AppName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to configure autostart in registry: {ex.Message}");
            }
        }
    }
}
