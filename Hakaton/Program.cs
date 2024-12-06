using Hakaton;
using Hakaton.Models;
using System.Globalization;
using System.Text.RegularExpressions;

public class Program
{
    const double EarthRadius = 6371000; // Радиус Земли (м)
    const double degToRad = Math.PI / 180.0;
    const double radToDeg = 180.0 / Math.PI;
    const double f = 1.0 / 298.257223563; // плоская эксцентриситет (WGS84)
    const double e2 = 2 * f - f * f; // эксцентриситет^2
    const double fovHorizontal = 62.2;  // Горизонтальное поле зрения в градусах
    const double fovVertical = 48.8;    // Вертикальное поле зрения в градусах

    private static readonly GeoService _service = new();

    private static async Task Main(string[] args)
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
                    using (StreamReader reader = new(logFile))
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
                KmlGenerator.WriteToKML(kmlFilePath, imaginePath, geo[0], geo[1], geo[2], geo[3]);
                GeoDatum geoDatum = new() { DateTime = dateTime, ImagePath = imaginePath, KmlData = kmlFilePath };
                if (await _service.SearchGeoData(geoDatum))
                    await _service.AddGeoData(geoDatum);
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

        double tiltAngleDeg = GetTiltAngleDeg(centerLatitude, centerLongitude, centerAltitude, beacon.EciQuatW, beacon.EciQuatX, beacon.EciQuatY, beacon.EciQuatZ); // Угол наклона камеры в градусах
        Console.WriteLine(tiltAngleDeg);

        // Перевод углов в радианы
        double horizontalFovRad = DegreesToRadians(fovHorizontal);
        double verticalFovRad = DegreesToRadians(fovVertical);
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

    public static double GetTiltAngleDeg(double latitude, double longitude, double altitude, double eciQuatW, double eciQuatX, double eciQuatY, double eciQuatZ)
    {
        double phi = latitude * Math.PI / 180;  
        double lambda = longitude * Math.PI / 180;
        double satelliteHeight = EarthRadius + altitude;

        // Вектор направления на Землю (центр планеты)
        double[] directionToEarth = {
            satelliteHeight * Math.Cos(phi) * Math.Cos(lambda),
            satelliteHeight * Math.Cos(phi) * Math.Sin(lambda),
            satelliteHeight * Math.Sin(phi)
        };

        // Вектор направления камеры в локальной системе координат спутника (взяли -Z)
        double[] cameraDirectionLocal = { 0, 0, -1 };

        // Преобразуем направление камеры в глобальную систему через кватернион
        double[] cameraDirectionGlobal = QuaternionRotate(cameraDirectionLocal, eciQuatW, eciQuatX, eciQuatY, eciQuatZ);

        // Угол между векторами
        double angleRad = AngleBetweenVectors(cameraDirectionGlobal, directionToEarth);
        double angleDeg = angleRad * 180 / Math.PI;

        return angleDeg;
    }

    // Функция для поворота вектора через кватернион
    static double[] QuaternionRotate(double[] vector, double w, double x, double y, double z)
    {
        double[] quatVector = { 0, vector[0], vector[1], vector[2] };

        double norm = Math.Sqrt(w * w + x * x + y * y + z * z);
        double invW = w / norm;
        double invX = -x / norm;
        double invY = -y / norm;
        double invZ = -z / norm;

        // Умножение кватернионов: RotatedVector = Rotation * Vector * Rotation^-1
        double[] rotatedQuat = QuaternionMultiply(
            QuaternionMultiply(new double[] { w, x, y, z }, quatVector),
            new double[] { invW, invX, invY, invZ }
        );

        return new double[] { rotatedQuat[1], rotatedQuat[2], rotatedQuat[3] };
    }

    // Функция для умножения двух кватернионов
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

    // Функция для вычисления угла между двумя векторами
    static double AngleBetweenVectors(double[] v1, double[] v2)
    {
        double dotProduct = v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
        double magnitudeV1 = Math.Sqrt(v1[0] * v1[0] + v1[1] * v1[1] + v1[2] * v1[2]);
        double magnitudeV2 = Math.Sqrt(v2[0] * v2[0] + v2[1] * v2[1] + v2[2] * v2[2]);
        return Math.Acos(dotProduct / (magnitudeV1 * magnitudeV2));
    }

    // Метод для перевода градусов в радианы
    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
