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

        public static List<ClimateZone> ElevationZones { get; private set; }
        public static List<ClimateZone> TemperatureZones { get; private set; }
        public static List<ClimateZone> HumidityZones { get; private set; }

        private static string elevationZonePath = @"..\..\resources\worldGen\ElevationZone.json";
        private static string temperatureZonePath = @"..\..\resources\worldGen\TemperatureZone.json";
        private static string humidityZonePath = @"..\..\resources\worldGen\HumidityZone.json";
        static ClimateZone()
        {
            ElevationZones = new JsonReader().ReadJsonArray<ClimateZone>(elevationZonePath);
            TemperatureZones = new JsonReader().ReadJsonArray<ClimateZone>(temperatureZonePath);
            HumidityZones = new JsonReader().ReadJsonArray<ClimateZone>(humidityZonePath);
        }

        public static ClimateZone GetTemperatureZone(String name)
        {
            foreach(ClimateZone z in TemperatureZones)
            {
                if(z.Name == name) return z;                    
            }
            throw new Exception("Temperature zone with name : " + name + " does not exist!");
        }

        public static ClimateZone GetElevationZone(String name)
        {
            foreach (ClimateZone z in ElevationZones)
            {
                if (z.Name == name) return z;
            }
            throw new Exception("Elevation zone with name : " + name + " does not exist!");
        }


        public static ClimateZone GetHumidityZone(String name)
        {
            foreach (ClimateZone z in HumidityZones)
            {
                if (z.Name == name) return z;
            }
            throw new Exception("Humidity zone with name : " + name + " does not exist!");
        }


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
