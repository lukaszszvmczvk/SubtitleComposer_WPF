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

        public ObservableCollection<DataItem> Load(string path, bool isTranslation=false)
        {
            var dataItems = new ObservableCollection<DataItem>();

            try
            {
                string[] lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (int.TryParse(lines[i], out int sequenceNumber))
                    {
                        string[] timeValues = lines[i + 1].Split("-->");
                        TimeSpan startTime = TimeSpan.Parse(timeValues[0].Trim());
                        TimeSpan endTime = TimeSpan.Parse(timeValues[1].Trim());

                        string text = string.Empty;
                        i += 2;

                        while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
                        {
                            text += lines[i] + Environment.NewLine;
                            i++;
                        }

                        text = text.TrimEnd(Environment.NewLine.ToCharArray());

                        DataItem item = new DataItem();
                        item.sTime = startTime;
                        item.hTime = endTime;
                        if (isTranslation)
                            item.translation = text;
                        else
                            item.text = text;
                        dataItems.Add(item);
                    }
                }
            }
            catch ()
            { }

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
                        if (saveTranslation == true)
                            writer.WriteLine(dataItem.translation);
                        else
                            writer.WriteLine(dataItem.text);
                        writer.WriteLine();
                        lineNumber++;
                    }
                }
            }
            catch (Exception ex)
            {
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
