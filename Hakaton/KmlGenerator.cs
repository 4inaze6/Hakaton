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

            kmlDocument.Save(fileName);
        }
    }
}

