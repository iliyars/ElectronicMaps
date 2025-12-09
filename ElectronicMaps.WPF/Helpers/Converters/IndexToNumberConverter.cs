using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ElectronicMaps.WPF.Helpers.Converters
{
    public class ItemIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Binding.DoNothing;

            var item = values[0];
            var collection = values[1] as IEnumerable;

            if (item == null || collection == null)
                return Binding.DoNothing;

            // Пытаемся найти индекс элемента в коллекции
            var list = collection as IList ?? collection.Cast<object>().ToList();

            var index = list.IndexOf(item);
            if (index < 0)
                return Binding.DoNothing;

            return index + 1; // 1,2,3...
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
