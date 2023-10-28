using System.Windows.Forms;
using System.Diagnostics;

namespace SyncIMEStatus
{
    public class SettingManager
    {
        public SettingManager() { }

        public static async void LoadSettings()
        {
            var settings = new SettingJson();

            await settings.DeserializeFromFileAsync();

            if (settings.KeyCodeList != null)
                SyncIme.ResetHookKeys(settings.KeyCodeList);
            /*
            SyncIme.Current.HookKeys.Add(new HookKeyCode((int)Keys.IMENonconvert, KeyPassThroughMode.KeyUp, ImeMode.ImeOff));
            SyncIme.Current.HookKeys.Add(new HookKeyCode((int)Keys.IMEConvert, KeyPassThroughMode.KeyUp, ImeMode.ImeOn));
            */
        }

        public static void SeveSettings()
        {
            var settings = new SettingJson();

            settings.KeyCodeList.AddRange(SyncIme.Current.HookKeys); 

            settings.SerializeToFile();
        }
    }
}
