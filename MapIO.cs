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
            if (!File.Exists(path + name))
            {
                Directory.CreateDirectory(path + name);
            }

            File.WriteAllText(path + name + Path.DirectorySeparatorChar + name + ".worldmap", json);
            Console.WriteLine("Map successfully saved!");
        }

        public WorldMapSaveData Load()
        {
            Console.WriteLine("Loading map...");           

            WorldMapSaveData mapData = default(WorldMapSaveData); 
            while(mapData == default(WorldMapSaveData))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[MapIO] Enter world name: ");
                string name = Console.ReadLine();
                try
                {
                    JsonReader jr = new JsonReader();
                    mapData = jr.ReadJson<WorldMapSaveData>(path + name + Path.DirectorySeparatorChar + name + ".worldmap");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[MapIO] {0} map data successfully loaded from file!", name);
                }
                catch (IOException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("{0} was not found. ", name);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Creating a new world called {0}", name);
                   
                    mapData = LoadDefault();
                    mapData.MapName = name;
                    mapData.BaseSeed = new Random().Next();                                 
                }
            }

            Console.ResetColor();
            return mapData;
        }
        /*
         * mapData = LoadDefault();

        //set a random seed for the new map
        mapData.BaseSeed = new Random().Next();
        Console.WriteLine("[MapIO] No map name specified, loading default parameters");
        */
        private WorldMapSaveData LoadDefault()
        {
            JsonReader jr = new JsonReader();
            return jr.ReadJson<WorldMapSaveData>(defaultParamsPath);
        }
    }
}