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

    struct Latitude
    {
        public LatitudeName Name;
        public double WindDirection;   //radians from north
        public double latitude;

        public Latitude(LatitudeName name, double windDir, double lat)
        {
            this.Name = name;
            this.WindDirection = windDir;
            this.latitude = lat;
        }
    }
}
