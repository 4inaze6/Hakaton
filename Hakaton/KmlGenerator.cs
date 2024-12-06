using System.Xml;

namespace Hakaton
{
    public class KmlGenerator
    {
        public static void WriteToKML(string fileName, string imagePath, double northLat, double southLat, double eastLon, double westLon)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement kmlElement = xmlDoc.CreateElement("kml");
            kmlElement.SetAttribute("xmlns", "http://www.opengis.net/kml/2.2");
            xmlDoc.AppendChild(kmlElement);

            XmlElement documentElement = xmlDoc.CreateElement("Document");
            kmlElement.AppendChild(documentElement);

            XmlElement styleElement = xmlDoc.CreateElement("Style");
            styleElement.SetAttribute("id", "polygonStyle");
            documentElement.AppendChild(styleElement);

            XmlElement iconStyleElement = xmlDoc.CreateElement("IconStyle");
            styleElement.AppendChild(iconStyleElement);

            XmlElement iconElement = xmlDoc.CreateElement("Icon");
            iconStyleElement.AppendChild(iconElement);
            XmlElement hrefElement = xmlDoc.CreateElement("href");

            hrefElement.InnerText = $"{Path.GetDirectoryName(imagePath)}\\{Path.GetFileNameWithoutExtension(imagePath)}.jpg";  
            iconElement.AppendChild(hrefElement);

            XmlElement placemarkElement = xmlDoc.CreateElement("Placemark");
            documentElement.AppendChild(placemarkElement);

            XmlElement styleUrlElement = xmlDoc.CreateElement("styleUrl");
            styleUrlElement.InnerText = "#polygonStyle";
            placemarkElement.AppendChild(styleUrlElement);

            XmlElement descriptionElement = xmlDoc.CreateElement("description");
            descriptionElement.InnerText = "Скорректированная область видимости";
            placemarkElement.AppendChild(descriptionElement);

            XmlElement polygonElement = xmlDoc.CreateElement("Polygon");
            placemarkElement.AppendChild(polygonElement);

            XmlElement outerBoundaryIsElement = xmlDoc.CreateElement("outerBoundaryIs");
            polygonElement.AppendChild(outerBoundaryIsElement);

            XmlElement linearRingElement = xmlDoc.CreateElement("LinearRing");
            outerBoundaryIsElement.AppendChild(linearRingElement);

            // Создаем элемент coordinates
            XmlElement coordinatesElement = xmlDoc.CreateElement("coordinates");
            linearRingElement.AppendChild(coordinatesElement);

            string coordinatesText = $"{westLon.ToString().Replace(',', '.')},{northLat.ToString().Replace(',', '.')},0 " +
                                     $"{eastLon.ToString().Replace(',', '.')},{northLat.ToString().Replace(',', '.')},0 " +
                                     $"{eastLon.ToString().Replace(',', '.')},{southLat.ToString().Replace(',', '.')},0 " +
                                     $"{westLon.ToString().Replace(',', '.')},{southLat.ToString().Replace(',', '.')},0 " +
                                     $"{westLon.ToString().Replace(',', '.')},{northLat.ToString().Replace(',', '.')},0";
            coordinatesElement.InnerText = coordinatesText;
            xmlDoc.Save(fileName);
            Console.WriteLine($"KML файл сохранен по пути: {fileName}");
        }
    }
}

