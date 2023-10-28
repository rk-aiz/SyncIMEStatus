using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SyncIMEStatus
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyName = System.IO.Path.GetFileName(assemblyPath);
            var mutexObj = new Mutex(false, assemblyName);

            try
            {
                if (!mutexObj.WaitOne(0, false))
                {
                    mutexObj.Close();
                    return;
                }
            }
            catch (AbandonedMutexException)
            { }

            try
            {
                var appContext = new ApplicationContext();

                TaskTray.Current.ShowIcon();

                SettingManager.LoadSettings();

                SyncIme.Current.BeginHook();

                Application.Run(appContext);

                SyncIme.Current.EndHook();

                //SettingManager.SeveSetting();
            }
            finally
            {
                mutexObj.ReleaseMutex();
                mutexObj.Close();
            }
        }
    }
}
