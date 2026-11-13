using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using QuickAudioSwitcher.Core;

namespace QuickAudioSwitcher.UI
{
    public partial class FlyoutWindow : Window
    {
        private readonly AudioEngine _audioEngine;
        private readonly Func<AppConfig> _getConfig;
        private readonly Action _openSettings;
        private readonly DispatcherTimer _refreshDebounceTimer;
        private bool _isUpdatingVolumeFromCode;
        private string? _selectedDeviceId;

        public FlyoutWindow(AudioEngine audioEngine, Func<AppConfig> getConfig, Action openSettings)
        {
            _isUpdatingVolumeFromCode = true;
            InitializeComponent();
            _isUpdatingVolumeFromCode = false;

            _audioEngine = audioEngine;
            _getConfig = getConfig;
            _openSettings = openSettings;

            _refreshDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _refreshDebounceTimer.Tick += (s, e) =>
            {
                _refreshDebounceTimer.Stop();
                RefreshDevices();
            };

            Deactivated += (s, e) => Hide();

            _audioEngine.DefaultDeviceChanged += (id) => Dispatcher.Invoke(() =>
            {
                _refreshDebounceTimer.Stop();
                _refreshDebounceTimer.Start();
            });

            _audioEngine.AudioEndpointsChanged += () => Dispatcher.Invoke(() =>
            {
                _refreshDebounceTimer.Stop();
                _refreshDebounceTimer.Start();
            });

            UpdateLocalization();
            Loc.LanguageChanged += UpdateLocalization;
            ThemeManager.ThemeChanged += () => Dispatcher.Invoke(() => { RefreshDevices(); RefreshVolume(); });
        }

        private void UpdateLocalization()
        {
            TxtHeaderTitle.Text = Loc.T("FlyoutTitle");
            BtnSettings.Content = Loc.T("BtnSettings");
            TxtNoFavoritesTitle.Text = Loc.T("NoFavoritesTitle");
            TxtNoFavoritesHint.Text = Loc.T("NoFavoritesHint");
            BtnMute.ToolTip = Loc.T("MuteTooltip");
        }

        public void ShowNear(System.Drawing.Point cursorPoint)
        {
            RefreshDevices();
            RefreshVolume();

            // Force layout pass to calculate true DesiredSize before showing
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));

            double winWidth = DesiredSize.Width > 0 ? DesiredSize.Width : 340;
            double winHeight = DesiredSize.Height > 0 ? DesiredSize.Height : 320;

            var screen = System.Windows.Forms.Screen.FromPoint(cursorPoint);
            var dpi = VisualTreeHelper.GetDpi(this);

            double workLeft = screen.WorkingArea.Left / dpi.DpiScaleX;
            double workTop = screen.WorkingArea.Top / dpi.DpiScaleY;
            double workRight = screen.WorkingArea.Right / dpi.DpiScaleX;
            double workBottom = screen.WorkingArea.Bottom / dpi.DpiScaleY;

            double cursorX = cursorPoint.X / dpi.DpiScaleX;
            double cursorY = cursorPoint.Y / dpi.DpiScaleY;

            double targetX = cursorX - (winWidth / 2);
            double targetY = cursorY - winHeight - 12;

            // Keep within current monitor working area
            if (targetX + winWidth > workRight - 8)
                targetX = workRight - winWidth - 8;
            if (targetX < workLeft + 8)
                targetX = workLeft + 8;

            if (targetY < workTop + 8)
                targetY = cursorY + 12; // In case taskbar is at top
            if (targetY + winHeight > workBottom - 8)
                targetY = workBottom - winHeight - 8;

            Left = targetX;
            Top = targetY;

            Show();
            Activate();
        }

        public void RefreshDevices()
        {
            var config = _getConfig();
            var allDevices = _audioEngine.GetPlaybackDevices();
            var defaultId = _selectedDeviceId ?? _audioEngine.GetDefaultDeviceId();

            PnlFavorites.Children.Clear();

            // Match favorites from config
            var favoriteList = new List<AudioDevice>();
            foreach (var dev in allDevices)
            {
                var pref = config.Devices.FirstOrDefault(p => string.Equals(p.Id, dev.Id, StringComparison.OrdinalIgnoreCase));
                if (pref != null && pref.IsFavorite)
                {
                    dev.IsFavorite = true;
                    dev.CustomName = pref.CustomName;
                    dev.IsDefault = string.Equals(dev.Id, defaultId, StringComparison.OrdinalIgnoreCase);
                    favoriteList.Add(dev);
                }
            }

            if (favoriteList.Count == 0)
            {
                PnlNoFavorites.Visibility = Visibility.Visible;
                return;
            }

            PnlNoFavorites.Visibility = Visibility.Collapsed;

            foreach (var dev in favoriteList)
            {
                var itemBorder = CreateDeviceItem(dev);
                PnlFavorites.Children.Add(itemBorder);
            }

            RefreshVolume();
        }

        private static (Brush bg, Brush border, Brush title, Brush subtitle, Brush accent, Brush ledInactive, Brush iconInactive) GetDeviceItemBrushes(bool isSelected)
        {
            if (ThemeManager.IsDark)
            {
                return (
                    bg: isSelected ? new SolidColorBrush(Color.FromRgb(19, 37, 55)) : new SolidColorBrush(Color.FromRgb(26, 29, 36)),
                    border: isSelected ? new SolidColorBrush(Color.FromRgb(0, 229, 255)) : new SolidColorBrush(Color.FromRgb(40, 45, 57)),
                    title: isSelected ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                    subtitle: new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                    accent: new SolidColorBrush(Color.FromRgb(0, 229, 255)),
                    ledInactive: new SolidColorBrush(Color.FromRgb(51, 58, 74)),
                    iconInactive: new SolidColorBrush(Color.FromRgb(100, 116, 139))
                );
            }
            else
            {
                return (
                    bg: isSelected ? new SolidColorBrush(Color.FromRgb(224, 242, 254)) : new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    border: isSelected ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                    title: isSelected ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                    subtitle: new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                    accent: new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                    ledInactive: new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                    iconInactive: new SolidColorBrush(Color.FromRgb(100, 116, 139))
                );
            }
        }

        private Border CreateDeviceItem(AudioDevice device)
        {
            var brushes = GetDeviceItemBrushes(device.IsDefault);

            var border = new Border
            {
                Tag = device.Id,
                Background = brushes.bg,
                BorderBrush = brushes.border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(7),
                Margin = new Thickness(0, 0, 0, 6),
                Padding = new Thickness(12, 9, 12, 9),
                Cursor = Cursors.Hand,
                SnapsToDevicePixels = true
            };

            border.MouseEnter += (s, e) =>
            {
                bool isSelected = string.Equals(border.Tag as string, _selectedDeviceId, StringComparison.OrdinalIgnoreCase);
                if (!isSelected)
                {
                    border.Background = ThemeManager.IsDark ? new SolidColorBrush(Color.FromRgb(36, 40, 51)) : new SolidColorBrush(Color.FromRgb(241, 245, 249));
                    border.BorderBrush = ThemeManager.IsDark ? new SolidColorBrush(Color.FromRgb(59, 67, 86)) : new SolidColorBrush(Color.FromRgb(148, 163, 184));
                }
            };
            border.MouseLeave += (s, e) =>
            {
                bool isSelected = string.Equals(border.Tag as string, _selectedDeviceId, StringComparison.OrdinalIgnoreCase);
                if (!isSelected)
                {
                    var b = GetDeviceItemBrushes(false);
                    border.Background = b.bg;
                    border.BorderBrush = b.border;
                }
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });

            // Left icon & LED
            var iconStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            var led = new System.Windows.Shapes.Ellipse
            {
                Tag = "LedIndicator",
                Width = 6,
                Height = 6,
                Margin = new Thickness(0, 0, 6, 0),
                Fill = device.IsDefault ? brushes.accent : brushes.ledInactive,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconStack.Children.Add(led);

            var icon = new TextBlock
            {
                Tag = "SpeakerIcon",
                Text = "🔊",
                FontSize = 14,
                Foreground = device.IsDefault ? brushes.accent : brushes.iconInactive,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconStack.Children.Add(icon);
            Grid.SetColumn(iconStack, 0);
            grid.Children.Add(iconStack);

            // Names
            var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0) };
            var title = new TextBlock
            {
                Tag = "TitleText",
                Text = device.DisplayName,
                FontSize = 13,
                FontWeight = device.IsDefault ? FontWeights.SemiBold : FontWeights.Normal,
                Foreground = brushes.title,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            var subtitle = new TextBlock
            {
                Tag = "SubtitleText",
                Text = !string.IsNullOrWhiteSpace(device.CustomName) ? $"{device.SystemName} ({device.AdapterName})" : device.AdapterName,
                FontSize = 11,
                Foreground = brushes.subtitle,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 2, 0, 0)
            };
            textStack.Children.Add(title);
            textStack.Children.Add(subtitle);
            Grid.SetColumn(textStack, 1);
            grid.Children.Add(textStack);

            // Active indicator
            var check = new TextBlock
            {
                Tag = "CheckIndicator",
                Text = "✓",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = brushes.accent,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = device.IsDefault ? Visibility.Visible : Visibility.Collapsed
            };
            Grid.SetColumn(check, 2);
            grid.Children.Add(check);

            border.Child = grid;

            border.MouseLeftButtonDown += (s, e) =>
            {
                _selectedDeviceId = device.Id;
                HighlightSelection(device.Id);

                // Update volume immediately using target device's volume
                float targetVol = _audioEngine.GetDeviceVolume(device.Id);
                _isUpdatingVolumeFromCode = true;
                SliderVolume.Value = (int)Math.Round(targetVol * 100);
                TxtVolume.Text = $"{(int)Math.Round(targetVol * 100)}%";
                _isUpdatingVolumeFromCode = false;

                // Perform switch asynchronously in background without freezing UI
                _ = _audioEngine.SetDefaultDeviceAsync(device.Id);
            };

            return border;
        }

        private void HighlightSelection(string selectedId)
        {
            foreach (UIElement child in PnlFavorites.Children)
            {
                if (child is Border b)
                {
                    bool isSelected = string.Equals(b.Tag as string, selectedId, StringComparison.OrdinalIgnoreCase);
                    var brushes = GetDeviceItemBrushes(isSelected);

                    b.Background = brushes.bg;
                    b.BorderBrush = brushes.border;

                    if (b.Child is Grid g)
                    {
                        foreach (UIElement gridChild in g.Children)
                        {
                            if (gridChild is StackPanel iconSp)
                            {
                                foreach (UIElement subChild in iconSp.Children)
                                {
                                    if (subChild is System.Windows.Shapes.Ellipse led && (string?)led.Tag == "LedIndicator")
                                    {
                                        led.Fill = isSelected ? brushes.accent : brushes.ledInactive;
                                    }
                                    else if (subChild is TextBlock tbIcon && (string?)tbIcon.Tag == "SpeakerIcon")
                                    {
                                        tbIcon.Foreground = isSelected ? brushes.accent : brushes.iconInactive;
                                    }
                                }
                            }
                            else if (gridChild is TextBlock tb)
                            {
                                if ((string?)tb.Tag == "CheckIndicator")
                                {
                                    tb.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;
                                    tb.Foreground = brushes.accent;
                                }
                            }
                            else if (gridChild is StackPanel sp)
                            {
                                foreach (UIElement spChild in sp.Children)
                                {
                                    if (spChild is TextBlock tbTitle && (string?)tbTitle.Tag == "TitleText")
                                    {
                                        tbTitle.FontWeight = isSelected ? FontWeights.SemiBold : FontWeights.Normal;
                                        tbTitle.Foreground = brushes.title;
                                    }
                                    else if (spChild is TextBlock tbSub && (string?)tbSub.Tag == "SubtitleText")
                                    {
                                        tbSub.Foreground = brushes.subtitle;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RefreshVolume()
        {
            _isUpdatingVolumeFromCode = true;
            try
            {
                float vol = _audioEngine.GetCurrentMasterVolume();
                bool mute = _audioEngine.GetCurrentMute();

                SliderVolume.Value = (int)Math.Round(vol * 100);
                TxtVolume.Text = $"{(int)Math.Round(vol * 100)}%";
                BtnMute.Content = mute ? "🔇" : "🔊";

                Color activeColor = ThemeManager.IsDark ? Color.FromRgb(0, 229, 255) : Color.FromRgb(2, 132, 199);
                Color muteColor = ThemeManager.IsDark ? Color.FromRgb(255, 183, 3) : Color.FromRgb(217, 119, 6);

                BtnMute.Foreground = new SolidColorBrush(mute ? muteColor : activeColor);
            }
            finally
            {
                _isUpdatingVolumeFromCode = false;
            }
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUpdatingVolumeFromCode || TxtVolume == null || SliderVolume == null || _audioEngine == null) return;

            float newVol = (float)(SliderVolume.Value / 100.0);
            TxtVolume.Text = $"{(int)SliderVolume.Value}%";
            _audioEngine.SetCurrentMasterVolume(newVol);
        }

        private void BtnMute_Click(object sender, RoutedEventArgs e)
        {
            bool current = _audioEngine.GetCurrentMute();
            _audioEngine.SetCurrentMute(!current);
            RefreshVolume();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            _openSettings();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
