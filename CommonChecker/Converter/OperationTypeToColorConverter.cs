using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CommonChecker.Converter
{
    public class OperationTypeToColorConverter : IValueConverter
    {
        private SolidColorBrush RedBrush = new SolidColorBrush(Colors.Red);
        private SolidColorBrush YellowBrush = new SolidColorBrush(Colors.Yellow);
        private SolidColorBrush BlueBrush = new SolidColorBrush(Colors.Blue);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte v = (byte)value;
            if (v == 1)
            {
                return BlueBrush;
            }
            else if (v == 2)
            {
                return YellowBrush;
            }
            else if (v == 3)
            {
                return RedBrush;
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
