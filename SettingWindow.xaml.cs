using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static SyncIMEStatus.Win32Native;

namespace SyncIMEStatus
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {

        private enum CatchKeyMode : int
        {
            DontCatch,
            CatchHookKey,
            CatchReplaceKey,
        }

        public bool IsAlive { get; private set; }
        private CatchKeyMode _catchKeyMode = CatchKeyMode.DontCatch;
        private IntPtr _catchKeyHookId;
        private WindowsHookDelegate _catchKeyHookProc;

        public SettingWindow()
        {
            InitializeComponent();
            IsAlive = true;
            Closing += (s, e) => SettingManager.LoadSettings();
            Closed += (s, e) => { IsAlive = false; };
            _catchKeyHookProc = CatchKeyHookProc;
        }

        private int escKeyDownCount = 0;
        private HookKeyCode.ModifierKeys _currentModifiers = 0;
        private IntPtr CatchKeyHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (_catchKeyMode == CatchKeyMode.DontCatch) { return CallNextHookEx(_catchKeyHookId, nCode, wParam, lParam); }

                int keyStat = (int)wParam;
            int keyCode = Marshal.ReadInt32(lParam);

            if (keyStat != Win32Native.WM_KEYUP)
            {
                if (keyCode == (int)System.Windows.Forms.Keys.Escape)
                {
                    escKeyDownCount++;
                }
                else
                {
                    _currentModifiers |= HookKeyCode.GetModifierKey(keyCode);
                }

                if (escKeyDownCount < 3)
                {
                    return (IntPtr)1;
                }
            }

            if (DataContext is ViewModel vm)
            {
                    Debug.WriteLine($"{_catchKeyMode} : {(System.Windows.Forms.Keys)keyCode}");
                    switch (_catchKeyMode)
                    {
                        case CatchKeyMode.CatchHookKey:
                            vm.TryAddCustomKey(keyCode, _currentModifiers);
                            break;
                        case CatchKeyMode.CatchReplaceKey:
                            if (escKeyDownCount < 3)
                            {
                                vm.SetReplaceKey(keyCode);
                            }
                            else
                            {
                                vm.SetReplaceKey((int)System.Windows.Forms.Keys.None);
                            }
                            break;
                    }

            }

            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(AddCustomKeyTextBox), null);
            Keyboard.ClearFocus();
            _currentModifiers = 0;
            escKeyDownCount = 0;
            _catchKeyMode = CatchKeyMode.DontCatch;

            return CallNextHookEx(_catchKeyHookId, nCode, wParam, lParam);
        }

        private void AddCustomKeyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {

            if (IntPtr.Zero == _catchKeyHookId)
            {
                _catchKeyMode = CatchKeyMode.CatchHookKey;
                _catchKeyHookId = SetLLKeyEventHook(_catchKeyHookProc);
            }
        }

        private void SetReplaceKeyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (IntPtr.Zero == _catchKeyHookId)
            {
                _catchKeyMode = CatchKeyMode.CatchReplaceKey;
                _catchKeyHookId = SetLLKeyEventHook(_catchKeyHookProc);
            }
        }

        private async void AddCustomKeyTextBox_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            if (!await RemoveWinHookEx(_catchKeyHookId))
            {
                throw new Win32Exception("Failed to release the hook that supplements key input.");
            };
            _catchKeyHookId = IntPtr.Zero;
        }

        private void AddCustomKeyTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (sender is TextBox tb)
            {
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(tb), null);
                Keyboard.ClearFocus();
            }
        }

        private void RootGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid gr)
            {
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(gr), null);
                Keyboard.ClearFocus();
            }
        }

        private void CustomKeyRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(CustomKeysListBox.SelectedItem is HookKeyCode key)) { return; }
            if (!(DataContext is ViewModel vm)) { return; }

            vm.RemoveCustomKey(key);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModel vm)
            {
                vm.ApplyCustomKeys();
            }
        }

        private void CancelButtton_Click(object sender, RoutedEventArgs e)
        {
            SettingManager.LoadSettings();
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModel vm)
            {
                vm.ApplyCustomKeys();
            }
            Close();
        }
    }
}
