using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;
using static SyncIMEStatus.Win32Native;

namespace SyncIMEStatus
{
    public class SyncIme : INotifyPropertyChanged
    {
        public static readonly SyncIme Current = new SyncIme();

        private IntPtr _keyEventHookId = IntPtr.Zero;
        private static WindowsHookDelegate _keyEventHookProc;
        private static WinEventDelegate _winEventDelg;

        private IntPtr _winEventHookId = IntPtr.Zero;

        private IntPtr _focusEventHookId = IntPtr.Zero;

        private bool _imeStat;
        public bool ImeStat
        {
            get { return _imeStat; }
            set { _imeStat = value; }
        }

        private bool _hookEnabled = false;
        public bool HookEnabled
        {
            get { return _hookEnabled; }
            set { _hookEnabled = value; NotifyPropertyChanged(); }
        }

        private object _hookKeysLock = new object();
        private readonly List<HookKeyCode> _hookKeys = new List<HookKeyCode>();
        public List<HookKeyCode> HookKeys
        {
            get
            {
                lock (_hookKeysLock) { return _hookKeys; }
            }
        }

        public static void ResetHookKeys(IEnumerable<HookKeyCode> keyCodes)
        {
            lock (Current._hookKeysLock)
            {
                Current._hookKeys.Clear();
                Current._hookKeys.AddRange(keyCodes);
            }
            Current.NotifyPropertyChanged("HookKeys");
        }

        public static void AddHookKeys(IEnumerable<HookKeyCode> keyCodes)
        {
            lock (Current._hookKeysLock)
            {
                Current._hookKeys.AddRange(keyCodes);
            }
            Current.NotifyPropertyChanged("HookKeys");
        }

        private SyncIme()
        {
            _keyEventHookProc = LowLevelKeyboardProc;
            _winEventDelg = WinEventCallBack;
            //_lastHwndForeground = GetForegroundWindow();

            if (GetFocusedWindowImeStat(out bool imeEnabled))
            {
                _imeStat = imeEnabled;
            }
        }

        public bool BeginHook()
        {
            if (IntPtr.Zero == _keyEventHookId)
            {
                _keyEventHookId = SetLLKeyEventHook(_keyEventHookProc);
            }

            if (IntPtr.Zero == _winEventHookId)
            {
                _winEventHookId = SetForegroundEventHook(_winEventDelg);
            }


            if (IntPtr.Zero == _focusEventHookId)
            {
                _focusEventHookId = SetFocusEventHook(_winEventDelg);
            }

            if (IntPtr.Zero == _keyEventHookId || IntPtr.Zero == _winEventHookId)
            {
                return false;
            }
            HookEnabled = true;
            return true;
        }

        public bool EndHook()
        {
            try
            {
                _ = RemoveWinHookEx(_keyEventHookId);
                _keyEventHookId = IntPtr.Zero;

                RemoveWinEventHook(_winEventHookId);
                _winEventHookId = IntPtr.Zero;

                RemoveWinEventHook(_focusEventHookId);
                _focusEventHookId = IntPtr.Zero;

                HookEnabled = false;
            }
            catch
            {
                return false;
            }
            return true;
        }

        private IntPtr _lastHwndForeground;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void WinEventCallBack(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            //Debug.WriteLine($"eventType {eventType.ToString("X")}");
            /*
            if (_lastHwndForeground == hwnd) { return; }

            if (!GetWindowImeStat(_lastHwndForeground, out bool imeStat)) { return; }

            _lastHwndForeground = hwnd;

            //Debug.WriteLine($"Last Window {hwnd} IME {imeStat}");
            */
            SetFocusedWindowImeStat(_imeStat);
            SetWindowImeStat(hwnd, _imeStat);
            SetWindowImeStat(GetForegroundWindow(), _imeStat);
        }

        private IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) { return CallNextHookEx(_keyEventHookId, nCode, wParam, lParam); }

            int keyStat = (int)wParam;
            int keyCode = Marshal.ReadInt32(lParam);

            int result = 0;
            int count = _hookKeys.Count;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    if (_hookKeys[i].KeyCode == keyCode)
                    {
                        result += _hookKeys[i].KeyProc(keyCode, keyStat);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (result == 0)
                return CallNextHookEx(_keyEventHookId, nCode, wParam, lParam);
            else
                return (IntPtr)1;
        }
    }
}
