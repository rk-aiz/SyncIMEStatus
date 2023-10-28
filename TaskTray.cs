using SyncIMEStatus.Helpers;
using System;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows;
using System.Drawing;
using static SyncIMEStatus.Helpers.Win32DesktopApi;

namespace SyncIMEStatus
{
    public class TaskTray
    {
        public static readonly TaskTray Current = new TaskTray();
        private NotifyIcon _notifyIcon;
        private CustomContextMenu _cMenu;
        private TaskTray() { }

        public void ShowIcon()
        {
            //通知領域にアイコンを表示
            _notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = Properties.Resources.AppIcon,
                Text = "SyncIMEStatus"
            };

            _notifyIcon.MouseClick += NotifyIcon_MouseClick;
            _cMenu = new CustomContextMenu();
            
            _cMenu.SourceInitialized += (s, e) =>
            {
                if (_notifyIcon.GetIconRect() is Rectangle rect)
                    _cMenu.SetLocation(rect.X, rect.Y);
                Dispatcher.CurrentDispatcher.DoEvents(DispatcherPriority.ContextIdle);
            };
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ShowCMenuWindow();
            }
        }

        /// <summary>
        /// 設定画面を表示
        /// </summary>
        public void ShowCMenuWindow()
        {
            if (_cMenu == null)
                _cMenu = new CustomContextMenu();
            //キー入力のForm => WPF 転送
            //ElementHost.EnableModelessKeyboardInterop(_cMenu);

            if (_notifyIcon.GetIconRect() is Rectangle rect)
                _cMenu.ShowActivate(rect.X, rect.Y);
        }

        public void HideMenuWindow()
        {
            _cMenu.HideDeactivate();
        }

        /*
        /// <summary>
        /// コンテキストメニューの初期化
        /// </summary>
        private ContextMenuStrip InitializeCustomContextMenu()
        {
            var menuStrip = new ContextMenuStrip();

            menuStrip.Items.Add("Exit", null, (s, e) => Close());

            return menuStrip;
        }
        */

        public void Close()
        {
            if (_cMenu != null)
            {
                if (_cMenu.DataContext is ViewModel vm)
                {
                    vm.CloseSettingWindow();
                }
                _cMenu.Close();
            }

            if (null != _notifyIcon)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            System.Windows.Forms.Application.ExitThread();
        }
    }
}
