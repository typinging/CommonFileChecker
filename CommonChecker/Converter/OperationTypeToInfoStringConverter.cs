using System;
using System.Globalization;
using System.Windows.Data;

namespace CommonChecker.Converter
{
    public class OperationTypeToInfoStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte v = (byte)value;
            if (v == 1)
            {
                return "新增";
            }
            else if (v == 2)
            {
                return "修改";
            }
            else if (v == 3)
            {
                return "删除";
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
