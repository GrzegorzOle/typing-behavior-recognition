using System;
using System.Globalization;
using System.Windows.Data;

namespace InvisiTypeAI.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolToStartStopConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMonitoring)
            {
                return isMonitoring ? "Wyłącz monitorowanie" : "Włącz monitorowanie";
            }

            return "Włącz monitorowanie";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // niepotrzebne w tym przypadku
        }
    }
}