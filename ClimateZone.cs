using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ReSource
{
    class ClimateZone
    {
        [JsonProperty("name")]
        public String Name;

        [JsonProperty("type")]
        public String Type;

        [JsonProperty("zone")]
        public int Zone;

        [JsonProperty("color")]
        public SFML.Graphics.Color Color;

        public static List<ClimateZone> ElevationZones = new JsonReader(@"..\..\..\resources\worldGen\ElevationZone.json").ReadJson<ClimateZone>();
        public static List<ClimateZone> TemperatureZones = new JsonReader(@"..\..\..\resources\worldGen\TemperatureZone.json").ReadJson<ClimateZone>();
        public static List<ClimateZone> HumidityZones = new JsonReader(@"..\..\..\resources\worldGen\HumidityZone.json").ReadJson<ClimateZone>();

        public static ClimateZone GetElevationZone(int i)
        {
            foreach(ClimateZone z in ElevationZones)
            {
                if(z.Zone == i)
                {
                    return z;
                }
            }

            throw new IndexOutOfRangeException("Elevation zone " + i + " does not exist!");
        }
    }          
}
