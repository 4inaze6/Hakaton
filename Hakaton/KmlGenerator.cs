using System.Xml;

namespace Hakaton
{
    public class KmlGenerator
    {
        public static void WriteToKML(string fileName, string imagePath, double northLat, double southLat, double eastLon, double westLon)
        {
            XmlDocument xmlDoc = new XmlDocument();

            // Создаем корневой элемент KML
            XmlElement kmlElement = xmlDoc.CreateElement("kml");
            kmlElement.SetAttribute("xmlns", "http://www.opengis.net/kml/2.2");
            xmlDoc.AppendChild(kmlElement);

            // Создаем элемент Document
            XmlElement documentElement = xmlDoc.CreateElement("Document");
            kmlElement.AppendChild(documentElement);

            // Создаем Style для иконки
            XmlElement styleElement = xmlDoc.CreateElement("Style");
            styleElement.SetAttribute("id", "polygonStyle");
            documentElement.AppendChild(styleElement);

            // Создаем IconStyle для добавления изображения
            XmlElement iconStyleElement = xmlDoc.CreateElement("IconStyle");
            styleElement.AppendChild(iconStyleElement);

            // Добавляем ссылку на изображение
            XmlElement iconElement = xmlDoc.CreateElement("Icon");
            iconStyleElement.AppendChild(iconElement);
            XmlElement hrefElement = xmlDoc.CreateElement("href");
            hrefElement.InnerText = imagePath;
            iconElement.AppendChild(hrefElement);

            // Создаем Placemark
            XmlElement placemarkElement = xmlDoc.CreateElement("Placemark");
            documentElement.AppendChild(placemarkElement);

            // Применяем стиль к полигону
            XmlElement styleUrlElement = xmlDoc.CreateElement("styleUrl");
            styleUrlElement.InnerText = "#polygonStyle";
            placemarkElement.AppendChild(styleUrlElement);

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
            string coordinatesText = $"{westLon.ToString().Replace(',', '.')},{northLat.ToString().Replace(',', '.')},0 " +
                                     $"{eastLon.ToString().Replace(',', '.')},{northLat.ToString().Replace(',', '.')},0 " +
                                     $"{eastLon.ToString().Replace(',', '.')},{southLat.ToString().Replace(',', '.')},0 " +
                                     $"{westLon.ToString().Replace(',', '.')},{southLat.ToString().Replace(',', '.')},0 " +
                                     $"{westLon.ToString().Replace(',', '.')},{northLat.ToString().Replace(',', '.')},0";
            coordinatesElement.InnerText = coordinatesText;

            // Сохраняем файл KML
            string filePath = fileName;
            xmlDoc.Save(filePath);

            Console.WriteLine($"KML файл сохранен по пути: {filePath}");
        }
    }

}

