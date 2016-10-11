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

        [JsonIgnore]
        public WorldMap ParentMap { get; private set; }
        [JsonIgnore]
        public List<MapTile> OrthogonalNeighbours = new List<MapTile>();
        [JsonIgnore]
        public List<MapTile> DiagonalNeighbours = new List<MapTile>();    
        [JsonIgnore]    
        public int TileSize { get; private set; }

        public int LandmassId = -1;
                
        [JsonIgnore]
        public ClimateZone HumidityZone;
        [JsonIgnore]
        public ClimateZone TemperatureZone;
        [JsonIgnore]
        public ClimateZone ElevationZone;

        //Elevation        
        public double Elevation { get; set; }
        //elevation noise coefficients        
        [JsonIgnore]
        public double ElevationPerlin { get; set; }
        [JsonIgnore]
        public double ElevationVoronoi { get; set; }
        [JsonIgnore]
        public double ElevationGaussian { get; set; }

        //rivers
        [JsonIgnore]
        public Vector2i DownslopeDir { get; set; }

        public int RiverVolume { get; set; }
        
        public bool RiverSource = false;

        [JsonIgnore]
        public bool DownhillToSea = false;

        //water & moisture        
        public WaterType Water { get; set; }

        [JsonIgnore]
        public bool Coast = false;
        public double RainShadow { get; set; }
        public double Rainfall { get; set; }              

        //temperature
        public double Temperature { get; set; }

        //wind        
        public double WindDirection { get; set; }
        public double WindStrength { get; set; }
        public int DistanceToCoast { get; set; }
        [JsonIgnore]
        public double PrevailingWindDir { get; set; }
        [JsonIgnore]
        public double WindNoise { get; set; }        
        [JsonIgnore]
        public double BaseWindStrength { get; set; }
        [JsonIgnore]
        public double ContinentWindStrength { get; set; }        

        //Biome
        [JsonIgnore]
        public Biome Biome { get; set; }

        //colors for the different map views
        [JsonIgnore]
        public Color DisplayColour;
        
        public MapTile(WorldMap parentMap, Vector2i globalIndex, int tileSize)
        {
            this.ParentMap = parentMap;
            this.TileSize = tileSize;
            this.GlobalIndex = globalIndex;          
            
            Water = WaterType.Unassigned;
            DistanceToCoast = parentMap.windThresholdDist;   
        }        

        public void Update(float dt)
        {

        }             
    }
}
