using PublicTransport.Core.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Xamarin.Forms.Maps;

namespace PublicTransport.Core
{
    class ILocationToBingLocationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ILocation location)
            {
                return new Position(location.Latitude, location.Longitude);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
