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

            // Создаем GroundOverlay
            XmlElement groundOverlayElement = xmlDoc.CreateElement("GroundOverlay");
            documentElement.AppendChild(groundOverlayElement);

            // Добавляем имя
            XmlElement nameElement = xmlDoc.CreateElement("name");
            nameElement.InnerText = "Изображение на карте";
            groundOverlayElement.AppendChild(nameElement);

            // Добавляем описание
            XmlElement descriptionElement = xmlDoc.CreateElement("description");
            descriptionElement.InnerText = "Наложение изображения на карту";
            groundOverlayElement.AppendChild(descriptionElement);

            // Добавляем ссылку на изображение
            XmlElement iconElement = xmlDoc.CreateElement("Icon");
            groundOverlayElement.AppendChild(iconElement);

            XmlElement hrefElement = xmlDoc.CreateElement("href");
            hrefElement.InnerText = imagePath;
            iconElement.AppendChild(hrefElement);

            // Указываем координаты границ
            XmlElement latLonBoxElement = xmlDoc.CreateElement("LatLonBox");
            groundOverlayElement.AppendChild(latLonBoxElement);

            XmlElement northElement = xmlDoc.CreateElement("north");
            northElement.InnerText = northLat.ToString().Replace(',', '.'); // Северная широта
            latLonBoxElement.AppendChild(northElement);

            XmlElement southElement = xmlDoc.CreateElement("south");
            southElement.InnerText = southLat.ToString().Replace(',', '.'); // Южная широта
            latLonBoxElement.AppendChild(southElement);

            XmlElement eastElement = xmlDoc.CreateElement("east");
            eastElement.InnerText = eastLon.ToString().Replace(',', '.'); // Восточная долгота
            latLonBoxElement.AppendChild(eastElement);

            XmlElement westElement = xmlDoc.CreateElement("west");
            westElement.InnerText = westLon.ToString().Replace(',', '.'); // Западная долгота
            latLonBoxElement.AppendChild(westElement);

            string filePath = fileName;
            xmlDoc.Save(filePath);
        }
    }

}

