using SyncIMEStatus.Helpers;
using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Threading;

namespace SyncIMEStatus
{
    /// <summary>
    /// CustomPopup.xaml の相互作用ロジック
    /// </summary>
    public partial class CustomPopup : Popup
    {
        public CustomPopup()
        {
            DataContext = new ViewModel();
            InitializeComponent();
        }

        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const uint WS_POPUP = 0x80000000;
        public const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        public static extern uint GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, uint newStyle);

        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        private void CustomPopup_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.DoEvents();
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual(this.Child);
            Debug.WriteLine($"popup child handle : {hwndSource.Handle}");

            hwndSource.AddHook(WndProc);

            SetWindowPos(hwndSource.Handle, -1, 0, 0, 0, 0, 0x0001 | 0x0002);

            if (Environment.OSVersion.Version.Major >= 6)
            {
                // Aeroが使用可能か確認
                bool bolAeroEnabled = false;
                DwmIsCompositionEnabled(out bolAeroEnabled);

                if (bolAeroEnabled == true)
                {
                    Debug.WriteLine("DwmIsCompositionEnabled true");
                    int intAttrValue = 2;
                    int intAttrSize = sizeof(int);
                    DwmSetWindowAttribute(hwndSource.Handle, DWMWINDOWATTRIBUTE.NCRenderingPolicy, ref intAttrValue, intAttrSize);

                    MARGINS objMargins = new MARGINS()
                    {
                        leftWidth = 1,
                        rightWidth = 1,
                        topHeight = 1,
                        bottomHeight = 1
                    };

                    DwmExtendFrameIntoClientArea(hwndSource.Handle, ref objMargins);
                }
                else
                {
                    Debug.WriteLine("DwmIsCompositionEnabled false");
                }
            }
        }

        [DllImport("user32", EntryPoint = "SetWindowPos")]
        private static extern int SetWindowPos(IntPtr hWnd, int hwndInsertAfter, int x, int y, int cx, int cy, int wFlags);


        private void SetLocation(object sender, RoutedEventArgs e)
        {
            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
            var mousePosition = new Point(point.X, point.Y);
            var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            var mouse = transform.Transform(mousePosition);

            Rect _workArea = SystemParameters.WorkArea;
            int margin = 10;
            /*
            if (0 < _workArea.Left) // タスクバーが左側
            {
                Left = mouse.X + margin;
                Top = mouse.Y - Height;
            }
            else if (0 < _workArea.Top) // タスクバーが上側
            {
                Left = mouse.X - Width + margin;
                Top = mouse.Y + margin;
            }
            else if (_workArea.Right < SystemParameters.PrimaryScreenWidth) // タスクバーが右側
            {
                Left = mouse.X - Width - margin;
                Top = mouse.Y - Height;
            }
            else // タスクバーが下側
            {
                Left = mouse.X - Width + 10;
                Top = mouse.Y - Height - margin;
            }
            */
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x001C: // WM_APPACTIVE
                    if (wParam == IntPtr.Zero)
                    {
                        //Hide();
                        handled = true;
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        private enum DWMWINDOWATTRIBUTE : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool enabled);

        const int WM_NCPAINT = 0x85;

        public void Hide()
        {
            Debug.WriteLine("Hide()");
            IsOpen = false;
        }

        public void Show()
        {
            IsOpen = true;
            Dispatcher.DoEvents();
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual(this.Child);
            Debug.WriteLine($"Show : {hwndSource.Handle}");
            SetForegroundWindow(hwndSource.Handle.ToInt32());
        }

        public void Close()
        {
            Debug.WriteLine("Close()");
            Width = 0;
            Dispatcher.DoEvents(DispatcherPriority.ApplicationIdle);
            IsOpen = false;
        }
    }
}
