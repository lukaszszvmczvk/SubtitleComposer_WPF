using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace wpf_lab2
{
    public class DurationConverter : IMultiValueConverter
    {
        TimeSpan showTime;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
                return "";
            TimeSpan start = (TimeSpan)values[0];
            this.showTime = start;
            TimeSpan end = (TimeSpan)values[1];
            TimeSpan duration = end - start;
            var converter = new TimeSpanConverter();
            string s = "";
            if (duration.CompareTo(TimeSpan.Zero) < 0)
                s = "-";
            return s + converter.Convert(duration, null, null, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            TimeSpan[] times = new TimeSpan[2];
            if (value is string str)
            {
                TimeSpan interval;
                string[] parsePatterns = new string[] { "%s", "%s\\.fff", "%s\\.ff", "%s\\.f", "%m\\:%s", "%m\\:%s\\.f",
                    "%m\\:%s\\.ff", "%m\\:%s\\.fff", "h\\:%m\\:%s\\.fff","h\\:%m\\:%s\\.f", "h\\:%m\\:%s\\.ff","h\\:%m\\:%s"};
                if (TimeSpan.TryParseExact(str, parsePatterns, culture, TimeSpanStyles.None, out interval) == false)
                    return new[] { Binding.DoNothing, Binding.DoNothing };

                TimeSpan hide = showTime + interval;
                return new[] { (object)showTime, (object)hide };

            }
            throw new NotImplementedException();
        }
    }
    public class TextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = (string)value;
            return $"Text: {text.Length} characters";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TranslationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = (string)value;
            return $"Translation: {text.Length} characters";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                if (timeSpan.Milliseconds == 0)
                {
                    if (timeSpan.Hours != 0)
                        return timeSpan.ToString(@"h\:%m\:%s");
                    if (timeSpan.Minutes != 0)
                        return timeSpan.ToString(@"%m\:%s");
                    if (timeSpan.Seconds != 0)
                        return timeSpan.ToString(@"%s");
                    return "0";
                }
                else
                {
                    if (timeSpan.Hours != 0)
                        return timeSpan.ToString(@"h\:%m\:%s\.fff").TrimEnd('0');
                    if (timeSpan.Minutes != 0)
                        return timeSpan.ToString(@"%m\:%s\.fff").TrimEnd('0');
                    return timeSpan.ToString(@"%s\.FFF");
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                TimeSpan interval;
                string[] parsePatterns = new string[] { "%s", "%s\\.fff", "%s\\.ff", "%s\\.f", "%m\\:%s", "%m\\:%s\\.f",
                    "%m\\:%s\\.ff", "%m\\:%s\\.fff", "h\\:%m\\:%s\\.fff","h\\:%m\\:%s\\.f", "h\\:%m\\:%s\\.ff","h\\:%m\\:%s"};
                if (TimeSpan.TryParseExact(str, parsePatterns, culture, TimeSpanStyles.None, out interval))
                    return interval;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
