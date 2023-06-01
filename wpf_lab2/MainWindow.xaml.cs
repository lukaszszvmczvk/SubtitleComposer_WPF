using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using SubtitlePlugins;
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
        public static ObservableCollection<DataItem> data { get; set; }
        public static DataItem selectedItem { get; set; }
        public MainWindow()
        {
            data = new ObservableCollection<DataItem>();
            this.DataContext = data;
            InitializeComponent();
            grid.Items.SortDescriptions.Add(new SortDescription(grid.Columns[0].SortMemberPath, ListSortDirection.Ascending));
            grid.Columns[0].SortDirection = ListSortDirection.Ascending;
            LoadPlugins();
        }
        private void LoadPlugins()
        {
            pluginManager = new PluginManager();
            pluginManager.LoadPlugins();
            var save = this.saveMenuItem;
            var open = this.openMenuItem;
            var saveTranslation = this.saveTranslationMenuItem;
            foreach(var plugin in pluginManager._plugins)
            {
                MenuItem newSaveItem = new MenuItem()
                {
                    Header = plugin.Name,
                };
                newSaveItem.Click += pluginSaveText;
                MenuItem newOpenItem = new MenuItem()
                {
                    Header = plugin.Name,
                };
                newOpenItem.Click += pluginOpen;
                MenuItem newSaveTranslationItem = new MenuItem()
                {
                    Header = plugin.Name,
                };
                newSaveTranslationItem.Click += pluginSaveTranslation;
                saveTranslation.Items.Add(newSaveTranslationItem);
                save.Items.Add(newSaveItem);
                open.Items.Add(newOpenItem);
            }
        }
        private void pluginSaveText(object sender, RoutedEventArgs e)
        {
            var menuitem = sender as MenuItem;
            if(menuitem != null)
            {
                var name = menuitem.Header;
                var plugin = pluginManager._plugins.Find(x => x.Name == name);
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = $"Pliki (*{plugin.Extension})|*{plugin.Extension}";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.FileName = "NowyPlik" + plugin.Extension;
                saveFileDialog.DefaultExt = plugin.Extension;
                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    string filePath = saveFileDialog.FileName;
                    plugin.Save(filePath, data);
                }
            }
        }
        private void pluginSaveTranslation(object sender, RoutedEventArgs e)
        {
            var menuitem = sender as MenuItem;
            if (menuitem != null)
            {
                var name = menuitem.Header;
                var plugin = pluginManager._plugins.Find(x => x.Name == name);
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = $"Pliki (*{plugin.Extension})|*{plugin.Extension}";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.FileName = "NowyPlik" + plugin.Extension;
                saveFileDialog.DefaultExt = plugin.Extension;
                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    string filePath = saveFileDialog.FileName;
                    plugin.Save(filePath, data, true);
                }
            }
        }
        private void pluginOpen(object sender, RoutedEventArgs e)
        {
            var menuitem = sender as MenuItem;
            if (menuitem != null)
            {
                var name = menuitem.Header;
                var plugin = pluginManager._plugins.Find(x => x.Name == name);
                OpenFileDialog openFileDialog = new OpenFileDialog();

                openFileDialog.Filter = $"Pliki (*{plugin.Extension})|*{plugin.Extension}";
                openFileDialog.FilterIndex = 1;

                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    string filePath = openFileDialog.FileName;
                    data = plugin.Load(filePath);
                    grid.ItemsSource = data;
                }
            }
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
                    subtitles = data.Where(ob => ob.sTime <= time && ob.hTime >= time).Select(ob => ob.translation).ToList();
                else
                    subtitles = data.Where(ob=>ob.sTime <= time && ob.hTime >= time).Select(ob=>ob.text).ToList();
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
            try
            {
                var time = data.Max(ob => ob.hTime);
                var item = new DataItem();
                item.hTime = item.sTime = time;
                data.Add(item);
            }
            catch { }
        }
        private void addAfterClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = grid.SelectedItems.Cast<object>().ToList();
                var it = items[items.Count-1] as DataItem;
                if (it == null)
                    items.RemoveAt(items.Count-1);
                var time = items.Max(ob => ((DataItem)ob).hTime);
                var item = new DataItem();
                item.hTime=item.sTime=time;
                data.Add(item);
            }
            catch { }
        }
        private void deleteClick(object sender, RoutedEventArgs e)
        {
            var items = grid.SelectedItems.Cast<object>().ToArray();
            for (int i = 0; i < items.Length; ++i)
            {
                var it = items[i] as DataItem;
                if (it == null) 
                    continue;
                data.Remove((DataItem)items[i]);
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
        private void initializeNewItem(object sender, InitializingNewItemEventArgs e)
        {
            DataItem newItem = e.NewItem as DataItem;
            if (MainWindow.data.Count > 0)
            {
                newItem.sTime = MainWindow.data.Max(x => x.hTime);
                newItem.hTime = MainWindow.data.Max(x => x.hTime);
            }
            else
            {
                newItem.sTime = TimeSpan.Zero;
                newItem.hTime= TimeSpan.Zero;
            }
        }
    }
}