using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SyncIMEStatus.Helpers
{
    /// <summary>
    /// Flags属性のEnumをバインディングソースに、ConverterParameter指定の値をBooleanに変換します。
    /// ConvertBackを使用するにはNot And代入を指定するフラグ「ClearFlag」がEnumに必要で、
    /// setアクセサで|=value代入と&= ~value代入を条件分岐する必要があります。
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(Boolean))]
    public class FlagsToBooleanConverter : IValueConverter
    {
        public static FlagsToBooleanConverter I = new FlagsToBooleanConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string enumProperty &&
                  value is Enum enumValue &&
                  Attribute.GetCustomAttribute(enumValue.GetType(), typeof(FlagsAttribute)) is FlagsAttribute))
                { return Binding.DoNothing; }

            var a = System.Convert.ToInt64(enumValue);
            var b = System.Convert.ToInt64(Enum.Parse(enumValue.GetType(), enumProperty));
            return (a & b) == b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string enumProperty &&
                  value is bool boolValue &&
                  Attribute.GetCustomAttribute(targetType, typeof(FlagsAttribute)) is FlagsAttribute))
                { return Binding.DoNothing; }

            if (boolValue)
            {
                return Enum.Parse(targetType, enumProperty);
            }
            else
            {
                return Enum.ToObject(targetType,
                    System.Convert.ToInt64(Enum.Parse(targetType, "ClearFlag")) |
                    System.Convert.ToInt64(Enum.Parse(targetType, enumProperty)));
            }
        }
    }
}
