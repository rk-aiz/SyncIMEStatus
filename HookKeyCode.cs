using SyncIMEStatus.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using static SyncIMEStatus.Win32Native;

namespace SyncIMEStatus
{
    [TypeConverter(typeof(Helpers.EnumDescriptionTypeConverter))]
    public enum KeyPassThroughMode
    {
        [Helpers.LocalizedDescription("strThroughBoth")]
        Both,
        [Helpers.LocalizedDescription("strThroughKeyDown")]
        KeyDown,
        [Helpers.LocalizedDescription("strThroughKeyUp")]
        KeyUp,
        [Helpers.LocalizedDescription("strNone")]
        None,
    }


    [TypeConverter(typeof(Helpers.EnumDescriptionTypeConverter))]
    public enum ImeMode
    {
        [Helpers.LocalizedDescription("strDoNothing")]
        DoNothing,
        [Helpers.LocalizedDescription("strDetect")]
        Detect,
        [Helpers.LocalizedDescription("strImeOn")]
        ImeOn,
        [Helpers.LocalizedDescription("strImeOnAndSend")]
        ImeOnAndSend,
        [Helpers.LocalizedDescription("strImeOff")]
        ImeOff,
        [Helpers.LocalizedDescription("strImeOffAndSend")]
        ImeOffAndSend,
        [Helpers.LocalizedDescription("strToggle")]
        Toggle,
        [Helpers.LocalizedDescription("strImeOffAndSend")]
        ToggleAndSend,
    }

    [TypeConverter(typeof(Helpers.EnumDescriptionTypeConverter))]
    public enum TriggerKeyState
    {
        [Helpers.LocalizedDescription("strKeyUp")]
        KeyUp,
        [Helpers.LocalizedDescription("strKeyDown")]
        KeyDown,
        [Helpers.LocalizedDescription("strBoth")]
        Both,
        [Helpers.LocalizedDescription("strDoNothing")]
        DoNothing,
    }

    [TypeConverter(typeof(Helpers.EnumDescriptionTypeConverter))]
    public enum SendMessageMode
    {
        [Helpers.LocalizedDescription("strDoNothing")]
        DoNothing,
        [Helpers.LocalizedDescription("strSend")]
        Send
    }

    public class HookKeyCode : INotifyPropertyChanged
    {
        private int _keyCode;
        public int KeyCode { get { return _keyCode; } set { _keyCode = value; NotifyPropertyChanged(); } }

        private ModifierKeys _modifiers = ModifierKeys.None;
        public ModifierKeys Modifiers { get { return _modifiers; } set { _modifiers = value; NotifyPropertyChanged(); } }
        public ModifierKeys ModifiersAndAdd
        {
            get { return _modifiers; }
            set
            {
                //Debug.WriteLine(value);
                if ((value & ModifierKeys.ClearFlag) == ModifierKeys.ClearFlag)
                {
                    _modifiers &= ~value;
                }
                else
                {
                    _modifiers |= value;
                }
                NotifyPropertyChanged();
            }
        }
        private TriggerKeyState _triggerState;
        public TriggerKeyState TriggerState { get { return _triggerState; } set { _triggerState = value; NotifyPropertyChanged(); } }

        private ImeMode _imeMode;
        public ImeMode ImeMode { get { return _imeMode; } set { _imeMode = value; NotifyPropertyChanged(); } }

        //private SendMessageMode _sendMessage;
        //public SendMessageMode SendMessage { get { return _sendMessage; } set { _sendMessage = value; NotifyPropertyChanged(); } }

        private KeyPassThroughMode _passThroughMode;
        public KeyPassThroughMode PassThroughMode { get { return _passThroughMode; } set { _passThroughMode = value; NotifyPropertyChanged(); } }

        private int _replaceKey;
        public int ReplaceKey { get { return _replaceKey; } set { _replaceKey = value; NotifyPropertyChanged(); } }

        private ModifierKeys _replaceModifiers;
        public ModifierKeys ReplaceModifiers { get { return _replaceModifiers; } set { _replaceModifiers = value; NotifyPropertyChanged(); } }
        public ModifierKeys ReplaceModifiersAndAdd
        {
            get { return _replaceModifiers; }
            set
            {
                //Debug.WriteLine(value);
                if ((value & ModifierKeys.ClearFlag) == ModifierKeys.ClearFlag)
                {
                    _replaceModifiers &= ~value;
                }
                else
                {
                    _replaceModifiers |= value;
                }
                NotifyPropertyChanged();
            }
        }

        public HookKeyCode(int keyCode,
                           ModifierKeys modKeys,
                           KeyPassThroughMode passThroughMode = KeyPassThroughMode.Both,
                           ImeMode imeMode = ImeMode.Detect,
                           TriggerKeyState triggerState = TriggerKeyState.KeyUp)
        {
            KeyCode = keyCode;
            Modifiers = modKeys;
            ImeMode = imeMode;
            PassThroughMode = passThroughMode;
            TriggerState = triggerState;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public int KeyProc(int keyCode, int keyStat)
        {
            //キーコード確認
            if (KeyCode != keyCode) { return 0; }

            foreach (ModifierKeys modKey in Modifiers.GetFlagMembers())
            {
                Key key = GetKeyFromModifierKey(modKey);
                if (key != Key.None && !Keyboard.IsKeyDown(key))
                {
                    return 0;
                }
            }

            if (keyStat != Win32Native.WM_KEYDOWN && keyStat != Win32Native.WM_KEYUP) { return 1; }

            bool canInvoke = false;
            switch (_triggerState)
            {
                case TriggerKeyState.DoNothing:
                    break;
                case TriggerKeyState.KeyDown:
                    if (keyStat == Win32Native.WM_KEYDOWN) { canInvoke = true; }
                    break;
                case TriggerKeyState.KeyUp:
                    if (keyStat == Win32Native.WM_KEYUP) { canInvoke = true; }
                    break;
                case TriggerKeyState.Both:
                    canInvoke = true;
                    break;
            }

            if (canInvoke)
            {
                IEnumerable<Keys> altModKeys =
                    ReplaceModifiers.GetFlagMembers().Where(modKey => modKey != ModifierKeys.None)
                                                     .Select(modKey => GetFormModifierKeys(modKey));

                foreach (Keys amk in altModKeys)
                {
                    Win32Native.SendInput(amk, SendInputMode.KeyDown);
                }

                if (ReplaceKey != (int)Keys.None)
                {
                    Win32Native.SendInput((Keys)ReplaceKey, SendInputMode.KeyDown | SendInputMode.KeyUp);
                }

                foreach (Keys amk in altModKeys)
                {
                    Win32Native.SendInput(amk, SendInputMode.KeyUp);
                }

                switch (ImeMode)
                {
                    case ImeMode.DoNothing:
                        break;
                    case ImeMode.Detect:
                        if (GetFocusedWindowImeStat(out bool ie0))
                        {
                            SyncIme.Current.ImeStat = ie0;
                            break;
                        }
                        break;
                    case ImeMode.ImeOn:
                        SyncIme.Current.ImeStat = true;
                        break;
                    case ImeMode.ImeOff:
                        SyncIme.Current.ImeStat = false;
                        break;
                    case ImeMode.Toggle:
                        if (GetFocusedWindowImeStat(out bool ie1))
                        {
                            SyncIme.Current.ImeStat = ie1 ? false : true; //IME 状態切替
                        }
                        break;
                    case ImeMode.ImeOnAndSend:
                        SyncIme.Current.ImeStat = true;
                        if (GetFocusedWindowImeStat(out bool ie2) && ie2 == true)
                        {
                            SetFocusedWindowImeStat(true);
                        }
                        break;
                    case ImeMode.ImeOffAndSend:
                        SyncIme.Current.ImeStat = false;
                        if (GetFocusedWindowImeStat(out bool ie3) && ie3 != false)
                        {
                            SetFocusedWindowImeStat(false);
                        }
                        break;
                    case ImeMode.ToggleAndSend:
                        if (GetFocusedWindowImeStat(out bool ie4))
                        {
                            SyncIme.Current.ImeStat = ie4 ? false : true; //IME 状態切替
                            SetFocusedWindowImeStat(SyncIme.Current.ImeStat);
                        }
                        break;
                }
            }

            bool passThrough = false;
            switch (_passThroughMode)
            {
                case KeyPassThroughMode.None:
                    break;
                case KeyPassThroughMode.KeyDown:
                    if (keyStat == Win32Native.WM_KEYDOWN) { passThrough = true; }
                    break;
                case KeyPassThroughMode.KeyUp:
                    if (keyStat == Win32Native.WM_KEYUP) { passThrough = true; }
                    break;
                case KeyPassThroughMode.Both:
                    passThrough = true;
                    { return 0; }
            }

            return passThrough ? 0 : 1;
        }

        [Flags]
        public enum ModifierKeys : int
        {
            None = 0,
            ClearFlag = 1,
            LeftAlt = 2,
            RightAlt = 4,
            LeftCtrl = 8,
            RightCtrl = 16,
            LeftShift = 32,
            RightShift = 64,
        }

        private static Key GetKeyFromModifierKey(ModifierKeys modKeyCode)
        {
            switch (modKeyCode)
            {
                case ModifierKeys.LeftAlt:
                    return Key.LeftAlt;
                case ModifierKeys.RightAlt:
                    return Key.RightAlt;
                case ModifierKeys.LeftCtrl:
                    return Key.LeftCtrl;
                case ModifierKeys.RightCtrl:
                    return Key.RightCtrl;
                case ModifierKeys.LeftShift:
                    return Key.LeftShift;
                case ModifierKeys.RightShift:
                    return Key.RightShift;
            }
            return Key.None;
        }

        public static ModifierKeys GetModifierKey(int keyCode)
        {
            switch ((FormModifierKeys)keyCode)
            {
                case FormModifierKeys.LeftAlt:
                    return ModifierKeys.LeftAlt;
                case FormModifierKeys.RightAlt:
                    return ModifierKeys.RightAlt;
                case FormModifierKeys.LeftCtrl:
                    return ModifierKeys.LeftCtrl;
                case FormModifierKeys.RightCtrl:
                    return ModifierKeys.RightCtrl;
                case FormModifierKeys.LeftShift:
                    return ModifierKeys.LeftShift;
                case FormModifierKeys.RightShift:
                    return ModifierKeys.RightShift;
            }
            return ModifierKeys.None;
        }

        public static Keys GetFormModifierKeys(ModifierKeys modKey)
        {
            switch (modKey)
            {
                case ModifierKeys.LeftAlt:
                    return Keys.LMenu;
                case ModifierKeys.RightAlt:
                    return Keys.RMenu;
                case ModifierKeys.LeftCtrl:
                    return Keys.LControlKey;
                case ModifierKeys.RightCtrl:
                    return Keys.RControlKey;
                case ModifierKeys.LeftShift:
                    return Keys.LShiftKey;
                case ModifierKeys.RightShift:
                    return Keys.RShiftKey;
            }
            return Keys.None;
        }

        [TypeConverterAttribute(typeof(KeysConverter))]
        public enum FormModifierKeys : int
        {
            None = 0,
            LeftAlt = System.Windows.Forms.Keys.LMenu,
            RightAlt = System.Windows.Forms.Keys.RMenu,
            LeftCtrl = System.Windows.Forms.Keys.LControlKey,
            RightCtrl = System.Windows.Forms.Keys.RControlKey,
            LeftShift = System.Windows.Forms.Keys.LShiftKey,
            RightShift = System.Windows.Forms.Keys.RShiftKey
        }

        /*
        public enum Keys : int
        {
            KeyCode = 0x0000FFFF,

            Modifiers = unchecked((int)0xFFFF0000),

            None = 0x00,

            LButton = 0x01,

            RButton = 0x02,

            Cancel = 0x03,

            MButton = 0x04,

            XButton1 = 0x05,

            XButton2 = 0x06,

            Back = 0x08,

            Tab = 0x09,

            LineFeed = 0x0A,

            Clear = 0x0C,

            Return = 0x0D,

            Enter = Return,

            ShiftKey = 0x10,

            ControlKey = 0x11,

            Menu = 0x12,

            Pause = 0x13,

            Capital = 0x14,

            CapsLock = 0x14,

            KanaMode = 0x15,

            HanguelMode = 0x15,

            HangulMode = 0x15,

            JunjaMode = 0x17,

            FinalMode = 0x18,

            HanjaMode = 0x19,

            KanjiMode = 0x19,

            Escape = 0x1B,

            IMEConvert = 0x1C,

            IMENonconvert = 0x1D,

            IMEAccept = 0x1E,

            IMEAceept = IMEAccept,

            IMEModeChange = 0x1F,

            Space = 0x20,

            Prior = 0x21,

            PageUp = Prior,

            Next = 0x22,

            PageDown = Next,

            End = 0x23,

            Home = 0x24,

            Left = 0x25,

            Up = 0x26,

            Right = 0x27,

            Down = 0x28,

            Select = 0x29,

            Print = 0x2A,

            Execute = 0x2B,

            Snapshot = 0x2C,

            PrintScreen = Snapshot,

            Insert = 0x2D,

            Delete = 0x2E,

            Help = 0x2F,

            D0 = 0x30,

            D1 = 0x31,

            D2 = 0x32,

            D3 = 0x33,

            D4 = 0x34,

            D5 = 0x35,

            D6 = 0x36,

            D7 = 0x37,

            D8 = 0x38,

            D9 = 0x39,

            A = 0x41,

            B = 0x42,

            C = 0x43,

            D = 0x44,

            E = 0x45,

            F = 0x46,

            G = 0x47,

            H = 0x48,

            I = 0x49,

            J = 0x4A,

            K = 0x4B,

            L = 0x4C,

            M = 0x4D,

            N = 0x4E,

            O = 0x4F,

            P = 0x50,

            Q = 0x51,

            R = 0x52,

            S = 0x53,

            T = 0x54,

            U = 0x55,

            V = 0x56,

            W = 0x57,

            X = 0x58,

            Y = 0x59,

            Z = 0x5A,

            LWin = 0x5B,

            RWin = 0x5C,

            Apps = 0x5D,

            Sleep = 0x5F,

            NumPad0 = 0x60,

            NumPad1 = 0x61,

            NumPad2 = 0x62,

            NumPad3 = 0x63,

            NumPad4 = 0x64,

            NumPad5 = 0x65,

            NumPad6 = 0x66,

            NumPad7 = 0x67,

            NumPad8 = 0x68,

            NumPad9 = 0x69,

            Multiply = 0x6A,

            Add = 0x6B,

            Separator = 0x6C,

            Subtract = 0x6D,

            Decimal = 0x6E,

            Divide = 0x6F,

            F1 = 0x70,

            F2 = 0x71,

            F3 = 0x72,

            F4 = 0x73,

            F5 = 0x74,

            F6 = 0x75,

            F7 = 0x76,

            F8 = 0x77,

            F9 = 0x78,

            F10 = 0x79,

            F11 = 0x7A,

            F12 = 0x7B,

            F13 = 0x7C,

            F14 = 0x7D,

            F15 = 0x7E,

            F16 = 0x7F,

            F17 = 0x80,

            F18 = 0x81,

            F19 = 0x82,

            F20 = 0x83,

            F21 = 0x84,

            F22 = 0x85,

            F23 = 0x86,

            F24 = 0x87,

            NumLock = 0x90,

            Scroll = 0x91,

            LShiftKey = 0xA0,

            RShiftKey = 0xA1,

            LControlKey = 0xA2,

            RControlKey = 0xA3,

            LMenu = 0xA4,

            RMenu = 0xA5,

            BrowserBack = 0xA6,

            BrowserForward = 0xA7,

            BrowserRefresh = 0xA8,

            BrowserStop = 0xA9,

            BrowserSearch = 0xAA,

            BrowserFavorites = 0xAB,

            BrowserHome = 0xAC,

            VolumeMute = 0xAD,

            VolumeDown = 0xAE,

            VolumeUp = 0xAF,

            MediaNextTrack = 0xB0,

            MediaPreviousTrack = 0xB1,

            MediaStop = 0xB2,

            MediaPlayPause = 0xB3,

            LaunchMail = 0xB4,

            SelectMedia = 0xB5,

            LaunchApplication1 = 0xB6,

            LaunchApplication2 = 0xB7,

            OemSemicolon = 0xBA,

            Oem1 = OemSemicolon,

            Oemplus = 0xBB,

            Oemcomma = 0xBC,

            OemMinus = 0xBD,

            OemPeriod = 0xBE,

            OemQuestion = 0xBF,

            Oem2 = OemQuestion,

            Oemtilde = 0xC0,

            Oem3 = Oemtilde,

            OemOpenBrackets = 0xDB,

            Oem4 = OemOpenBrackets,

            OemPipe = 0xDC,

            Oem5 = OemPipe,

            OemCloseBrackets = 0xDD,

            Oem6 = OemCloseBrackets,

            OemQuotes = 0xDE,

            Oem7 = OemQuotes,

            Oem8 = 0xDF,

            OemBackslash = 0xE2,

            Oem102 = OemBackslash,

            ProcessKey = 0xE5,

            Packet = 0xE7,

            Attn = 0xF6,

            Crsel = 0xF7,

            Exsel = 0xF8,

            EraseEof = 0xF9,

            Play = 0xFA,

            Zoom = 0xFB,

            NoName = 0xFC,

            Pa1 = 0xFD,

            OemClear = 0xFE,

            Shift = 0x00010000,

            Control = 0x00020000,

            Alt = 0x00040000,
        }*/
    }
}
