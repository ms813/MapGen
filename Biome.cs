using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;
using Newtonsoft.Json;

namespace ReSource
{
    struct LandBiomeLookup
    {
        [JsonProperty("temperatureZones")]
        internal List<String> temperatureZones;

        [JsonProperty("humidityZones")]
        internal List<String> humidityZones;

        [JsonProperty("biomes")]
        internal String[,] biomes;           
    }

    class Biome
    {
        public static Dictionary<String, Biome> Biomes { get; private set; }      
        private static LandBiomeLookup BiomeLookup;

        static Biome()
        {
            Biomes = new Dictionary<string, Biome>();
            BiomeLookup = new JsonReader(@"..\..\..\resources\worldGen\LandBiomeLookup.json").ReadJson<LandBiomeLookup>();
            List<Biome> temp = new JsonReader(@"..\..\..\resources\worldGen\biomes.json").ReadJsonArray<Biome>();            
            
            
            foreach (Biome b in temp)
            {
                Biomes.Add(b.Name, b);
            }           
        }

        private Biome() { }

        public int Count = 0;

        [JsonProperty("color")]
        public Color Color { get; private set; }

        [JsonProperty("name")]
        public String Name { get; private set; }
     
        public static Biome GetBiome(MapTile t)
        {
            if(t.Water == WaterType.Ocean)
            {
                //if below the freezing threshold set ocean to icecap
                if(t.TemperatureZone.Zone == 0)
                {
                    return Biomes["iceCap"];
                }
                else
                {
                    //otherwise return the ocean biome part
                    return Biomes[t.ElevationZone.Name];
                }                             
            }
            else
            {
                int i = BiomeLookup.humidityZones.FindIndex(x => x == t.HumidityZone.Name);
                int j = BiomeLookup.temperatureZones.FindIndex(x => x == t.TemperatureZone.Name);

                return Biomes[BiomeLookup.biomes[i, j]];
            }                       
        }      
  
    }   
}
