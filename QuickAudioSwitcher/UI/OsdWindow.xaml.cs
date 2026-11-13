using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using QuickAudioSwitcher.Core;

namespace QuickAudioSwitcher.UI
{
    public partial class OsdWindow : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private readonly DispatcherTimer _hideTimer;
        private readonly DoubleAnimation _fadeOutAnim;

        public OsdWindow()
        {
            InitializeComponent();

            _hideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1400) };
            _hideTimer.Tick += (s, e) =>
            {
                _hideTimer.Stop();
                BeginAnimation(OpacityProperty, _fadeOutAnim);
            };

            _fadeOutAnim = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(250));
            _fadeOutAnim.Completed += (s, e) =>
            {
                Hide();
            };

            UpdateLocalization();
            Loc.LanguageChanged += UpdateLocalization;
        }

        private void UpdateLocalization()
        {
            TxtOsdTitle.Text = Loc.T("OsdTitle");
            TxtOsdVolume.Text = Loc.T("OsdVolume");
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Set window styles to non-activating and click-through
            var handle = new WindowInteropHelper(this).Handle;
            int exStyle = GetWindowLong(handle, GWL_EXSTYLE);
            SetWindowLong(handle, GWL_EXSTYLE, exStyle | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
        }

        public void ShowDevice(string deviceName, int volumePercent)
        {
            TxtDeviceName.Text = deviceName;
            PbVolume.Value = Math.Clamp(volumePercent, 0, 100);
            TxtVolumePercent.Text = $"{volumePercent}%";

            // Force layout pass to calculate exact size for this text
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));

            double osdWidth = DesiredSize.Width;
            double osdHeight = DesiredSize.Height;

            // Get current active monitor where the mouse cursor is
            var screen = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);
            var dpi = VisualTreeHelper.GetDpi(this);

            double workLeft = screen.WorkingArea.Left / dpi.DpiScaleX;
            double workWidth = screen.WorkingArea.Width / dpi.DpiScaleX;
            double workBottom = screen.WorkingArea.Bottom / dpi.DpiScaleY;

            Left = workLeft + (workWidth - osdWidth) / 2;
            Top = workBottom - osdHeight - 50;

            _hideTimer.Stop();
            BeginAnimation(OpacityProperty, null);
            Opacity = 1.0;

            Show();
            _hideTimer.Start();
        }
    }
}
