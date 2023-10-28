using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Interop;
using static SyncIMEStatus.Helpers.Win32DesktopApi;
using SyncIMEStatus.Helpers;
using System.Windows.Threading;
using System.Diagnostics;

namespace SyncIMEStatus
{
    /// <summary>
    /// CustomContextMenu.xaml の相互作用ロジック
    /// </summary>
    public partial class CustomContextMenu : Window
    {
        public CustomContextMenu()
        {
            DataContext = new ViewModel();
            InitializeComponent();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            Debug.WriteLine("StateChanged");
            base.OnStateChanged(e);
        }

        private bool prepare = false;

        public async void ShowActivate(int baseX, int baseY)
        {
            prepare = true;
            RaiseShowUpEvent();
            SetLocation(baseX, baseY);
            Show();
            Activate();
            await Task.Delay(100);
            prepare = false;
        }

        public async void HideDeactivate()
        {
            RaiseHideDownEvent();
            await Task.Delay(100);
            if (!IsActive && !prepare)
            {
                Hide();
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            HideDeactivate();
            base.OnDeactivated(e);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            if (PresentationSource.FromVisual(this) is HwndSource hwndSource) { ApplyDwmAttribute(hwndSource.Handle); }
            base.OnSourceInitialized(e);
        }

        public void SetLocation(int baseX, int baseY)
        {
            Rect _workArea = SystemParameters.WorkArea;
            int margin = 20;
            int left;
            int top;

            if (0 < _workArea.Left) // タスクバーが左側
            {
                left = baseX + margin;
                top = baseY - (int)Height;
            }
            else if (0 < _workArea.Top) // タスクバーが上側
            {
                left = baseX - (int)Width + margin;
                top = baseY + margin;
            }
            else if (_workArea.Right < SystemParameters.PrimaryScreenWidth) // タスクバーが右側
            {
                left = baseX - (int)Width - margin;
                top = baseY - (int)Height;
            }
            else // タスクバーが下側
            {
                left = baseX - (int)Width + 20;
                top = baseY - 195;
            }

            if (HwndSource is HwndSource hwndSource)
                SetWindowPos(hwndSource.Handle, -1, left, top, 0, 0, 0x0001);
        }

        public HwndSource HwndSource
        {
            get { return (HwndSource)PresentationSource.FromVisual(this); }
        }

        public event RoutedEventHandler ShowUp
        {
            add { AddHandler(ShowUpEvent, value); }
            remove { RemoveHandler(ShowUpEvent, value); }
        }

        public event RoutedEventHandler HideDown
        {
            add { AddHandler(HideDownEvent, value); }
            remove { RemoveHandler(HideDownEvent, value); }
        }

        public void RaiseShowUpEvent()
        {
            RaiseEvent(new RoutedEventArgs(routedEvent: ShowUpEvent));
        }

        public void RaiseHideDownEvent()
        {
            RaiseEvent(new RoutedEventArgs(routedEvent: HideDownEvent));
        }

        public static readonly RoutedEvent ShowUpEvent = EventManager.RegisterRoutedEvent(
            name: "ShowUp",
            routingStrategy: RoutingStrategy.Direct,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(CustomContextMenu));

        public static readonly RoutedEvent HideDownEvent = EventManager.RegisterRoutedEvent(
            name: "HideDown",
            routingStrategy: RoutingStrategy.Direct,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(CustomContextMenu));
    }
}
