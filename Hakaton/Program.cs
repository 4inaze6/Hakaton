using Hakaton;
using System.Text.RegularExpressions;

string rootPath = @"C:\Users\kvale\OneDrive\Рабочий стол\САФУ хакатон\САФУ хакатон\Photo_telemetry_hackaton";
string[] imaginePaths = [];
string logs = "";
string logFile = "";
string photo = "";
if (Directory.Exists(rootPath))
{
    string[] directoryEntries = Directory.GetDirectories(rootPath);
    for (int i = 0; i < directoryEntries.Length; i++)
    {
        imaginePaths.Append(Directory.GetFiles(directoryEntries[i], "*.jpg", SearchOption.AllDirectories)[0]);
        string dateTimePattern = @"(\d{4})-(\d{2})-(\d{2})_(\d{2})-(\d{2})-(\d{2})";

        Match match = Regex.Match(imaginePaths[0], dateTimePattern);

        if (match.Success)
        {
            int year = int.Parse(match.Groups[1].Value);
            int month = int.Parse(match.Groups[2].Value);
            int day = int.Parse(match.Groups[3].Value);
            int hour = int.Parse(match.Groups[4].Value);
            int minute = int.Parse(match.Groups[5].Value);
            int second = int.Parse(match.Groups[6].Value);

            DateTime dateTime = new DateTime(year, month, day, hour, minute, second);
        }
        logs = Directory.GetDirectories(rootPath)[0];
        logFile = Directory.GetFiles(logs)[0];
        var beacons = new List<DataEntry>();

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
                    if (line != string.Empty)
                    {
                        string[] fields = line.Split(',');
                        DataEntry data = new()
                        {
                            TimeUsec = Convert.ToInt32(fields[0]),
                            EciQuatW = Convert.ToDouble(fields[1]),
                            EciQuatX = Convert.ToDouble(fields[2]),
                            EciQuatY = Convert.ToDouble(fields[3]),
                            EciQuatZ = Convert.ToDouble(fields[4]),
                            Latitude = Convert.ToDouble(fields[5]),
                            Longitude = Convert.ToDouble(fields[6]),
                            Altitude = Convert.ToDouble(fields[7])
                        };
                        beacons.Add(data);
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Файл не найден.");
        }
        string kmlFilePath = @"C:\Users\kvale\OneDrive\Рабочий стол\САФУ хакатон\САФУ хакатон\Kmls";
        GenerateKmlFile(beacons, kmlFilePath);
    }
}
    