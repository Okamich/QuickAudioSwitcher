using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace QuickAudioSwitcher.Core
{
    public class HotkeyManager : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9001;

        private const int WH_MOUSE_LL = 14;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_XBUTTONDOWN = 0x020B;

        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;
        public const uint MOD_NOREPEAT = 0x4000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private HwndSource? _hwndSource;
        private IntPtr _mouseHookId = IntPtr.Zero;
        private LowLevelMouseProc? _mouseProc;

        private bool _isRegistered;
        private bool _isMouseBinding;
        private int _targetMouseButton; // 3 = Middle, 4 = XButton1, 5 = XButton2
        private uint _targetModifiers;
        private uint _targetKey;

        public event Action? HotkeyPressed;
        public bool IsRegistered => _isRegistered;

        public HotkeyManager()
        {
            var parameters = new HwndSourceParameters("QuickAudioSwitcherHotkeyListener")
            {
                WindowStyle = 0,
                ExtendedWindowStyle = 0
            };
            parameters.SetPosition(0, 0);
            parameters.SetSize(0, 0);
            _hwndSource = new HwndSource(parameters);
            _hwndSource.AddHook(HwndHook);

            _mouseProc = MouseHookCallback;
        }

        public bool RegisterKeyboard(uint modifiers, uint key)
        {
            Unregister();
            if (_hwndSource == null) return false;

            _isMouseBinding = false;
            _targetModifiers = modifiers;
            _targetKey = key;

            uint combined = modifiers | MOD_NOREPEAT;
            _isRegistered = RegisterHotKey(_hwndSource.Handle, HOTKEY_ID, combined, key);
            if (!_isRegistered)
            {
                _isRegistered = RegisterHotKey(_hwndSource.Handle, HOTKEY_ID, modifiers, key);
            }

            return _isRegistered;
        }

        public bool RegisterMouse(int mouseButton, uint modifiers)
        {
            Unregister();

            _isMouseBinding = true;
            _targetMouseButton = mouseButton;
            _targetModifiers = modifiers;

            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            IntPtr moduleHandle = GetModuleHandle(curModule?.ModuleName);

            _mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc!, moduleHandle, 0);
            _isRegistered = _mouseHookId != IntPtr.Zero;
            return _isRegistered;
        }

        public void Unregister()
        {
            if (_hwndSource != null)
            {
                UnregisterHotKey(_hwndSource.Handle, HOTKEY_ID);
            }

            if (_mouseHookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHookId);
                _mouseHookId = IntPtr.Zero;
            }

            _isRegistered = false;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(_ => HotkeyPressed?.Invoke());
                handled = true;
            }
            return IntPtr.Zero;
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _isMouseBinding)
            {
                int msg = wParam.ToInt32();
                if (msg == WM_XBUTTONDOWN || msg == WM_MBUTTONDOWN)
                {
                    var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    int clickedButton = 0;

                    if (msg == WM_MBUTTONDOWN)
                    {
                        clickedButton = 3;
                    }
                    else if (msg == WM_XBUTTONDOWN)
                    {
                        uint xbutton = (hookStruct.mouseData >> 16) & 0xFFFF;
                        clickedButton = (xbutton == 1) ? 4 : 5;
                    }

                    if (clickedButton == _targetMouseButton && CheckModifiersMatch(_targetModifiers))
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem(_ => HotkeyPressed?.Invoke());
                    }
                }
            }

            return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
        }

        private static bool CheckModifiersMatch(uint targetModifiers)
        {
            bool needCtrl = (targetModifiers & MOD_CONTROL) != 0;
            bool needAlt = (targetModifiers & MOD_ALT) != 0;
            bool needShift = (targetModifiers & MOD_SHIFT) != 0;

            bool isCtrl = (GetKeyState(0x11) & 0x8000) != 0;
            bool isAlt = (GetKeyState(0x12) & 0x8000) != 0;
            bool isShift = (GetKeyState(0x10) & 0x8000) != 0;

            return (needCtrl == isCtrl) && (needAlt == isAlt) && (needShift == isShift);
        }

        public void Dispose()
        {
            Unregister();
            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(HwndHook);
                _hwndSource.Dispose();
                _hwndSource = null;
            }
        }
    }
}
