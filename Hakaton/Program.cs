using Hakaton;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

internal class Program
{
    private static void Main(string[] args)
    {
        string rootPath = @"C:\Users\kvale\OneDrive\Рабочий стол\САФУ хакатон\САФУ хакатон\Photo_telemetry_hackaton";
        string[] imaginePaths = [];
        string logs = "";
        string logFile = "";
        string photo = "";
        string imaginePath = "";

        if (Directory.Exists(rootPath))
        {
            string[] directoryEntries = Directory.GetDirectories(rootPath);
            for (int i = 0; i < directoryEntries.Length; i++)
            {
                DateTime dateTime = DateTime.Now;
                imaginePath = Directory.GetFiles(directoryEntries[i], "*.jpg", SearchOption.AllDirectories)[0];
                Console.WriteLine(imaginePath);
                string dateTimePattern = @"(\d{4})-(\d{2})-(\d{2})_(\d{2})-(\d{2})-(\d{2})";

                Match match = Regex.Match(imaginePath, dateTimePattern);

                if (match.Success)
                {
                    int year = int.Parse(match.Groups[1].Value);
                    int month = int.Parse(match.Groups[2].Value);
                    int day = int.Parse(match.Groups[3].Value);
                    int hour = int.Parse(match.Groups[4].Value);
                    int minute = int.Parse(match.Groups[5].Value);
                    int second = int.Parse(match.Groups[6].Value);

                    dateTime = new DateTime(year, month, day, hour, minute, second);
                }
                Console.WriteLine("dt - " +dateTime);
                logs = Directory.GetDirectories(directoryEntries[i])[0];
                logFile = Directory.GetFiles(logs)[0];
                var beacons = new List<Beacon>();

                if (File.Exists(logFile))
                {
                    using (StreamReader reader = new StreamReader(logFile))
                    {
                        int lineNumber = 0;
                        bool skipFirstTwoLines = true;

                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            lineNumber++;

                            if (skipFirstTwoLines)
                            {
                                if (lineNumber > 2)
                                {
                                    skipFirstTwoLines = false;
                                }
                                continue;
                            }
                            if (line != "")
                            {
                                string[] fields = line.Split(',');
                                long microS = long.Parse(fields[0]);
                                Beacon data = new()
                                {
                                    Time = ConvertMicrosecondsToDateTime(microS),
                                    EciQuatW = double.Parse(fields[1], CultureInfo.InvariantCulture),
                                    EciQuatX = double.Parse(fields[2], CultureInfo.InvariantCulture),
                                    EciQuatY = double.Parse(fields[3], CultureInfo.InvariantCulture),
                                    EciQuatZ = double.Parse(fields[4], CultureInfo.InvariantCulture),
                                    Latitude = double.Parse(fields[5], CultureInfo.InvariantCulture),
                                    Longitude = double.Parse(fields[6], CultureInfo.InvariantCulture),
                                    Altitude = double.Parse(fields[7], CultureInfo.InvariantCulture)
                                };
                                Console.WriteLine(data.Time);
                                beacons.Add(data);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Файл не найден.");
                }
                string kmlFilePath = @$"C:\Users\kvale\OneDrive\Рабочий стол\САФУ хакатон\САФУ хакатон\Kmls\{Path.GetFileNameWithoutExtension(imaginePath)}.kml";
                Beacon interpolatedBeacon = Interpolate.InterpolateBeacon(beacons, dateTime);
                KmlGenerator.WriteToKML(interpolatedBeacon, kmlFilePath);
            }
        }

        static DateTime ConvertMicrosecondsToDateTime(long microseconds)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long ticks = microseconds * 10; 
            return epoch.AddTicks(ticks);
        }
    }
}