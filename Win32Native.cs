using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncIMEStatus
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GUITHREADINFO
    {
        public int cbSize;
        public int flags;
        public IntPtr hwndActive;
        public IntPtr hwndFocus;
        public IntPtr hwndCapture;
        public IntPtr hwndMenuOwner;
        public IntPtr hwndMoveSize;
        public IntPtr hwndCaret;
        public System.Drawing.Rectangle rcCaret;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Input
    {
        public int Type;
        public InputUnion ui;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct InputUnion
    {
        [FieldOffset(0)]
        public MouseInput Mouse;
        [FieldOffset(0)]
        public KeyboardInput Keyboard;
        [FieldOffset(0)]
        public HardwareInput Hardware;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MouseInput
    {
        public int X;
        public int Y;
        public int Data;
        public int Flags;
        public int Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct KeyboardInput
    {
        public short VirtualKey;
        public short ScanCode;
        public int Flags;
        public int Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct HardwareInput
    {
        public int uMsg;
        public short wParamL;
        public short wParamH;
    }

    public class Win32Native
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, WindowsHookDelegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern bool GetGUIThreadInfo(uint dwthreadid, ref GUITHREADINFO lpguithreadinfo);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsDelegate callback, IntPtr lparam);

        [DllImport("user32.dll")]
        private extern static void SendInput(int nInputs, Input[] pInputs, int cbsize);

        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
        private extern static int MapVirtualKey(int wCode, int wMapType);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        public const uint EVENT_OBJECT_SELECTION = 0x8006;
        public const uint EVENT_OBJECT_IME_HIDE = 0x8028;
        public const uint EVENT_OBJECT_IME_SHOW = 0x8027;
        public const uint EVENT_OBJECT_FOCUS = 0x8005;
        public const uint WINEVENT_OUTOFCONTEXT = 0x0000;
        public const uint WINEVENT_SKIPOWNPROCESS = 0x0002;
        public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        public const uint EVENT_SYSTEM_CAPTURESTART = 0x0008;
        public const uint EVENT_MAX = 0x7FFFFFFF;
        public const int WH_KEYBOARD_LL = 0x000D;
        public const int WM_KEYUP = 0x0101;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_SYSKEYUP = 0x0101;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSCOMMAND = 0x0112;
        public const int WM_IME_CONTROL = 0x0283;
        public const int IMC_GETOPENSTATUS = 0x0005;
        public const int IMC_SETOPENSTATUS = 0x0006;
        private const int MAPVK_VK_TO_VSC = 0;
        // private const int MAPVK_VSC_TO_VK = 1;
        //private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const int KEYEVENTF_KEYUP = 0x0002;
        //private const int KEYEVENTF_SCANCODE = 0x0008;
        //private const int KEYEVENTF_UNICODE = 0x0004;

        public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);
        public delegate IntPtr WindowsHookDelegate(int nCode, IntPtr wParam, IntPtr lParam);
        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        // SetWindowImeStatViaContext 追加
        [DllImport("imm32.dll")]
        static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll")]
        static extern bool ImmSetOpenStatus(IntPtr hIMC, bool bOpen);

        [DllImport("imm32.dll")]
        static extern bool ImmGetOpenStatus(IntPtr hIMC, out bool bOpen);

        [DllImport("imm32.dll")]
        static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

        public static bool SetWindowImeStatViaContext(IntPtr hWnd, bool open)
        {
            var hImc = ImmGetContext(hWnd);
            if (hImc == IntPtr.Zero) return false;
            var ok = ImmSetOpenStatus(hImc, open);
            ImmReleaseContext(hWnd, hImc);
            return ok;
        }



        //ひらがな／半角カナ／アルファベット混在など、変換モードまで制御したい場合は以下 API
        [DllImport("imm32.dll")]
        static extern bool ImmSetConversionStatus(IntPtr hIMC, int conversion, int sentence);

        // 変換モード定数例
        const int IME_CMODE_NATIVE = 0x0001; // かな
        const int IME_CMODE_KATAKANA = 0x0002; // カタカナ
        const int IME_CMODE_FULLSHAPE = 0x0008; // 全角
        const int IME_CMODE_ROMAN = 0x0010; // ローマ字

        // ひらがな全角ローマ字→
        //var conversion = IME_CMODE_NATIVE | IME_CMODE_FULLSHAPE | IME_CMODE_ROMAN;
        //ImmSetConversionStatus(hIMC, conversion, 0);

        public static bool GetWindowImeStatViaContext(IntPtr hWnd, out bool imeStat)
        {
            imeStat = false;
            // hWnd の簡易チェック
            if (hWnd == IntPtr.Zero)
                return false;

            // IME コンテキスト取得
            IntPtr hImc = ImmGetContext(hWnd);
            if (hImc == IntPtr.Zero)
                return false;

            try
            {
                // IME開閉状態取得
                if (!ImmGetOpenStatus(hImc, out imeStat))
                    return false;

                return true;
            }
            finally
            {
                // 必ずリリース
                ImmReleaseContext(hWnd, hImc);
            }
        }


        public static bool GetWindowImeStat(IntPtr hWnd, out bool imeStat)
        {
            IntPtr imwd = ImmGetDefaultIMEWnd(hWnd);
            if (imwd == IntPtr.Zero) { imeStat = false; return false; }
            imeStat = SendMessage(imwd, WM_IME_CONTROL, IMC_GETOPENSTATUS, 0) != IntPtr.Zero;
            return true;
        }

        public static bool SetWindowImeStat(IntPtr hWnd, bool imeStat)
        {
            IntPtr imwd = ImmGetDefaultIMEWnd(hWnd);
            if (imwd == IntPtr.Zero) { return false; }

            SendMessage(imwd, WM_IME_CONTROL, IMC_SETOPENSTATUS, imeStat ? 1 : 0);
            return true;
        }

        public static bool GetFocusedWindowImeStat(out bool imeStat)
        {
            if (!GetFocusedWindow(out IntPtr hwndFocus)) { imeStat = false; return false; }

            if (!GetWindowImeStatViaContext(hwndFocus, out imeStat)) { return false; }
            return true;
        }

        public static bool SetFocusedWindowImeStat(bool imeStat)
        {
            if (!GetFocusedWindow(out IntPtr hwndFocus)) { return false; }

            if (!SetWindowImeStat(hwndFocus, imeStat)) { return false; };

            return true;
        }

        // ViaContext 追加
        public static bool SetFocusedWindowImeStatViaContext(bool imeStat)
        {
            if (!GetFocusedWindow(out IntPtr hwndFocus)) { return false; }

            if (!SetWindowImeStatViaContext(hwndFocus, imeStat)) { return false; }

            return true;
        }

        private static bool GetFocusedWindow(out IntPtr hWnd)
        {
            GUITHREADINFO gti = new GUITHREADINFO();
            gti.cbSize = Marshal.SizeOf(gti);

            if (!GetGUIThreadInfo(0, ref gti))
            {
                Debug.WriteLine("GetGUIThreadInfo failed");
                hWnd = IntPtr.Zero;
                return false;
            }
            else
            {
                hWnd = gti.hwndFocus;
                return true;
            }
        }

        public static IntPtr SetLLKeyEventHook(WindowsHookDelegate proc)
        {
            IntPtr id = IntPtr.Zero;

            id = SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);

            Debug.Assert(id != IntPtr.Zero, "SetWindowsHookEx failed.");
            return id;
        }

        public static IntPtr SetForegroundEventHook(WinEventDelegate callback)
        {
            IntPtr id = IntPtr.Zero;
            id = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_CAPTURESTART, IntPtr.Zero, callback, 0, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

            Debug.Assert(id != IntPtr.Zero, "SetForegroundEventHook failed.");
            return id;
        }

        public static IntPtr SetFocusEventHook(WinEventDelegate callback)
        {
            IntPtr id = IntPtr.Zero;
            id = SetWinEventHook(EVENT_OBJECT_FOCUS, EVENT_OBJECT_SELECTION, IntPtr.Zero, callback, 0, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

            Debug.Assert(id != IntPtr.Zero, "SetForegroundEventHook failed.");
            return id;
        }

        public static async Task<bool> RemoveWinHookEx(IntPtr hookId, int attempts = 3)
        {
            if (IntPtr.Zero == hookId) { return true; }

            for (int i = 0; i < attempts; i++)
            {
                if (UnhookWindowsHookEx(hookId)) { return true; }
                await Task.Delay(1000);
            }
            
            return false;
        }

        public static async void RemoveWinEventHook(IntPtr hookId, int attempts = 3)
        {
            if (IntPtr.Zero != hookId)
            {
                for (int i = 0; i < attempts; i++)
                {
                    if (UnhookWinEvent(hookId)) { return; }
                    await Task.Delay(1000);
                }
                return;
            }
        }

        [Flags]
        public enum SendInputMode
        {
            KeyDown = 1,
            KeyUp = 2,
        }

        public static void SendInput(Keys key, SendInputMode mode)
        {
            List<Input> inputList = new List<Input>();
            int vsc = MapVirtualKey((int)key, MAPVK_VK_TO_VSC);

            if (mode.HasFlag(SendInputMode.KeyDown))
            {
                Debug.WriteLine($"KeyDown {key}");
                Input iKeyDown = new Input { Type = 1 };// KeyBoard = 1
                iKeyDown.ui.Keyboard.VirtualKey = (short)key;
                iKeyDown.ui.Keyboard.ScanCode = (short)vsc;
                iKeyDown.ui.Keyboard.Flags = 0;
                iKeyDown.ui.Keyboard.Time = 0;
                iKeyDown.ui.Keyboard.ExtraInfo = IntPtr.Zero;
                inputList.Add(iKeyDown);
            }

            if (mode.HasFlag(SendInputMode.KeyUp))
            {
                Debug.WriteLine($"KeyUp {key}");
                Input iKeyUp = new Input { Type = 1 };// KeyBoard = 1
                iKeyUp.ui.Keyboard.VirtualKey = (short)key;
                iKeyUp.ui.Keyboard.ScanCode = (short)vsc;
                iKeyUp.ui.Keyboard.Flags = KEYEVENTF_KEYUP;
                iKeyUp.ui.Keyboard.Time = 0;
                iKeyUp.ui.Keyboard.ExtraInfo = IntPtr.Zero;
                inputList.Add(iKeyUp);
            }

            if (inputList.Count > 0)
            {
                Input[] inputs = inputList.ToArray();
                SendInput(inputs.Length, inputs, Marshal.SizeOf(inputs[0]));
            }
        }
    }
}
