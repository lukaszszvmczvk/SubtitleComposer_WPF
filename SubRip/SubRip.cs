using SubtitlePlugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRip
{
    public class SrtSubtitlesPlugin : ISubtitlesPlugin
    {
        public string Name => "SubRip";
        public string Extension => ".srt";

        public ObservableCollection<DataItem> Load(string path)
        {
            var dataItems = new ObservableCollection<DataItem>();

            try
            {
                string[] lines = File.ReadAllLines(path);

                for (int i = 0; i < lines.Length; i += 5)
                {
                    int lineNumber = int.Parse(lines[i]);
                    string timeRange = lines[i + 1];
                    string text = lines[i + 2];
                    string translation = lines[i + 3];

                    TimeSpan startTime, endTime;
                    ParseTimeRange(timeRange, out startTime, out endTime);
                    var dataItem = new DataItem();
                    dataItem.sTime = startTime;
                    dataItem.hTime = endTime;
                    dataItem.text=text;
                    dataItem.translation = translation;
                    dataItems.Add(dataItem);
                }
            }
            catch (Exception ex)
            {
            }

            return dataItems;
        }
        public void Save(string path, ObservableCollection<DataItem> dataItems,bool saveTranslation=false)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    int lineNumber = 1;

                    foreach (DataItem dataItem in dataItems)
                    {
                        writer.WriteLine(lineNumber);
                        writer.WriteLine($"{FormatTimeRange(dataItem.sTime)} --> {FormatTimeRange(dataItem.hTime)}");
                        writer.WriteLine(dataItem.text);
                        if (saveTranslation == true)
                            writer.WriteLine(dataItem.translation);
                        else
                            writer.WriteLine("");
                        writer.WriteLine();
                        lineNumber++;
                    }
                }
            }
            catch (Exception ex)
            {
                // Obsługa błędów zapisu danych
                Console.WriteLine($"Błąd zapisu danych: {ex.Message}");
            }
        }

        private void ParseTimeRange(string timeRange, out TimeSpan startTime, out TimeSpan endTime)
        {
            string[] parts = timeRange.Split(new[] { " --> " }, StringSplitOptions.RemoveEmptyEntries);
            startTime = TimeSpan.Parse(parts[0]);
            endTime = TimeSpan.Parse(parts[1]);
        }

        private string FormatTimeRange(TimeSpan time)
        {
            return time.ToString(@"hh\:mm\:ss\,fff");
        }
    }
}
