using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;
using SFML.System;

using Newtonsoft.Json;

namespace ReSource
{
    class MapTile
    {
        /*
         * For any map
         */
        //General              
        public Vector2i GlobalIndex { get; private set; }   //id in the whole world

        public WorldMap ParentMap { get; private set; }        
        public List<MapTile> OrthogonalNeighbours = new List<MapTile>();        
        public List<MapTile> DiagonalNeighbours = new List<MapTile>();            
        public int TileSize { get; private set; }

        public int LandmassId = -1;                
      
        public ClimateZone HumidityZone;
        public ClimateZone TemperatureZone;
        public ClimateZone ElevationZone;

        //Elevation        
        public double Elevation { get; set; }
        //elevation noise coefficients        
        public double ElevationPerlin { get; set; }
        public double ElevationVoronoi { get; set; }
        public double ElevationGaussian { get; set; }

        //rivers
        public Vector2i DownslopeDir { get; set; }

        public int RiverVolume { get; set; }
        
        public bool RiverSource = false;

        public bool DownhillToSea = false;

        //water & moisture        
        public WaterType Water { get; set; }

        public bool Coast = false;
        public double RainShadow { get; set; }
        public double Rainfall { get; set; }              

        //temperature
        public double Temperature { get; set; }

        //wind        
        public double WindDirection { get; set; }
        public double WindStrength { get; set; }
        public int DistanceToCoast { get; set; }
        public double PrevailingWindDir { get; set; }
        public double WindNoise { get; set; }        
        public double BaseWindStrength { get; set; }
        public double ContinentWindStrength { get; set; }        

        //Biome
        public Biome Biome { get; set; }

        //colors for the different map views
        public Color DisplayColour;
        
        public MapTile(WorldMap parentMap, Vector2i globalIndex, int tileSize)
        {
            this.ParentMap = parentMap;
            this.TileSize = tileSize;
            this.GlobalIndex = globalIndex;          
            
            Water = WaterType.Unassigned;

            DistanceToCoast = parentMap.WindThresholdDist;   
        }        

        public void Update(float dt)
        {

        }             
    }
}
