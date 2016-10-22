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
            Console.WriteLine();
            Console.WriteLine("[MapIO] Saving map...");            
            string json = JsonConvert.SerializeObject(mapData, Formatting.Indented);
            if (!File.Exists(path + name))
            {
                Directory.CreateDirectory(path + name);
            }

            File.WriteAllText(path + name + Path.DirectorySeparatorChar + name + ".worldmap", json);
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("[MapIO] {0} map successfully saved", name);
            Console.ResetColor();
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
                    Console.WriteLine("{0} was not found. ", name);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Creating a new world called {0}", name);
                   
                    mapData = LoadDefault();
                    mapData.MapName = name;
                    Console.ForegroundColor = ConsoleColor.Cyan;

                    Console.Write("World map height (default = {0}): ", mapData.MapSize.Y);
                    string input = Console.ReadLine();
                    mapData.MapSize.Y = string.IsNullOrEmpty(input) ? mapData.MapSize.Y : int.Parse(input);

                    int defaultX = (int)Math.Round(mapData.MapSize.Y * 1.5d);
                    Console.Write("World map width (default = {0}): ", defaultX);
                    input = Console.ReadLine();
                    mapData.MapSize.X = string.IsNullOrEmpty(input) ? defaultX : int.Parse(input);

                    mapData.BaseSeed = new Random().Next();

                    mapData.WorldCalendar.Unpack();                    
                }
            }

            Console.ResetColor();
            return mapData;
        }        
      
        private WorldMapSaveData LoadDefault()
        {
            JsonReader jr = new JsonReader();
            return jr.ReadJson<WorldMapSaveData>(defaultParamsPath);
        }
    }
}