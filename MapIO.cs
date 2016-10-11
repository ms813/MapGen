using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using System.IO;

namespace ReSource
{
    class MapIO
    {
        private static string path = @"..\..\saves\";
        public void Save(WorldMap map, string name)
        {
            Console.WriteLine("Saving map...");
            string json = JsonConvert.SerializeObject(map, Formatting.Indented);
            if(!File.Exists(path + name))
            {
                Directory.CreateDirectory(path + name);
            }            
             
           File.WriteAllText(path + name + Path.DirectorySeparatorChar + name + ".worldmap", json);
            Console.WriteLine("Map successfully saved!");
        }        
    }
}