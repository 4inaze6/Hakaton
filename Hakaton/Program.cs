using Hakaton;
using Hakaton.Data;
using Hakaton.Models;
using System.Globalization;
using System.Text.RegularExpressions;

public class Program
{
    public static readonly GeoContext _context = new();
    const double a = 6378137.0; // радиус Земли (метры)
    const double f = 1.0 / 298.257223563; // плоская эксцентриситет (WGS84)
    const double e2 = 2 * f - f * f; // эксцентриситет^2
    const double fovHorizontal = 62.2;  // Горизонтальное поле зрения в градусах
    const double fovVertical = 48.8;    // Вертикальное поле зрения в градусах

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
                                if (lineNumber > 0)
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
                                    Time = Convert.ToDateTime(ConvertMicrosecondsToDateTime(microS)),
                                    EciQuatW = double.Parse(fields[1], CultureInfo.InvariantCulture),
                                    EciQuatX = double.Parse(fields[2], CultureInfo.InvariantCulture),
                                    EciQuatY = double.Parse(fields[3], CultureInfo.InvariantCulture),
                                    EciQuatZ = double.Parse(fields[4], CultureInfo.InvariantCulture),
                                    Latitude = double.Parse(fields[9], CultureInfo.InvariantCulture),
                                    Longitude = double.Parse(fields[10], CultureInfo.InvariantCulture),
                                    Altitude = double.Parse(fields[11], CultureInfo.InvariantCulture)
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
                string kmlFilePath = @$"C:\Users\kvale\OneDrive\Рабочий стол\САФУ хакатон\САФУ хакатон\Kmls\{Path.GetFileNameWithoutExtension(imaginePath)}.kml";
                Beacon interpolatedBeacon = Interpolate.InterpolateBeacon(beacons, dateTime);
                var corners = CalculateCorners(interpolatedBeacon.Latitude, interpolatedBeacon.Longitude, interpolatedBeacon.Altitude, [interpolatedBeacon.EciQuatW, interpolatedBeacon.EciQuatX, interpolatedBeacon.EciQuatY, interpolatedBeacon.EciQuatZ]);
                KmlGenerator.WriteToKML(kmlFilePath, imaginePath, corners);
                GeoDatum geoDatum = new GeoDatum() { DateTime = dateTime, ImagePath = imaginePath, KmlData = kmlFilePath};
                _context.Add(geoDatum);
                _context.SaveChanges();
            }
        }
    }
    static DateTime ConvertMicrosecondsToDateTime(long microseconds)
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        long ticks = microseconds * 10;
        return epoch.AddTicks(ticks);
    }


    static (double lat, double lon, double alt)[] CalculateCorners(double latitude, double longitude, double altitude, double[] quat)
    {
        double horizontalFOV = 62.2 * Math.PI / 180;
        double verticalFOV = 48.8 * Math.PI / 180;

        // Смещения в метрах
        double[] offsets = new double[]
        {
            Math.Tan(horizontalFOV / 2) * altitude, // Смещение по долготе
            Math.Tan(verticalFOV / 2) * altitude    // Смещение по широте
        };

        var corners = new (double lat, double lon, double alt)[4];

        var transformedOffsets = new (double x, double y, double z)[4];

        transformedOffsets[0] = ApplyQuaternionToOffset(quat, offsets[0], offsets[1], altitude); // Угол 1 (вверх-вправо)
        transformedOffsets[1] = ApplyQuaternionToOffset(quat, -offsets[0], offsets[1], altitude); // Угол 2 (вверх-влево)
        transformedOffsets[2] = ApplyQuaternionToOffset(quat, -offsets[0], -offsets[1], altitude); // Угол 4 (вниз-влево)
        transformedOffsets[3] = ApplyQuaternionToOffset(quat, offsets[0], -offsets[1], altitude); // Угол 3 (вниз-вправо)

        // Расчет углов с учетом кватернионов
        for (int i = 0; i < 4; i++)
        {
            corners[i] = (latitude + (transformedOffsets[i].y / 111320), longitude + (transformedOffsets[i].x / (111320 * Math.Cos(latitude * Math.PI / 180))), altitude);
        }

        return corners;
    }

    static (double x, double y, double z) ApplyQuaternionToOffset(double[] quat, double offsetX, double offsetY, double offsetZ)
    {
        double[] vector = [0, offsetX, offsetY, offsetZ];

        double qw = quat[0], qx = quat[1], qy = quat[2], qz = quat[3];

        // q * v * q^-1
        double[] qConjugate = [qw, -qx, -qy, -qz];

        // q * v
        double[] temp = QuaternionMultiply(quat, vector);

        // (q * v) * q^-1
        double[] result = QuaternionMultiply(temp, qConjugate);

        return (result[1], result[2], result[3]); // Возврат x, y, z
    }

    static double[] QuaternionMultiply(double[] q1, double[] q2)
    {
        return new double[]
        {
            q1[0] * q2[0] - q1[1] * q2[1] - q1[2] * q2[2] - q1[3] * q2[3],
            q1[0] * q2[1] + q1[1] * q2[0] + q1[2] * q2[3] - q1[3] * q2[2],
            q1[0] * q2[2] - q1[1] * q2[3] + q1[2] * q2[0] + q1[3] * q2[1],
            q1[0] * q2[3] + q1[1] * q2[2] - q1[2] * q2[1] + q1[3] * q2[0]
        };
    }
}
