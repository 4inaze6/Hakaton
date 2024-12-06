using System;
using System.Xml;
using System.IO;

class SatelliteVisibilityCalculator
{
    // Константы
    private const double EarthRadius = 6371000;  // Радиус Земли в метрах

    public static void Main()
    {
        // Данные спутника и параметры поля зрения
        double latitude = 32.5;  // Средняя широта снимка
        double longitude = 87.4;  // Средняя долгота снимка
        double altitude = 538816.5;  // Высота спутника в метрах

        // Поле зрения камеры
        double horizontalFovDeg = 62.2;  // Горизонтальное поле зрения в градусах
        double verticalFovDeg = 48.8;    // Вертикальное поле зрения в градусах

        // Угол наклона камеры
        double tiltAngleDeg = 30;  // Угол наклона камеры в градусах

        // Перевод углов в радианы
        double horizontalFovRad = DegreesToRadians(horizontalFovDeg);
        double verticalFovRad = DegreesToRadians(verticalFovDeg);
        double tiltAngleRad = DegreesToRadians(tiltAngleDeg);

        // Вычисление эффективной высоты
        double effectiveAltitude = altitude * Math.Cos(tiltAngleRad);

        // Вычисление горизонтального и вертикального диапазонов
        double horizontalRange = 2 * effectiveAltitude * Math.Tan(horizontalFovRad / 2);
        double verticalRange = 2 * effectiveAltitude * Math.Tan(verticalFovRad / 2);

        // Перевод диапазонов в градусы широты и долготы
        double latitudeShift = (verticalRange / 2) / EarthRadius * (180 / Math.PI);
        double longitudeShift = (horizontalRange / 2) / (EarthRadius * Math.Cos(DegreesToRadians(latitude))) * (180 / Math.PI);

        // Вычисляем координаты углов области видимости
        double northLatitude = latitude + latitudeShift;
        double southLatitude = latitude - latitudeShift;
        double eastLongitude = longitude + longitudeShift;
        double westLongitude = longitude - longitudeShift;

        // Создание KML файла
        CreateKmlFile(northLatitude, southLatitude, eastLongitude, westLongitude);
    }

    // Метод для перевода градусов в радианы
    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    // Метод для создания KML файла
    private static void CreateKmlFile(double northLat, double southLat, double eastLon, double westLon)
    {
        // Создаем XML документ
        XmlDocument xmlDoc = new XmlDocument();

        // Создаем корневой элемент KML
        XmlElement kmlElement = xmlDoc.CreateElement("kml");
        kmlElement.SetAttribute("xmlns", "http://www.opengis.net/kml/2.2");
        xmlDoc.AppendChild(kmlElement);

        // Создаем элемент Document
        XmlElement documentElement = xmlDoc.CreateElement("Document");
        kmlElement.AppendChild(documentElement);

        // Создаем Placemark
        XmlElement placemarkElement = xmlDoc.CreateElement("Placemark");
        documentElement.AppendChild(placemarkElement);

        // Добавляем описание
        XmlElement descriptionElement = xmlDoc.CreateElement("description");
        descriptionElement.InnerText = "Скорректированная область видимости";
        placemarkElement.AppendChild(descriptionElement);

        // Создаем элемент Polygon
        XmlElement polygonElement = xmlDoc.CreateElement("Polygon");
        placemarkElement.AppendChild(polygonElement);

        // Создаем outerBoundaryIs
        XmlElement outerBoundaryIsElement = xmlDoc.CreateElement("outerBoundaryIs");
        polygonElement.AppendChild(outerBoundaryIsElement);

        // Создаем LinearRing
        XmlElement linearRingElement = xmlDoc.CreateElement("LinearRing");
        outerBoundaryIsElement.AppendChild(linearRingElement);

        // Создаем элемент coordinates
        XmlElement coordinatesElement = xmlDoc.CreateElement("coordinates");
        linearRingElement.AppendChild(coordinatesElement);

        // Формируем координаты для полигона (все координаты в формате lon,lat,0)
        string coordinatesText = $"{westLon.ToString().Replace(',', '.')},{northLat.ToString().Replace(',', '.')},0 {eastLon.ToString().Replace(',', '.')},{northLat.ToString().Replace(',', '.')},0 {eastLon.ToString().Replace(',', '.')},{southLat.ToString().Replace(',', '.')},0 {westLon.ToString().Replace(',', '.')},{southLat.ToString().Replace(',', '.')},0 {westLon.ToString().Replace(',', '.')},{northLat.ToString().Replace(',', '.')},0";
        coordinatesElement.InnerText = coordinatesText;

        // Сохраняем файл KML
        string filePath = "snapshot_area_corrected.kml";
        xmlDoc.Save(filePath);

        Console.WriteLine($"KML файл сохранен по пути: {filePath}");
    }
}
