using SyncIMEStatus.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIMEStatus.Helpers
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ToggleStateDesc
    {
        [LocalizedDescription("strOff")]
        Off,
        [LocalizedDescription("strOn")]
        On,
    }
}
