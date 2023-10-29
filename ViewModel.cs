using SyncIMEStatus.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace SyncIMEStatus
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ViewModel()
        {
            if (SyncIme.Current is INotifyPropertyChanged iNotify)
            {
                iNotify.PropertyChanged += (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case "HookEnabled":
                            NotifyPropertyChanged("SyncEnabled");
                            break;
                        case "HookKeys":
                            CustomKeys.Clear();
                            CustomKeys.AddRange(SyncIme.Current.HookKeys);
                            break;
                    }
                };
            }
            CustomKeys.AddRange(SyncIme.Current.HookKeys);
            /*
            CustomKeys.AddPropertyChanged(new PropertyChangedEventHandler((s, e) =>
            {
                Debug.WriteLine("PropertyChanged!!");
                SyncIme.Current.HookKeys.ForEach(item =>
                {
                    Debug.WriteLine($"{item.KeyCode}, {item.ImeMode}, {item.PassThroughMode}");
                });
            }));
            */
        }

        public void ApplyCustomKeys()
        {
            SyncIme.ResetHookKeys(CustomKeys);
            SettingManager.SeveSettings();
        }


        private ICommand _exitCommand;
        private ICommand _openSettingCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public bool SyncEnabled
        {
            get { return SyncIme.Current.HookEnabled; }
            set
            {
                if (value == true)
                {
                    SyncIme.Current.BeginHook();
                }
                else
                {
                    SyncIme.Current.EndHook();
                }
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<HookKeyCode> CustomKeys { get; } = new ObservableCollection<HookKeyCode>();
        private int _customKeysSelectedIndex = 0;
        public int CustomKeysSelectedIndex
        {
            get { return  _customKeysSelectedIndex; }
            set { _customKeysSelectedIndex = value; NotifyPropertyChanged(); }
        }

        public bool TryAddCustomKey(int keyCode, HookKeyCode.ModifierKeys modKeys)
        {
            //重複設定の検出 → 別に重複してもいいからコメントアウト
            /*
            var dupKey = CustomKeys
                .Where(key => (key.KeyCode == keyCode && key.Modifiers == modKeys))
                .FirstOrDefault();

            if (dupKey != null)
            {
                CustomKeysSelectedIndex = CustomKeys.IndexOf(dupKey);
                return false;
            }
            */

            HookKeyCode newKeyCode = new HookKeyCode(keyCode, modKeys);
            CustomKeys.Add(newKeyCode);

            CustomKeysSelectedIndex = CustomKeys.IndexOf(newKeyCode);
            return true;
        }

        public bool SetReplaceKey(int keyCode)
        {
            var cKey = CustomKeys.ElementAtOrDefault(CustomKeysSelectedIndex);
            if (cKey != null)
            {
                cKey.ReplaceKey = keyCode;
            }
            return true;
        }

        public bool RemoveCustomKey(HookKeyCode keyCode)
        {
            var index = CustomKeys.IndexOf(keyCode) - 1;
            if (index < 0) index = 0;
            CustomKeys.Remove(keyCode);
            CustomKeysSelectedIndex = index;
            return true;
        }

        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                    _exitCommand = new DelegateCommand
                    {
                        ExecuteHandler = (param) => TaskTray.Current.Close(),
                    };
                return _exitCommand;
            }
        }

        private SettingWindow _settingWindow;
        public void CloseSettingWindow()
        {
            _settingWindow?.Close();
        }

        public ICommand OpenSettingCommand
        {
            get
            {
                if (_openSettingCommand == null)
                    _openSettingCommand = new DelegateCommand
                    {
                        ExecuteHandler = (param) =>
                        {
                            if (_settingWindow?.IsAlive == true)
                            {
                                _settingWindow.Activate();
                            }
                            else
                            {
                                _settingWindow = new SettingWindow();
                                _settingWindow.DataContext = this;
                                _settingWindow.Show();
                            }
                            TaskTray.Current.HideMenuWindow();
                        }
                    };
                return _openSettingCommand;
            }
        }
    }
}
