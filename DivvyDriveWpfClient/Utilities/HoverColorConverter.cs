using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DivvyDriveWpfClient.Utilities
{
    public class HoverColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                Color baseColor = brush.Color;

                // Hover sırasında rengi %20 aç
                byte R = (byte)(baseColor.R + ((255 - baseColor.R) * 0.2));
                byte G = (byte)(baseColor.G + ((255 - baseColor.G) * 0.2));
                byte B = (byte)(baseColor.B + ((255 - baseColor.B) * 0.2));

                return new SolidColorBrush(Color.FromRgb(R, G, B));
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
