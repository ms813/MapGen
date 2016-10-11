using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReSource
{
    class WorldMapBuilder
    {
        private WorldMapSaveData mapData;        

        public WorldMapBuilder(string name = "")
        {            
            MapIO io = new MapIO();            
            mapData = io.Load(name);  
            
            //if no name is specified, the default parameters are loaded by MapIO
            //generate the base seed for the new map here
            if(name == "")
            {                  
                mapData.BaseSeed = new Random().Next(int.MaxValue);
                Console.WriteLine(mapData.BaseSeed);
            }
        }
        
        public WorldMap Build()
        {
            WorldMap worldMap = new WorldMap(mapData);        
            worldMap.Init();

            return worldMap;
        }
    }
}
