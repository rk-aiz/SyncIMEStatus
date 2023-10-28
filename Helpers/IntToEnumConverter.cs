using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Input;

namespace SyncIMEStatus.Helpers
{
    [ValueConversion(typeof(int), typeof(Enum))]
    public class IntToEnumConverter : IValueConverter
    {
        public static IntToEnumConverter I = new IntToEnumConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is Type enumType &&
                  value is int intValue &&
                  enumType.IsEnum))
                { return value; } //DoNothingではないので注意

            if (Enum.IsDefined(enumType, intValue))
                return Enum.ToObject(enumType, intValue);
            else
                return intValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int res)
            {
                return res;
            }
            else
            {
                return Binding.DoNothing;
            }
        }
    }
}
