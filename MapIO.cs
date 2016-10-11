using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using System.IO;

namespace ReSource
{
    class MapIO
    {
        private static string path = @"..\..\saves\";
        private static string defaultParamsPath = @"..\..\resources\worldGen\worldMapDefaultParams.worldmap";
        public void Save(WorldMapSaveData mapData, string name)
        {
            Console.WriteLine("Saving map...");
            string json = JsonConvert.SerializeObject(mapData, Formatting.Indented);
            if(!File.Exists(path + name))
            {
                Directory.CreateDirectory(path + name);
            }            
             
            File.WriteAllText(path + name + Path.DirectorySeparatorChar + name + ".worldmap", json);
            Console.WriteLine("Map successfully saved!");
        }       
        
        public WorldMapSaveData Load(string name = "")
        {
            Console.WriteLine("Loading map...");

            JsonReader jr = new JsonReader();
            WorldMapSaveData mapData;
            if(name == "")
            {
                //if no map name is specified, the default parameters are loaded
                //in preparation for creating a new WorldMap
                mapData = jr.ReadJson<WorldMapSaveData>(defaultParamsPath);
                Console.WriteLine("[MapIO] No map name specified, loading default parameters");
            }
            else
            {
                mapData = jr.ReadJson<WorldMapSaveData>(path + name + Path.DirectorySeparatorChar + name + ".worldmap");
                Console.WriteLine("[MapIO] {0} map data successfully loaded from file!", name);
            }                     

            return mapData;
        }
    }
}