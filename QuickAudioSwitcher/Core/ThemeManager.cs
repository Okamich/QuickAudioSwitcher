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
                    ? Color.FromRgb(18, 19, 22)      // Deep obsidian
                    : Color.FromRgb(235, 238, 242);   // Precision chalk/silver enclosure

                Color cardBg = isDark
                    ? Color.FromRgb(26, 29, 36)      // Dark slate card
                    : Color.FromRgb(255, 255, 255);  // Pure crisp white card

                Color cardHover = isDark
                    ? Color.FromRgb(36, 40, 51)      // Slate card hover
                    : Color.FromRgb(243, 245, 249);  // Subtle soft hover

                Color cardActive = isDark
                    ? Color.FromRgb(19, 39, 56)      // Deep cyan-navy active
                    : Color.FromRgb(224, 242, 254);  // Sky-cyan active channel

                Color border = isDark
                    ? Color.FromRgb(40, 45, 57)      // Hairline dark border
                    : Color.FromRgb(215, 222, 232);  // Hairline crisp light border

                Color borderHighlight = isDark
                    ? Color.FromRgb(56, 63, 80)      // Highlight border
                    : Color.FromRgb(186, 197, 212);  // Light highlight border

                Color accentCyan = isDark
                    ? Color.FromRgb(0, 229, 255)     // Electric cyan
                    : Color.FromRgb(2, 132, 199);    // Vibrant marine blue (high contrast on white)

                Color accentBlue = isDark
                    ? Color.FromRgb(2, 132, 199)     // Studio blue
                    : Color.FromRgb(3, 105, 161);    // Deep studio navy

                Color accentAmber = isDark
                    ? Color.FromRgb(255, 183, 3)     // Neon amber
                    : Color.FromRgb(217, 119, 6);     // Dark amber

                Color textPrimary = isDark
                    ? Color.FromRgb(248, 250, 252)   // Crisp pure white
                    : Color.FromRgb(15, 23, 42);      // Deep ink slate (AAA contrast)

                Color textSecondary = isDark
                    ? Color.FromRgb(148, 163, 184)   // Cool muted gray
                    : Color.FromRgb(71, 85, 105);     // Dark slate gray (AA contrast 7.5:1)

                Color textMuted = isDark
                    ? Color.FromRgb(100, 116, 139)   // Subdued label
                    : Color.FromRgb(100, 116, 139);   // Subdued label (clear on light)

                Color shadowColor = isDark
                    ? Color.FromArgb(160, 0, 0, 0)
                    : Color.FromArgb(45, 71, 85, 105);

                // Update Application Resources
                app.Resources["StudioBgDarkColor"] = bgDark;
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
                app.Resources["StudioCardBgBrush"] = new SolidColorBrush(cardBg);
                app.Resources["StudioCardHoverBrush"] = new SolidColorBrush(cardHover);
                app.Resources["StudioCardActiveBrush"] = new SolidColorBrush(cardActive);
                app.Resources["StudioBorderBrush"] = new SolidColorBrush(border);
                app.Resources["StudioBorderHighlightBrush"] = new SolidColorBrush(borderHighlight);
                app.Resources["StudioAccentCyanBrush"] = new SolidColorBrush(accentCyan);
                app.Resources["StudioAccentBlueBrush"] = new SolidColorBrush(accentBlue);
                app.Resources["StudioAccentAmberBrush"] = new SolidColorBrush(accentAmber);
                app.Resources["StudioTextPrimaryBrush"] = new SolidColorBrush(textPrimary);
                app.Resources["StudioTextSecondaryBrush"] = new SolidColorBrush(textSecondary);
                app.Resources["StudioTextMutedBrush"] = new SolidColorBrush(textMuted);
                app.Resources["StudioShadowColor"] = shadowColor;

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
            : System.Drawing.Color.FromArgb(215, 222, 232);

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
