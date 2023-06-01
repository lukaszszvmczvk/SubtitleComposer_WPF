using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace SubtitlePlugins
{
    public class DataItem
    {
        public TimeSpan sTime { get; set; }
        public TimeSpan hTime { get; set; }
        public string? text { get; set; }
        public string? translation { get; set; }
    }
    public interface ISubtitlesPlugin
    {
        string Name { get; }
        string Extension { get; }
        ObservableCollection<DataItem> Load(string path);
        void Save(string path, ObservableCollection<DataItem> subtitles,bool saveTranslation=false);
    }
}
