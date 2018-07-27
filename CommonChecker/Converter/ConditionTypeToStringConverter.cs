using System;
using System.Globalization;

using System.Windows.Data;

namespace CommonChecker.Converter
{
    public class ConditionTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            ConditionType b = (ConditionType)value;
            if (b == ConditionType.Equal)
            {
                return "等于";
            }
            else if (b == ConditionType.More)
            {
                return "大于";
            }
            else
            {
                return "小于";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
