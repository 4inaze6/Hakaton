using Hakaton;
using Hakaton.Data;
using Hakaton.Models;
using System.Globalization;
using System.Text.RegularExpressions;

public class Program
{
    public static readonly GeoContext _context = new();
    double earthRadius = 6371000; // Радиус Земли (м)
    double degToRad = Math.PI / 180.0;
    double radToDeg = 180.0 / Math.PI;
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
                var geo = CalculateImageAreaFromBeacons(interpolatedBeacon);
                //var corners = CalculateCorners(interpolatedBeacon.Latitude, interpolatedBeacon.Longitude, interpolatedBeacon.Altitude, [interpolatedBeacon.EciQuatW, interpolatedBeacon.EciQuatX, interpolatedBeacon.EciQuatY, interpolatedBeacon.EciQuatZ]);
                KmlGenerator.WriteToKML(kmlFilePath, imaginePath, geo[0], geo[1], geo[2], geo[3]);
                GeoDatum geoDatum = new GeoDatum() { DateTime = dateTime, ImagePath = imaginePath, KmlData = kmlFilePath };
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

    public static double[] CalculateImageAreaFromBeacons(Beacon beacon)
    {
        // Константы
        double earthRadius = 6371000; // Радиус Земли (м)
        double degToRad = Math.PI / 180.0;
        double radToDeg = 180.0 / Math.PI;

        // Найти центр по широте, долготе и средней высоте
        double centerLatitude = beacon.Latitude;
        double centerLongitude = beacon.Longitude;
        double centerAltitude = beacon.Altitude;

        double tiltAngleDeg = 30; // Угол наклона камеры в градусах

        // Перевод углов в радианы
        double horizontalFovRad = DegreesToRadians(fovHorizontal);
        double verticalFovRad = DegreesToRadians(fovHorizontal);
        double tiltAngleRad = DegreesToRadians(tiltAngleDeg);

        // Вычисление эффективной высоты
        double effectiveAltitude = centerAltitude * Math.Cos(tiltAngleRad);

        // Вычисление горизонтального и вертикального диапазонов
        double horizontalRange = 2 * effectiveAltitude * Math.Tan(horizontalFovRad / 2);
        double verticalRange = 2 * effectiveAltitude * Math.Tan(verticalFovRad / 2);

        // Перевод диапазонов в градусы широты и долготы
        double latitudeShift = (verticalRange / 2) / earthRadius * (180 / Math.PI);
        double longitudeShift = (horizontalRange / 2) / (earthRadius * Math.Cos(DegreesToRadians(centerLatitude))) * (180 / Math.PI);

        // Вычисляем координаты углов области видимости
        double northLatitude = centerLatitude + latitudeShift;
        double southLatitude = centerLatitude - latitudeShift;
        double eastLongitude = centerLongitude + longitudeShift;
        double westLongitude = centerLongitude - longitudeShift;

        var geoPos = new double[4];

        geoPos[0] = northLatitude;
        geoPos[1] = southLatitude;
        geoPos[2] = eastLongitude;
        geoPos[3] = westLongitude;

        return geoPos;
    }

    // Метод для перевода градусов в радианы
    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
    //static (double lat, double lon, double alt)[] CalculateCorners(double latitude, double longitude, double altitude, double[] quat)
    //{
    //    double horizontalFOV = 62.2 * Math.PI / 180;
    //    double verticalFOV = 48.8 * Math.PI / 180;

    //    // Радиус Земли в метрах (средний радиус)
    //    double earthRadius = 6371000;

    //    // Вычисление смещений по широте и долготе с учётом радиуса Земли
    //    double verticalOffset = Math.Tan(verticalFOV / 2) * altitude;  // Смещение по вертикали
    //    double horizontalOffset = Math.Tan(horizontalFOV / 2) * altitude; // Смещение по горизонтали

    //    // Преобразование смещений в радианы
    //    double deltaLat = verticalOffset / earthRadius;  // Смещение по широте
    //    double deltaLon = horizontalOffset / (earthRadius * Math.Cos(latitude * Math.PI / 180));  // Смещение по долготе

    //    var corners = new (double lat, double lon, double alt)[4];

    //    var transformedOffsets = new (double x, double y, double z)[4];

    //    // Применяем кватернион для поворота
    //    transformedOffsets[0] = ApplyQuaternionToOffset(quat, horizontalOffset, verticalOffset, altitude); // Верхний правый угол
    //    transformedOffsets[1] = ApplyQuaternionToOffset(quat, -horizontalOffset, verticalOffset, altitude); // Верхний левый угол
    //    transformedOffsets[2] = ApplyQuaternionToOffset(quat, -horizontalOffset, -verticalOffset, altitude); // Нижний левый угол
    //    transformedOffsets[3] = ApplyQuaternionToOffset(quat, horizontalOffset, -verticalOffset, altitude); // Нижний правый угол

    //    // Перевод смещений в новые координаты с учётом сферической модели
    //    for (int i = 0; i < 4; i++)
    //    {
    //        corners[i] = (
    //            latitude + (transformedOffsets[i].y / earthRadius) * (180 / Math.PI), // Преобразование смещения по широте
    //            longitude + (transformedOffsets[i].x / (earthRadius * Math.Cos(latitude * Math.PI / 180))) * (180 / Math.PI), // Преобразование смещения по долготе
    //            altitude
    //        );
    //    }

    //    return corners;
    //}

    //static (double x, double y, double z) ApplyQuaternionToOffset(double[] quat, double offsetX, double offsetY, double offsetZ)
    //{
    //    double[] vector = [0, offsetX, offsetY, offsetZ];

    //    double qw = quat[0], qx = quat[1], qy = quat[2], qz = quat[3];

    //    // q * v * q^-1
    //    double[] qConjugate = [qw, -qx, -qy, -qz];

    //    // q * v
    //    double[] temp = QuaternionMultiply(quat, vector);

    //    // (q * v) * q^-1
    //    double[] result = QuaternionMultiply(temp, qConjugate);

    //    return (result[1], result[2], result[3]); // Возврат x, y, z
    //}

    //static double[] QuaternionMultiply(double[] q1, double[] q2)
    //{
    //    return new double[]
    //    {
    //        q1[0] * q2[0] - q1[1] * q2[1] - q1[2] * q2[2] - q1[3] * q2[3],
    //        q1[0] * q2[1] + q1[1] * q2[0] + q1[2] * q2[3] - q1[3] * q2[2],
    //        q1[0] * q2[2] - q1[1] * q2[3] + q1[2] * q2[0] + q1[3] * q2[1],
    //        q1[0] * q2[3] + q1[1] * q2[2] - q1[2] * q2[1] + q1[3] * q2[0]
    //    };
    //}
}
