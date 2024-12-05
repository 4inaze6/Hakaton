using System.Xml;
using System.Xml.Linq;

namespace Hakaton
{
    public class KmlGenerator
    {
        public static void WriteToKML(Beacon beacon, string filePath)
        {
            XmlDocument kmlDocument = new XmlDocument();

            XmlDeclaration declaration = kmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            kmlDocument.AppendChild(declaration);

            XmlElement kmlElement = kmlDocument.CreateElement("kml", "http://www.opengis.net/kml/2.2");
            kmlDocument.AppendChild(kmlElement);

            XmlElement documentElement = kmlDocument.CreateElement("Document");
            kmlElement.AppendChild(documentElement);

            XmlElement placemarkElement = kmlDocument.CreateElement("Placemark");
            documentElement.AppendChild(placemarkElement);

            XmlElement nameElement = kmlDocument.CreateElement("name");
            nameElement.InnerText = "Example Placemark";
            placemarkElement.AppendChild(nameElement);

            XmlElement pointElement = kmlDocument.CreateElement("Point");
            placemarkElement.AppendChild(pointElement);

            XmlElement coordinatesElement = kmlDocument.CreateElement("coordinates");
            coordinatesElement.InnerText = "-122.0838,37.4220,0";  
            pointElement.AppendChild(coordinatesElement);

            kmlDocument.Save(filePath);
        }
    }
}
