using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using SocketHook.HostedWpfSample.Extensions;

namespace SocketHook.HostedWpfSample.Converters
{
    public class BitmapToSource : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return default;
            if (!(value is Bitmap image)) return default;

            return image.ToBitmapSource();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default;
        }
    }
}
