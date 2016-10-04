using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace ReSource
{
    class MapSaver
    {
        public void Save(WorldMap map, string path)
        {
            string json = JsonConvert.SerializeObject(map);
            Console.WriteLine(json);
           
            //System.IO.File.WriteAllText(path, json);
        }        
    }
}