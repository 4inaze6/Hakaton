using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Hakaton
{
    public class Interpolate
    {
        public static Beacon InterpolateBeacon(List<Beacon> beacons, DateTime targetTime)
        {
            beacons = beacons.OrderBy(b => b.Time).ToList();

            Beacon startBeacon = null;
            Beacon endBeacon = null;

            for (int i = 0; i < beacons.Count - 1; i++)
            {
                if (targetTime >= beacons[i].Time && targetTime <= beacons[i + 1].Time)
                {
                    Console.WriteLine("Yes");
                    startBeacon = beacons[i];
                    endBeacon = beacons[i + 1];
                    break;
                }
            }

            if (startBeacon == null || endBeacon == null)
            {
                if (targetTime < beacons[0].Time)
                {
                    startBeacon = beacons[0];
                    endBeacon = beacons[1];
                }
                if (targetTime > beacons[beacons.Count - 1].Time)
                {
                    startBeacon = beacons[beacons.Count - 2];
                    endBeacon = beacons[beacons.Count - 1];
                }
            }

            double totalTime = (endBeacon.Time - startBeacon.Time).TotalSeconds;
            double timeDiff = (targetTime - startBeacon.Time).TotalSeconds;

            double t = timeDiff / totalTime;

            return InterpolateBeacon(startBeacon, endBeacon, t);
        }

        static Beacon InterpolateBeacon(Beacon startBeacon, Beacon endBeacon, double t)
        {
            double interpolatedLat = Interpolating(startBeacon.Latitude, endBeacon.Latitude, t);
            double interpolatedLon = Interpolating(startBeacon.Longitude, endBeacon.Longitude, t);
            double interpolatedAlt = Interpolating(startBeacon.Altitude, endBeacon.Altitude, t);

            Quaternion quatStart = new Quaternion((float)startBeacon.EciQuatX, (float)startBeacon.EciQuatY, (float)startBeacon.EciQuatZ, (float)startBeacon.EciQuatW);
            Quaternion quatEnd = new Quaternion((float)endBeacon.EciQuatX, (float)endBeacon.EciQuatY, (float)endBeacon.EciQuatZ, (float)endBeacon.EciQuatW);
            Quaternion interpolatedQuat = Quaternion.Slerp(quatStart, quatEnd, (float)t);

            return new Beacon
            {
                Latitude = interpolatedLat,
                Longitude = interpolatedLon,
                Altitude = interpolatedAlt,
                EciQuatW = interpolatedQuat.W,
                EciQuatX = interpolatedQuat.X,
                EciQuatY = interpolatedQuat.Y,
                EciQuatZ = interpolatedQuat.Z
            };
        }

        static double Interpolating(double start, double end, double t)
        {
            return (1 - t) * start + t * end;
        }
    }
}

