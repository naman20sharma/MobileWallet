using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace MyWallet.Converters
{
    public class NotificationTypeToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                string type = value as string;
                switch(type)
                {
                    case "Credential Offer":
                        return "#0a8f08";
                    case "Proof Request":
                        return "#DD1287";
                    default:
                        return "#6a1b9a";
                }

            }
            return "#6a1b9a";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
