using System;
using System.Globalization;
using System.Windows.Data;

namespace local_image_sender.Desktop.Classes
{
    public class BoolSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => System.Convert.ToBoolean(value) ? "✔" : "✖";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}