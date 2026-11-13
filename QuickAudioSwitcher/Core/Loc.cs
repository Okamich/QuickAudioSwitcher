using System;
using System.Collections.Generic;

namespace QuickAudioSwitcher.Core
{
    public static class Loc
    {
        public static string CurrentLanguage { get; private set; } = "ru";

        public static event Action? LanguageChanged;

        private static readonly Dictionary<string, Dictionary<string, string>> Translations = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ru"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Flyout
                ["FlyoutTitle"] = "АУДИОВЫХОД",
                ["BtnSettings"] = "⚙ Настройки",
                ["NoFavoritesTitle"] = "⚡ Нет избранных выходов",
                ["NoFavoritesHint"] = "Откройте «⚙ Настройки» и отметьте нужные устройства звёздочкой для быстрого переключения.",
                ["MuteTooltip"] = "Включить / отключить звук",

                // OSD
                ["OsdTitle"] = "ВЫХОД ЗВУКА",
                ["OsdVolume"] = "ГРОМКОСТЬ",

                // Settings Header
                ["SettingsWindowTitle"] = "Настройки QuickAudioSwitcher",
                ["SettingsHeaderTitle"] = "Конфигурация аудиовыходов и управления",
                ["SettingsHeaderSubtitle"] = "Отметьте выходы для ротации, задайте понятные псевдонимы (например, «Монитор Левый») и протестируйте звук кнопкой «▶ Тест».",

                // Settings Table Headers
                ["ColRotation"] = "РОТАЦИЯ",
                ["ColDevice"] = "УСТРОЙСТВО И ДРАЙВЕР",
                ["ColCustomName"] = "ПОЛЬЗОВАТЕЛЬСКОЕ ИМЯ",
                ["ColTest"] = "ПРОВЕРКА",

                // Device Row
                ["FavCheckbox"] = "★ В ротацию",
                ["CustomNameTooltip"] = "Задайте удобное имя, например: «Монитор Левый», «Монитор Правый» или «Колонки»",
                ["BtnTest"] = "▶ Тест",
                ["BtnTesting"] = "🔊 Звучит...",

                // Hotkey Section
                ["HotkeyTitle"] = "Глобальный хоткей / Кнопка мыши",
                ["ChkEnableHotkey"] = "Включить быстрое переключение",
                ["BtnReset"] = "Сброс",
                ["BtnResetTooltip"] = "Сбросить на значение по умолчанию (Ctrl + Alt + A)",
                ["TxtHotkeyHint"] = "Кликните на поле выше и нажмите любую комбинацию клавиш или боковую кнопку мыши (Mouse 4 / Mouse 5).",
                ["ListeningHotkey"] = "🔴 Слушаю... Нажмите клавишу или мышь (Esc - отмена)",

                // System Section
                ["SystemTitle"] = "Системная интеграция",
                ["ChkAutostart"] = "Запускать вместе с Windows (автозапуск)",
                ["ChkShowOsd"] = "Показывать OSD-плашку на экране при смене",
                ["TrayNotice"] = "QuickAudioSwitcher работает в фоновом режиме в системном трее возле часов.",

                // Preferences (Lang & Theme)
                ["PreferencesTitle"] = "Язык и внешний вид",
                ["LabelLanguage"] = "Язык интерфейса:",
                ["LabelTheme"] = "Тема оформления:",
                ["ThemeDark"] = "Тёмная (Dark)",
                ["ThemeLight"] = "Светлая (Light)",

                // Footer Buttons
                ["BtnRefresh"] = "🔄 Обновить список",
                ["BtnCancel"] = "Отмена",
                ["BtnSave"] = "💾 Сохранить и применить",

                // Tray Menu
                ["TrayMenuFlyout"] = "🎛  Выбор аудиовыхода",
                ["TrayMenuCycle"] = "🔁  Следующее устройство",
                ["TrayMenuSettings"] = "⚙  Настройки...",
                ["TrayMenuExit"] = "✕  Выход",
                ["TrayTooltipPrefix"] = "Звук: ",
            },
            ["en"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Flyout
                ["FlyoutTitle"] = "AUDIO OUTPUT",
                ["BtnSettings"] = "⚙ Settings",
                ["NoFavoritesTitle"] = "⚡ No favorite outputs",
                ["NoFavoritesHint"] = "Open \"⚙ Settings\" and star your desired audio endpoints for quick switching.",
                ["MuteTooltip"] = "Mute / Unmute",

                // OSD
                ["OsdTitle"] = "AUDIO OUTPUT",
                ["OsdVolume"] = "VOLUME",

                // Settings Header
                ["SettingsWindowTitle"] = "QuickAudioSwitcher Settings",
                ["SettingsHeaderTitle"] = "Audio Outputs & Switching Configuration",
                ["SettingsHeaderSubtitle"] = "Star playback devices to cycle, assign friendly aliases (e.g., \"Left Monitor\"), and test sound with \"▶ Test\".",

                // Settings Table Headers
                ["ColRotation"] = "ROTATION",
                ["ColDevice"] = "DEVICE & DRIVER",
                ["ColCustomName"] = "CUSTOM ALIAS",
                ["ColTest"] = "TEST",

                // Device Row
                ["FavCheckbox"] = "★ Favorite",
                ["CustomNameTooltip"] = "Set a friendly alias, e.g.: \"Left Monitor\", \"Right Monitor\", or \"Speakers\"",
                ["BtnTest"] = "▶ Test",
                ["BtnTesting"] = "🔊 Playing...",

                // Hotkey Section
                ["HotkeyTitle"] = "Global Shortcut / Mouse Button",
                ["ChkEnableHotkey"] = "Enable rapid audio switching",
                ["BtnReset"] = "Reset",
                ["BtnResetTooltip"] = "Reset to default shortcut (Ctrl + Alt + A)",
                ["TxtHotkeyHint"] = "Click the button above and press any key combination or side mouse button (Mouse 4 / Mouse 5).",
                ["ListeningHotkey"] = "🔴 Listening... Press key or mouse button (Esc to cancel)",

                // System Section
                ["SystemTitle"] = "System Integration",
                ["ChkAutostart"] = "Launch on Windows startup (Autostart)",
                ["ChkShowOsd"] = "Show on-screen HUD (OSD) upon switching",
                ["TrayNotice"] = "QuickAudioSwitcher runs silently in the system tray near the clock.",

                // Preferences (Lang & Theme)
                ["PreferencesTitle"] = "Language & Appearance",
                ["LabelLanguage"] = "Language:",
                ["LabelTheme"] = "Color theme:",
                ["ThemeDark"] = "Dark Studio",
                ["ThemeLight"] = "Light Studio",

                // Footer Buttons
                ["BtnRefresh"] = "🔄 Refresh devices",
                ["BtnCancel"] = "Cancel",
                ["BtnSave"] = "💾 Save & Apply",

                // Tray Menu
                ["TrayMenuFlyout"] = "🎛  Select Audio Output",
                ["TrayMenuCycle"] = "🔁  Next Device",
                ["TrayMenuSettings"] = "⚙  Settings...",
                ["TrayMenuExit"] = "✕  Exit",
                ["TrayTooltipPrefix"] = "Audio: ",
            }
        };

        public static void SetLanguage(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang)) lang = "ru";
            lang = lang.ToLowerInvariant();
            if (lang != "ru" && lang != "en") lang = "ru";

            if (CurrentLanguage != lang)
            {
                CurrentLanguage = lang;
                LanguageChanged?.Invoke();
            }
        }

        public static string T(string key)
        {
            if (Translations.TryGetValue(CurrentLanguage, out var dict) && dict.TryGetValue(key, out var val))
            {
                return val;
            }

            // Fallback to English, then key itself
            if (Translations["en"].TryGetValue(key, out var fallbackVal))
            {
                return fallbackVal;
            }

            return key;
        }
    }
}
