using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReSource
{
    public enum LatitudeName
    {
        NorthPole,
        NorthCircle,
        NorthTropic,
        Equator,
        SouthTropic,
        SouthCircle,
        SouthPole
    }

    struct WindLatitude
    {
        public LatitudeName Name;
        public double WindDirection;    //radians from north
        public double WindStrength;     //fraction between 0 and 1
        public double latitude;

        public WindLatitude(LatitudeName name, double windDir, double windStrength, double lat)
        {
            this.Name = name;
            this.WindDirection = windDir;
            this.WindStrength = windStrength;
            this.latitude = lat;
        }
    }
}
