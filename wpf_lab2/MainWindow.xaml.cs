using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace wpf_lab2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PluginManager pluginManager;
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private DispatcherTimer timer;
        public static ObservableCollection<DataItem> rows { get; set; }
        public static DataItem selectedItem { get; set; }
        public MainWindow()
        {
            rows = new ObservableCollection<DataItem>();
            this.DataContext = rows;
            InitializeComponent();
            LoadPlugins();
        }
        private void LoadPlugins()
        {
            pluginManager = new PluginManager();
            //pluginManager.LoadPlugins();
        }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void MenuItem_About(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is simple App.", "Simple app", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void TranslationChecked(object sender, RoutedEventArgs e)
        {
            this.grid.Columns[3].Visibility = Visibility.Visible;
            this.TranslationBox.Visibility = Visibility.Visible;
            this.textGroupBox.SetValue(Grid.ColumnSpanProperty, 1);
        }
        private void TranslationUnchecked(object sender, RoutedEventArgs e)
        {
            this.grid.Columns[3].Visibility = Visibility.Collapsed;
            this.TranslationBox.Visibility = Visibility.Collapsed;
            this.textGroupBox.SetValue(Grid.ColumnSpanProperty, 2);
        }
        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Viedo files |*.avi;*.mp4;*.mkv;*.mov;*.wmv";

            if (openFileDialog.ShowDialog() == true)
            {
                this.mediaPlayer.Source = new Uri(openFileDialog.FileName);
            }
        }
        private void playClick(object sender, RoutedEventArgs e)
        {
            this.mediaPlayer.Play();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += timer_Tick;
            timer.Start();
            mediaPlayerIsPlaying = true;
            mediaPlayer.Volume = (double)volumeSlider.Value;
            mediaPlayerIsPlaying = true;
        }
        private void pauseClick(object sender, RoutedEventArgs e)
        {
            mediaPlayerIsPlaying = false;
            this.mediaPlayer.Pause();
            timer.Stop();
        }
        private void stopClick(object sender, RoutedEventArgs e)
        {
            mediaPlayerIsPlaying = false;
            this.mediaPlayer.Stop();
            timer.Stop();
        }
        private void Element_MediaOpened(object sender, EventArgs e)
        {
            timelineSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            timelineSlider.Minimum = 0;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        private void Element_MediaEnded(object sender, EventArgs e)
        {
            mediaPlayer.Stop();
            timer.Stop();
        }
        private void mouseWheel(object sender, MouseWheelEventArgs e)
        {
            int delta = e.Delta;
            if (delta > 0)
            {
                if (mediaPlayer.Volume < 1.0)
                    mediaPlayer.Volume += 0.1;
            }
            else
            {
                if (mediaPlayer.Volume > 0.0)
                    mediaPlayer.Volume -= 0.1;
            }
            e.Handled = true;
        }
        private void sliderDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }
        private void sliderDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mediaPlayer.Position = TimeSpan.FromMilliseconds(timelineSlider.Value);
        }
        private void sliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timeTextBlock.Text = TimeSpan.FromMilliseconds(timelineSlider.Value).ToString(@"hh\:mm\:ss\.fff");
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mediaPlayer.Source != null) && (mediaPlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                timelineSlider.Value = mediaPlayer.Position.TotalMilliseconds;
                var time = new TimeSpan(0, 0, 0, 0, (int)mediaPlayer.Position.TotalMilliseconds);
                List<string> subtitles;
                if(check.IsChecked == true)
                    subtitles = rows.Where(ob => ob.sTime <= time && ob.hTime >= time).Select(ob => ob.translation).ToList();
                else
                    subtitles = rows.Where(ob=>ob.sTime <= time && ob.hTime >= time).Select(ob=>ob.text).ToList();
                if (subtitles.Count == 0)
                    subtitlesTextBlock.Visibility = Visibility.Collapsed;
                else
                    subtitlesTextBlock.Visibility = Visibility.Visible;
                string str = String.Join('\n', subtitles);
                subtitlesTextBlock.Text = str;
            }
        }
        private void addClick(object sender, RoutedEventArgs e)
        {
            var time = rows.Max(ob => ob.hTime);
            var item = new DataItem();
            item.hTime = item.sTime = time;
            rows.Add(item);
        }
        private void addAfterClick(object sender, RoutedEventArgs e)
        {
            var items = grid.SelectedItems.Cast<object>().ToList();
            var it = items[items.Count-1] as DataItem;
            if (it == null)
                items.RemoveAt(items.Count-1);
            var time = items.Max(ob => ((DataItem)ob).hTime);
            var item = new DataItem();
            item.hTime=item.sTime=time;
            rows.Add(item);
        }
        private void deleteClick(object sender, RoutedEventArgs e)
        {
            var items = grid.SelectedItems.Cast<object>().ToArray();
            for (int i = 0; i < items.Length; ++i)
            {
                var it = items[i] as DataItem;
                if (it == null) 
                    continue;
                rows.Remove((DataItem)items[i]);
            }
        }
        private void lbDown(object sender, MouseButtonEventArgs e)
        {
            if(mediaPlayerIsPlaying == true)
            {
                mediaPlayerIsPlaying = false;
                this.mediaPlayer.Pause();
                timer.Stop();
            }
            else
            {
                this.mediaPlayer.Play();
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(100);
                timer.Tick += timer_Tick;
                timer.Start();
                mediaPlayerIsPlaying = true;
                mediaPlayer.Volume = (double)volumeSlider.Value;
                mediaPlayerIsPlaying = true;
            }
        }
    }
    public class DataItem
    {
        public TimeSpan sTime { get; set; }
        public TimeSpan hTime { get; set; }
        public string? text { get; set; }
        public string? translation { get; set; }
        public DataItem()
        {
          if(MainWindow.rows.Count > 0)
          {
              this.sTime = MainWindow.rows.Max(x => x.hTime);
              this.hTime = MainWindow.rows.Max(x => x.hTime);
          }
          else
          {
              this.sTime = TimeSpan.Zero;
              this.hTime= TimeSpan.Zero;
          }
        }
    }
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
            return s+converter.Convert(duration, null, null, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            TimeSpan[] times = new TimeSpan[2];
            if(value is string str)
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
                    if(timeSpan.Hours!=0)
                        return timeSpan.ToString(@"h\:%m\:%s");
                    if(timeSpan.Minutes!=0)
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
                if (TimeSpan.TryParseExact(str, parsePatterns, culture, TimeSpanStyles.None,out interval))
                    return interval;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}