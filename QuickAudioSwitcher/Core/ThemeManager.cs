using System;
using System.Windows;
using System.Windows.Media;

namespace QuickAudioSwitcher.Core
{
    public static class ThemeManager
    {
        public static string CurrentTheme { get; private set; } = "Dark";

        public static event Action? ThemeChanged;

        public static bool IsDark => string.Equals(CurrentTheme, "Dark", StringComparison.OrdinalIgnoreCase);

        public static void ApplyTheme(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName)) themeName = "Dark";
            themeName = string.Equals(themeName, "Light", StringComparison.OrdinalIgnoreCase) ? "Light" : "Dark";

            CurrentTheme = themeName;
            bool isDark = IsDark;

            var app = Application.Current;
            if (app != null)
            {
                // Theme Palette:
                // Dark: Dark Studio Obsidian & Electric Cyan
                // Light: Precision Studio Silver/Chalk & Vibrant Marine Blue
                Color bgDark = isDark
                    ? Color.FromRgb(16, 18, 22)       // Deep obsidian canvas
                    : Color.FromRgb(237, 240, 244);   // Precision cool chalk/silver canvas

                Color containerBg = isDark
                    ? Color.FromRgb(22, 25, 34)       // Dark rack enclosure
                    : Color.FromRgb(248, 250, 252);   // Clean light rack enclosure

                Color cardBg = isDark
                    ? Color.FromRgb(28, 32, 44)       // Brushed dark tile
                    : Color.FromRgb(255, 255, 255);   // Pure crisp white tile

                Color cardHover = isDark
                    ? Color.FromRgb(36, 42, 58)       // Dark hover tile
                    : Color.FromRgb(241, 245, 249);   // Light hover tile

                Color cardActive = isDark
                    ? Color.FromRgb(19, 43, 64)       // Deep cyan-navy active
                    : Color.FromRgb(224, 242, 254);   // Vibrant sky-cyan active

                Color border = isDark
                    ? Color.FromRgb(40, 47, 62)       // Hairline dark border
                    : Color.FromRgb(203, 213, 225);   // Hairline light border (Slate-300)

                Color borderHighlight = isDark
                    ? Color.FromRgb(58, 68, 90)       // Highlight border
                    : Color.FromRgb(148, 163, 184);   // Highlight border (Slate-400)

                Color inputBg = isDark
                    ? Color.FromRgb(18, 20, 26)       // Dark input field
                    : Color.FromRgb(255, 255, 255);   // Light input field

                Color inputBorder = isDark
                    ? Color.FromRgb(46, 53, 68)       // Dark input border
                    : Color.FromRgb(203, 213, 225);   // Light input border

                Color buttonBg = isDark
                    ? Color.FromRgb(28, 33, 44)       // Dark button background
                    : Color.FromRgb(255, 255, 255);   // Light button background

                Color buttonBorder = isDark
                    ? Color.FromRgb(51, 60, 78)       // Dark button border
                    : Color.FromRgb(203, 213, 225);   // Light button border

                Color buttonHoverBg = isDark
                    ? Color.FromRgb(38, 45, 60)       // Dark button hover
                    : Color.FromRgb(241, 245, 249);   // Light button hover

                Color accentCyan = isDark
                    ? Color.FromRgb(0, 229, 255)      // Electric cyan
                    : Color.FromRgb(2, 132, 199);     // Vibrant marine blue (AAA contrast on white)

                Color accentBlue = isDark
                    ? Color.FromRgb(2, 132, 199)      // Studio blue
                    : Color.FromRgb(3, 105, 161);     // Deep studio navy

                Color accentAmber = isDark
                    ? Color.FromRgb(255, 183, 3)      // Neon amber
                    : Color.FromRgb(217, 119, 6);      // Dark readable amber

                Color textPrimary = isDark
                    ? Color.FromRgb(248, 250, 252)    // Crisp pure white (AAA)
                    : Color.FromRgb(15, 23, 42);       // Deepest slate ink (AAA 15:1 contrast)

                Color textSecondary = isDark
                    ? Color.FromRgb(148, 163, 184)    // Cool slate gray
                    : Color.FromRgb(51, 65, 85);       // Deep slate gray (AA 8:1 contrast)

                Color textMuted = isDark
                    ? Color.FromRgb(100, 116, 139)    // Slate-500
                    : Color.FromRgb(100, 116, 139);    // Slate-500

                // Update Application Resources
                app.Resources["StudioBgDarkColor"] = bgDark;
                app.Resources["StudioContainerBgColor"] = containerBg;
                app.Resources["StudioCardBgColor"] = cardBg;
                app.Resources["StudioCardHoverColor"] = cardHover;
                app.Resources["StudioCardActiveColor"] = cardActive;
                app.Resources["StudioBorderColor"] = border;
                app.Resources["StudioBorderHighlightColor"] = borderHighlight;
                app.Resources["StudioAccentCyanColor"] = accentCyan;
                app.Resources["StudioAccentBlueColor"] = accentBlue;
                app.Resources["StudioAccentAmberColor"] = accentAmber;
                app.Resources["StudioTextPrimaryColor"] = textPrimary;
                app.Resources["StudioTextSecondaryColor"] = textSecondary;
                app.Resources["StudioTextMutedColor"] = textMuted;

                app.Resources["StudioBgDarkBrush"] = new SolidColorBrush(bgDark);
                app.Resources["StudioContainerBgBrush"] = new SolidColorBrush(containerBg);
                app.Resources["StudioCardBgBrush"] = new SolidColorBrush(cardBg);
                app.Resources["StudioCardHoverBrush"] = new SolidColorBrush(cardHover);
                app.Resources["StudioCardActiveBrush"] = new SolidColorBrush(cardActive);
                app.Resources["StudioBorderBrush"] = new SolidColorBrush(border);
                app.Resources["StudioBorderHighlightBrush"] = new SolidColorBrush(borderHighlight);
                app.Resources["StudioInputBgBrush"] = new SolidColorBrush(inputBg);
                app.Resources["StudioInputBorderBrush"] = new SolidColorBrush(inputBorder);
                app.Resources["StudioButtonBgBrush"] = new SolidColorBrush(buttonBg);
                app.Resources["StudioButtonBorderBrush"] = new SolidColorBrush(buttonBorder);
                app.Resources["StudioButtonHoverBgBrush"] = new SolidColorBrush(buttonHoverBg);
                app.Resources["StudioAccentCyanBrush"] = new SolidColorBrush(accentCyan);
                app.Resources["StudioAccentBlueBrush"] = new SolidColorBrush(accentBlue);
                app.Resources["StudioAccentAmberBrush"] = new SolidColorBrush(accentAmber);
                app.Resources["StudioTextPrimaryBrush"] = new SolidColorBrush(textPrimary);
                app.Resources["StudioTextSecondaryBrush"] = new SolidColorBrush(textSecondary);
                app.Resources["StudioTextMutedBrush"] = new SolidColorBrush(textMuted);

                // Legacy compatibility
                app.Resources["BgDarkBrush"] = new SolidColorBrush(bgDark);
                app.Resources["BgCardBrush"] = new SolidColorBrush(cardBg);
                app.Resources["BgCardHoverBrush"] = new SolidColorBrush(cardHover);
                app.Resources["BgCardSelectedBrush"] = new SolidColorBrush(cardActive);
                app.Resources["BorderDarkBrush"] = new SolidColorBrush(border);
                app.Resources["AccentBlueBrush"] = new SolidColorBrush(accentCyan);
                app.Resources["TextMainBrush"] = new SolidColorBrush(textPrimary);
                app.Resources["TextMutedBrush"] = new SolidColorBrush(textSecondary);
            }

            ThemeChanged?.Invoke();
        }

        // WinForms / System.Drawing Helper Colors for Tray Context Menu
        public static System.Drawing.Color TrayMenuBg => IsDark
            ? System.Drawing.Color.FromArgb(18, 20, 26)
            : System.Drawing.Color.FromArgb(255, 255, 255);

        public static System.Drawing.Color TrayMenuBorder => IsDark
            ? System.Drawing.Color.FromArgb(40, 46, 58)
            : System.Drawing.Color.FromArgb(203, 213, 225);

        public static System.Drawing.Color TrayMenuHover => IsDark
            ? System.Drawing.Color.FromArgb(20, 40, 58)
            : System.Drawing.Color.FromArgb(224, 242, 254);

        public static System.Drawing.Color TrayMenuHoverBorder => IsDark
            ? System.Drawing.Color.FromArgb(0, 229, 255)
            : System.Drawing.Color.FromArgb(2, 132, 199);

        public static System.Drawing.Color TrayMenuSeparator => IsDark
            ? System.Drawing.Color.FromArgb(36, 42, 54)
            : System.Drawing.Color.FromArgb(226, 232, 240);

        public static System.Drawing.Color TrayMenuText => IsDark
            ? System.Drawing.Color.FromArgb(241, 245, 249)
            : System.Drawing.Color.FromArgb(15, 23, 42);

        public static System.Drawing.Color TrayMenuTextHover => IsDark
            ? System.Drawing.Color.FromArgb(0, 229, 255)
            : System.Drawing.Color.FromArgb(2, 132, 199);
    }
}
