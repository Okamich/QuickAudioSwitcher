using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using QuickAudioSwitcher.Core;

namespace QuickAudioSwitcher.UI
{
    public class TrayController : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly Action<Point> _onLeftClick;
        private readonly Action _onOpenSettings;
        private readonly Action _onCycleNext;
        private readonly Action _onExit;

        private readonly ToolStripMenuItem _itemFlyout;
        private readonly ToolStripMenuItem _itemCycle;
        private readonly ToolStripMenuItem _itemSettings;
        private readonly ToolStripMenuItem _itemExit;
        private readonly ContextMenuStrip _contextMenu;

        private string? _lastDeviceName;

        public TrayController(
            Action<Point> onLeftClick,
            Action onOpenSettings,
            Action onCycleNext,
            Action onExit)
        {
            _onLeftClick = onLeftClick;
            _onOpenSettings = onOpenSettings;
            _onCycleNext = onCycleNext;
            _onExit = onExit;

            _notifyIcon = new NotifyIcon
            {
                Text = "QuickAudioSwitcher",
                Visible = true,
                Icon = CreateSpeakerIcon()
            };

            _contextMenu = new ContextMenuStrip
            {
                ShowImageMargin = false,
                ShowCheckMargin = false,
                Renderer = new DarkStudioMenuRenderer(),
                BackColor = ThemeManager.TrayMenuBg,
                ForeColor = ThemeManager.TrayMenuText,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Padding = new Padding(4)
            };

            _itemFlyout = new ToolStripMenuItem(Loc.T("TrayMenuFlyout"), null, (s, e) => _onLeftClick(Cursor.Position))
            {
                Padding = new Padding(12, 6, 16, 6),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            _itemCycle = new ToolStripMenuItem(Loc.T("TrayMenuCycle"), null, (s, e) => _onCycleNext())
            {
                Padding = new Padding(12, 6, 16, 6)
            };

            _itemSettings = new ToolStripMenuItem(Loc.T("TrayMenuSettings"), null, (s, e) => _onOpenSettings())
            {
                Padding = new Padding(12, 6, 16, 6)
            };

            _itemExit = new ToolStripMenuItem(Loc.T("TrayMenuExit"), null, (s, e) => _onExit())
            {
                Padding = new Padding(12, 6, 16, 6)
            };

            _contextMenu.Items.Add(_itemFlyout);
            _contextMenu.Items.Add(_itemCycle);
            _contextMenu.Items.Add(_itemSettings);
            _contextMenu.Items.Add(new ToolStripSeparator { Margin = new Padding(4, 3, 4, 3) });
            _contextMenu.Items.Add(_itemExit);

            _notifyIcon.ContextMenuStrip = _contextMenu;

            Loc.LanguageChanged += OnLanguageChanged;
            ThemeManager.ThemeChanged += OnThemeChanged;

            _notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _onLeftClick(Cursor.Position);
                }
            };

            _notifyIcon.DoubleClick += (s, e) =>
            {
                _onOpenSettings();
            };
        }

        private void OnLanguageChanged()
        {
            _itemFlyout.Text = Loc.T("TrayMenuFlyout");
            _itemCycle.Text = Loc.T("TrayMenuCycle");
            _itemSettings.Text = Loc.T("TrayMenuSettings");
            _itemExit.Text = Loc.T("TrayMenuExit");

            UpdateTooltip(_lastDeviceName);
        }

        private void OnThemeChanged()
        {
            _contextMenu.BackColor = ThemeManager.TrayMenuBg;
            _contextMenu.ForeColor = ThemeManager.TrayMenuText;
            _contextMenu.Invalidate();
        }

        public void UpdateTooltip(string? currentDeviceName)
        {
            _lastDeviceName = currentDeviceName;
            try
            {
                string text = string.IsNullOrWhiteSpace(currentDeviceName)
                    ? "QuickAudioSwitcher"
                    : $"{Loc.T("TrayTooltipPrefix")}{currentDeviceName}";

                // Windows NotifyIcon Text property is limited to 63 characters
                if (text.Length > 63)
                {
                    text = text.Substring(0, 60) + "...";
                }
                _notifyIcon.Text = text;
            }
            catch
            {
                // Fallback
            }
        }

        private static Icon CreateSpeakerIcon()
        {
            try
            {
                // 1. Try extracting icon from executable
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    var icon = Icon.ExtractAssociatedIcon(exePath);
                    if (icon != null) return icon;
                }
            }
            catch { }

            try
            {
                // 2. Try loading from Resources/app.ico
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "app.ico");
                if (File.Exists(iconPath))
                {
                    return new Icon(iconPath, 32, 32);
                }
            }
            catch { }

            // 3. High-contrast procedural studio speaker fallback
            using var bmp = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using var brush = new SolidBrush(Color.FromArgb(0, 229, 255));
                using var pen = new Pen(Color.FromArgb(0, 229, 255), 2.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round };

                g.FillRectangle(brush, 4, 11, 7, 10);

                var conePoints = new PointF[]
                {
                    new PointF(10, 11),
                    new PointF(18, 5),
                    new PointF(18, 27),
                    new PointF(10, 21)
                };
                g.FillPolygon(brush, conePoints);

                g.DrawArc(pen, 16, 10, 8, 12, -45, 90);
                g.DrawArc(pen, 19, 6, 11, 20, -50, 100);
            }

            return Icon.FromHandle(bmp.GetHicon());
        }

        public void Dispose()
        {
            Loc.LanguageChanged -= OnLanguageChanged;
            ThemeManager.ThemeChanged -= OnThemeChanged;
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        #region Studio Menu Custom Renderer
        private sealed class DarkStudioColorTable : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground => ThemeManager.TrayMenuBg;
            public override Color ImageMarginGradientBegin => ThemeManager.TrayMenuBg;
            public override Color ImageMarginGradientMiddle => ThemeManager.TrayMenuBg;
            public override Color ImageMarginGradientEnd => ThemeManager.TrayMenuBg;
            public override Color MenuBorder => ThemeManager.TrayMenuBorder;
            public override Color MenuItemBorder => ThemeManager.TrayMenuHoverBorder;
            public override Color MenuItemSelected => ThemeManager.TrayMenuHover;
            public override Color MenuItemSelectedGradientBegin => ThemeManager.TrayMenuHover;
            public override Color MenuItemSelectedGradientEnd => ThemeManager.TrayMenuHover;
            public override Color MenuItemPressedGradientBegin => ThemeManager.TrayMenuHover;
            public override Color MenuItemPressedGradientMiddle => ThemeManager.TrayMenuHover;
            public override Color MenuItemPressedGradientEnd => ThemeManager.TrayMenuHover;
            public override Color SeparatorDark => ThemeManager.TrayMenuSeparator;
            public override Color SeparatorLight => ThemeManager.TrayMenuSeparator;
        }

        private sealed class DarkStudioMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkStudioMenuRenderer() : base(new DarkStudioColorTable())
            {
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Selected)
                {
                    var rc = new Rectangle(2, 1, e.Item.Width - 4, e.Item.Height - 2);
                    using var brush = new SolidBrush(ThemeManager.TrayMenuHover);
                    using var pen = new Pen(ThemeManager.TrayMenuHoverBorder, 1f);
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    using var path = CreateRoundedRect(rc, 4);
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
                else
                {
                    base.OnRenderMenuItemBackground(e);
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Selected
                    ? ThemeManager.TrayMenuTextHover
                    : ThemeManager.TrayMenuText;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                using var pen = new Pen(ThemeManager.TrayMenuSeparator, 1f);
                int y = e.Item.Height / 2;
                e.Graphics.DrawLine(pen, 8, y, e.Item.Width - 8, y);
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                using var pen = new Pen(ThemeManager.TrayMenuBorder, 1f);
                e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            }

            private static GraphicsPath CreateRoundedRect(Rectangle bounds, int radius)
            {
                var path = new GraphicsPath();
                int d = radius * 2;
                path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
                path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
                path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
                path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                return path;
            }
        }
        #endregion
    }
}
