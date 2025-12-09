using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ElectronicMaps.WPF.Helpers.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {

        public bool Inverted { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolValue)
                return Visibility.Collapsed;

            if(Inverted)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is not Visibility visibilityValue)
                return false;

            bool result = visibilityValue == Visibility.Visible;
            return Inverted ? !result : result;
        }
    }
}
