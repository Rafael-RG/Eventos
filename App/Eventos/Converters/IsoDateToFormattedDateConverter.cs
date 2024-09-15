using System.Globalization;

namespace Eventos.Converters
{
    public class IsoDateToFormattedDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string isoDate && DateTime.TryParse(isoDate, null, DateTimeStyles.RoundtripKind, out var dateTime))
            {
                return dateTime.ToString("HH:mm");
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

