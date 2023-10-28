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
        }

        public static void SeveSettings()
        {
            var settings = new SettingJson();

            settings.KeyCodeList.AddRange(SyncIme.Current.HookKeys); 

            settings.SerializeToFile();
        }
    }
}
