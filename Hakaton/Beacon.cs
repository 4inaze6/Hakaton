namespace Hakaton
{
    public class Beacon
    {
        public DateTime Time { get; set; } // Время в микросекундах
        public double EciQuatW { get; set; } // Кватернион ECI.w
        public double EciQuatX { get; set; } // Кватернион ECI.x
        public double EciQuatY { get; set; } // Кватернион ECI.y
        public double EciQuatZ { get; set; } // Кватернион ECI.z
        public double OrbQuatW { get; set; } // Кватернион Orb.w 
        public double OrbQuatX { get; set; } // Кватернион Orb.x
        public double OrbQuatY { get; set; } // Кватернион Orb.y
        public double OrbQuatZ { get; set; } // Кватернион Orb.z
        public double Latitude { get; set; } // Широта в градусах
        public double Longitude { get; set; } // Долгота в градусах
        public double Altitude { get; set; } // Высота в метрах
    }
}
