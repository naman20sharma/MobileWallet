using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace MyWallet.Converters
{
    public class NotificationTypeToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                string type = value as string;
                switch (type)
                {
                    case "Credential Offer":
                        return "credential.png";
                    case "Proof Request":
                        return "security.png";
                    default:
                        return "notification.png";
                }

            }
            return "notification.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
