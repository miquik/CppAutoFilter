using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CppAutoFilter
{
    public class RadioButtonCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            string rb = (string)parameter;
            string valueStr = (string)value;
            if (String.IsNullOrEmpty(valueStr))
            {
                if (rb == "%%all%%")
                {
                    return true;
                } else
                {
                    return false;
                }
            }

            if (valueStr == rb)
            {
                return true;
            }

            if (String.IsNullOrEmpty(rb))
            {
                return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            bool rb = (bool)value;
            if (rb)
            {
                if (parameter != null)
                {
                    return (string)parameter;
                }
            }
            return Binding.DoNothing; // throw new NotImplementedException();
        }
    }
}
