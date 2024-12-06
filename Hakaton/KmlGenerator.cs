using System.Xml;

namespace Hakaton
{
    public class KmlGenerator
    {
        public static void WriteToKML(string fileName, string imagePath, (double lat, double lon, double alt)[] corners)
        {
            XmlDocument kmlDocument = new XmlDocument();

            XmlDeclaration declaration = kmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            kmlDocument.AppendChild(declaration);

            XmlElement kmlElement = kmlDocument.CreateElement("kml", "http://www.opengis.net/kml/2.2");
            kmlDocument.AppendChild(kmlElement);

            XmlElement documentElement = kmlDocument.CreateElement("Document");
            kmlElement.AppendChild(documentElement);

            for (int i = 0; i < corners.Length; i++)
            {
                XmlElement placemark = kmlDocument.CreateElement("Placemark");
                documentElement.AppendChild(placemark);

                XmlElement name = kmlDocument.CreateElement("name");
                name.InnerText = $"Угол {i + 1}";
                placemark.AppendChild(name);

                XmlElement description = kmlDocument.CreateElement("description");
                description.InnerText = $"Угол снимка {i + 1}";
                placemark.AppendChild(description);

                XmlElement point = kmlDocument.CreateElement("Point");
                placemark.AppendChild(point);

                XmlElement coordinatesElement = kmlDocument.CreateElement("coordinates");
                string lon = corners[i].lon.ToString().Replace(',', '.');
                string lat = corners[i].lat.ToString().Replace(',', '.');
                coordinatesElement.InnerText = $"{lon},{lat},0";
                point.AppendChild(coordinatesElement);
            }

            XmlElement polygonPlacemark = kmlDocument.CreateElement("Placemark");
            documentElement.AppendChild(polygonPlacemark);

            XmlElement polygonName = kmlDocument.CreateElement("name");
            polygonName.InnerText = "Область снимка";
            polygonPlacemark.AppendChild(polygonName);

            XmlElement polygon = kmlDocument.CreateElement("Polygon");
            polygonPlacemark.AppendChild(polygon);

            XmlElement outerBoundaryIs = kmlDocument.CreateElement("outerBoundaryIs");
            polygon.AppendChild(outerBoundaryIs);

            XmlElement linearRing = kmlDocument.CreateElement("LinearRing");
            outerBoundaryIs.AppendChild(linearRing);

            XmlElement polygonCoordinates = kmlDocument.CreateElement("coordinates");
            string coords = "";
            foreach (var corner in corners)
            {
                coords += $"{corner.lon.ToString().Replace(',', '.')},{corner.lat.ToString().Replace(',', '.')},0\n";
            }
            coords += $" {corners[0].lon.ToString().Replace(',', '.')},{corners[0].lat.ToString().Replace(',', '.')},0"; // Замыкание полигона
            polygonCoordinates.InnerText = coords;
            linearRing.AppendChild(polygonCoordinates);
            // Добавление наложения изображения
            XmlElement groundOverlay = kmlDocument.CreateElement("GroundOverlay");
            documentElement.AppendChild(groundOverlay);

            XmlElement overlayName = kmlDocument.CreateElement("name");
            overlayName.InnerText = "Изображение наложения";
            groundOverlay.AppendChild(overlayName);

            XmlElement icon = kmlDocument.CreateElement("Icon");
            groundOverlay.AppendChild(icon);

            XmlElement href = kmlDocument.CreateElement("href");
            href.InnerText = imagePath; // Путь к локальному изображению
            icon.AppendChild(href);

            XmlElement latLonBox = kmlDocument.CreateElement("LatLonBox");
            groundOverlay.AppendChild(latLonBox);

            // Указываем границы наложения изображения
            XmlElement north = kmlDocument.CreateElement("north");
            north.InnerText = corners[0].lat.ToString().Replace(',', '.'); // Максимальная широта (север)
            latLonBox.AppendChild(north);

            XmlElement south = kmlDocument.CreateElement("south");
            south.InnerText = corners[2].lat.ToString().Replace(',', '.'); // Минимальная широта (юг)
            latLonBox.AppendChild(south);

            XmlElement east = kmlDocument.CreateElement("east");
            east.InnerText = corners[1].lon.ToString().Replace(',', '.'); // Максимальная долгота (восток)
            latLonBox.AppendChild(east);

            XmlElement west = kmlDocument.CreateElement("west");
            west.InnerText = corners[3].lon.ToString().Replace(',', '.'); // Минимальная долгота (запад)
            latLonBox.AppendChild(west);
            kmlDocument.Save(fileName);
        }
    }
}

