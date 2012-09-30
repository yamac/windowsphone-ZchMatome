using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ZchMatome.Converters
{
    public class ColorCodeToColorIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                value = int.Parse((value as string).TrimStart('#'), System.Globalization.NumberStyles.HexNumber);
            }
            if (value is int)
            {
                int index = 0;
                Constants.Media.AccentColorCodeToColorIndex.TryGetValue((int)value, out index);
                return index;
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }

    public class ColorCodeToColorNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                value = int.Parse((value as string).TrimStart('#'), System.Globalization.NumberStyles.HexNumber);
            }
            if (value is int)
            {
                int index = 0;
                Constants.Media.AccentColorCodeToColorIndex.TryGetValue((int)value, out index);
                return Constants.Media.AccentColorIndexToName[index];
            }
            else
            {
                return value;
            }
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorCodeToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                value = 0;
            }
            if (value is string)
            {
                value = int.Parse((value as string).TrimStart('#'), System.Globalization.NumberStyles.HexNumber);
            }
            if (value is int)
            {
                int i = (int)value;
                Color color = Color.FromArgb((byte)(i >> 24 & 0xff), (byte)(i >> 16 & 0xff), (byte)(i >> 8 & 0xff), (byte)(i & 0xff));
                return new SolidColorBrush(color);
            }
            throw new ArgumentException("Type of the value is incorrect.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
