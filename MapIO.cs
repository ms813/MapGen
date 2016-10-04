using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using System.IO;

namespace ReSource
{
    class MapIO
    {
        public void Save(WorldMap map, string path, string name)
        {
            //string json = JsonConvert.SerializeObject(map);
            if(!File.Exists(path + name))
            {
                Directory.CreateDirectory(path + name);
            }

            List<string> tileJson = new List<string>();

            foreach(MapTile t in map.Tiles.Values)
            {
                try
                {
                    String s = JsonConvert.SerializeObject(t);
                    tileJson.Add(s);                    
                } catch(JsonSerializationException e)
                {
                    Console.WriteLine(e.InnerException);
                }                
            }
             
            //System.IO.File.WriteAllText(path, json);
        }        
    }
}