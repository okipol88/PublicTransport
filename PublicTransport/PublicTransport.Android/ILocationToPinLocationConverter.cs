using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PublicTransport.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace PublicTransport.Droid
{
    public class ILocationToPinLocationConverter : IValueConverter
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