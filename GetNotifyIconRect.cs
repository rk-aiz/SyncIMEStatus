using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace SyncIMEStatus
{
    public static class NotifyIconHelper
    {
        public static Rectangle GetIconRect(this NotifyIcon icon)
        {
            RECT rect = new RECT();
            NOTIFYICONIDENTIFIER notifyIcon = new NOTIFYICONIDENTIFIER();

            notifyIcon.cbSize = Marshal.SizeOf(notifyIcon);
            //use hWnd and id of NotifyIcon instead of guid is needed
            notifyIcon.hWnd = GetHandle(icon);
            notifyIcon.uID = GetId(icon);

            int hresult = Shell_NotifyIconGetRect(ref notifyIcon, out rect);
            //rect now has the position and size of icon

            return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public Int32 left;
            public Int32 top;
            public Int32 right;
            public Int32 bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NOTIFYICONIDENTIFIER
        {
            public Int32 cbSize;
            public IntPtr hWnd;
            public Int32 uID;
            public Guid guidItem;
        }

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int Shell_NotifyIconGetRect([In] ref NOTIFYICONIDENTIFIER identifier, [Out] out RECT iconLocation);

        public static IntPtr GetHandle(this NotifyIcon icon)
        {
            if (typeof(NotifyIcon).GetField("window", BindingFlags.NonPublic | BindingFlags.Instance) is FieldInfo windowField)
            if (windowField.GetValue(icon) is NativeWindow window)
            {
                return window.Handle;
            }

            throw new InvalidOperationException("Get Handle of NotifyIcon Failed");
        }

        private static int GetId(NotifyIcon icon)
        {
            if (typeof(NotifyIcon).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance) is FieldInfo idField)
            {
                return (int)idField.GetValue(icon);
            }

            throw new InvalidOperationException("Get ID of NotifyIcon Failed");
        }

    }
}
