using System.Windows.Threading;

namespace SyncIMEStatus.Helpers
{
    internal static class DispatcherHelper
    {
        public static void DoEvents(this Dispatcher dp, DispatcherPriority priority = DispatcherPriority.Background)
        {
            DispatcherFrame frame = new DispatcherFrame();
            dp.BeginInvoke(priority, new DispatcherOperationCallback(obj =>
            {
                ((DispatcherFrame)obj).Continue = false;
                return null;
            }), frame);
            Dispatcher.PushFrame(frame);
        }
    }
}
