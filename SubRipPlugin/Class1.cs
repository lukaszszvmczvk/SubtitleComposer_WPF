namespace wpf_lab2
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class SrtSubtitlesPlugin : ISubtitlesPlugin
    {
        public string Name => "SubRip";
        public string Extension => ".srt";

        public ICollection<DataItem> Load(string path)
        {
            var dataItems = new List<DataItem>();

            try
            {
                string[] lines = File.ReadAllLines(path);

                for (int i = 0; i < lines.Length; i += 4)
                {
                    int lineNumber = int.Parse(lines[i]);
                    string timeRange = lines[i + 1];
                    string text = lines[i + 2];

                    TimeSpan startTime, endTime;
                    ParseTimeRange(timeRange, out startTime, out endTime);

                    var dataItem = new DataItem
                    {
                        sTime = startTime,
                        hTime = endTime,
                        text = text
                    };

                    dataItems.Add(dataItem);
                }
            }
            catch (Exception ex)
            {
                // Obsługa błędów wczytywania danych
                Console.WriteLine($"Błąd wczytywania danych: {ex.Message}");
            }

            return dataItems;
        }

        public void Save(string path, ICollection<DataItem> dataItems)
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