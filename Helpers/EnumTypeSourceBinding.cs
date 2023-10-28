using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SyncIMEStatus.Helpers
{
    /// <summary>
    /// Enum型をItemsSourceとして利用するためのBinding
    /// (例：EnumをComboBoxの選択肢として利用する)
    /// </summary>
    public class EnumTypeSourceBinding : Binding
    {
        private readonly Type _enumType;

        public object EnumValues
        {
            get { return Enum.GetValues(_enumType); }
        }

        public EnumTypeSourceBinding(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException($"{enumType} is not enum.");
            }
            _enumType = enumType;
            Path = new PropertyPath("EnumValues");
            Source = this;
            Mode = BindingMode.OneWay;
            ConverterCulture = CultureInfo.CurrentUICulture;
        }
    }
}