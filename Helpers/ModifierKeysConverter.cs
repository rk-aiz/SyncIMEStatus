using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIMEStatus.Helpers
{
    public class ModifierKeysTypeConverter : EnumConverter
    {
        public ModifierKeysTypeConverter(Type type) : base(type)
        { }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) &&
                value is Enum enumValue)
            {
                Debug.WriteLine("ModifierKeysTypeConverter");
                List<string> strList = new List<string>();
                Int64 a = Convert.ToInt64(enumValue);
                //StringBuilder sb = new StringBuilder();
                foreach (var val in Enum.GetValues(enumValue.GetType()))
                {
                    Int64 b = Convert.ToInt64(val);
                    if ((a & b) == b)
                    {
                        strList.Add(val.ToString());
                    }
                }

                return string.Join(" + ", strList);
            }
            else
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
