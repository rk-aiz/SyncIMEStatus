using SyncIMEStatus.Helpers;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SyncIMEStatus.Helpers
{
    public class CultureAwareBinding : Binding
    {
        public string Key
        {
            set
            {
                Path = new PropertyPath("Resources." + value);
                Source = ResourceHelper.Instance;
            }
        }

        public CultureAwareBinding()
        {
            ConverterCulture = CultureInfo.CurrentUICulture;
        }
    }
}
