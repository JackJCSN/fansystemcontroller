using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace com.JackJCSN.DataAPI
{
    [ValueConversion(typeof(Boolean), typeof(Boolean))]
    public class BooleanNotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return !((Boolean)value);
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return !((Boolean)value);
            }
            catch
            {
                return false;
            }
        }
    }

    [ValueConversion(typeof(Boolean), typeof(Visibility))]
    public class BooleanVisibilityConverter : IValueConverter
    {
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                Boolean v = (bool)value;
                if (v)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }
            catch
            {
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                Visibility v = (Visibility)value;
                switch (v)
                {
                    case Visibility.Hidden:
                    case Visibility.Collapsed:
                        return false;
                    case Visibility.Visible:
                        return true;
                    default:
                        return null;
                }
            }
            catch
            {
            }
            return null;
        }

        #endregion
    }

    [ValueConversion(typeof(Boolean), typeof(String))]
    public class IsConnected : IValueConverter
    {

        #region IValueConverter 成员
        public static readonly String stop;
        public static readonly String open;

        static IsConnected()
        {
            stop = R.Disconnect;
            open = R.Connect;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            try
            {
                bool r = Boolean.Parse(parameter.ToString());
                bool v;
                if (r)
                {
                    v = !(bool)value;
                }
                else
                {
                    v = (bool)value;
                }
                if (v)
                {
                    return open;
                }
                else
                {
                    return stop;
                }

            }
            catch
            {
                return stop;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }

        #endregion
    }

    [ValueConversion(typeof(Boolean), typeof(String))]
    public class ConnectedStat : IValueConverter
    {
        #region IValueConverter 成员
        public static readonly String stop;
        public static readonly String open;

        static ConnectedStat()
        {
            stop = R.NotConnected;
            open = R.Connected;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            try
            {
                bool  v = (bool)value;

                if (v)
                {
                    return open;
                }
                else
                {
                    return stop;
                }

            }
            catch
            {
                return stop;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }

        #endregion
    }

    [ValueConversion(typeof(Double), typeof(String))]
    [ValueConversion(typeof(Int32), typeof(String))]
    public class DoubleToIntString : IValueConverter
    {
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value is ValueType)
                {
                    return String.Format("{0,2:00.}", value);
                }
                if (value is string)
                {
                    double a;
                    double.TryParse(value.ToString(), out a);
                    return String.Format("{0,2:F2}", a);
                }
            }
            catch (Exception)
            {
            }
            return "00.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    [ValueConversion(typeof(Double), typeof(String))]
    public class TempToColor : IValueConverter
    {
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int i = (int)double.Parse(value.ToString());
                if (i < SC.TempLowest)
                {
                    i = SC.TempLowest;
                }
                if (i > SC.TempHight)
                {
                    i = SC.TempHight - 1;
                }
                
                Color s;
                Color e;
                double bl;
                if (i < SC.TempWarring)
                {
                    s = SC.rgbTemplateStart;
                    e = SC.rgbTemplateMid;
                    bl = 1d / (double)Math.Abs(SC.TempWarring - SC.TempLowest);
                }
                else
                {
                    s = SC.rgbTemplateMid;
                    e = SC.rgbTemplateEnd;
                    bl = 1d / (double)Math.Abs(SC.TempHight - SC.TempWarring);
                }
                double n = i * bl;
                var p = n - (int)n;
                var c = Color.FromRgb(
                    (byte)(s.R * (1d - p) + e.R * p),
                    (byte)(s.G * (1d - p) + e.G * p),
                    (byte)(s.B * (1d - p) + e.B * p));
                var rgb = String.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
                return rgb;
            }
            catch
            {
                return SC.rgbTemplateError.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    [ValueConversion(typeof(Double), typeof(String))]
    public class LoadToColor : IValueConverter
    {
        public static readonly double bl = 0.01d;

        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int i = (int)double.Parse(value.ToString());
                if (i < 0)
                {
                    i = 0;
                }
                if (i > 99)
                {
                    i = 99;
                }                
                Color s;
                Color e;
                var bl = 1d/(double)SC.LoadWarring;
                if (i < SC.LoadWarring)
                {
                    s = SC.rgbLoadStart;
                    e = SC.rgbLoadMid;
                }
                else
                {
                    bl = 1d / (double)(100 - SC.LoadWarring);
                    s = SC.rgbLoadMid;
                    e = SC.rgbLoadEnd;
                }
                double n = i * bl;
                var p = n - (int)n;
                var c = Color.FromRgb(
                    (byte)(s.R * (1d - p) + e.R * p),
                    (byte)(s.G * (1d - p) + e.G * p),
                    (byte)(s.B * (1d - p) + e.B * p));
                var rgb = String.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
                return rgb;
            }
            catch
            {
                return SC.rgbLoadError.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
