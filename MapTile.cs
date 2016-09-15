using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;
using SFML.System;

namespace ReSource
{
    class MapTile
    {
        public Vector2i GlobalIndex { get; private set; }   //id in the whole world
        public int ChunkIndex { get; private set; }    //id within its own chunk (0–255)
        private WorldMap ParentMap;
        public bool border { get; private set; }         

        public List<MapTile> OrthogonalNeighbours = new List<MapTile>();
        public List<MapTile> DiagonalNeighbours = new List<MapTile>();
        
        /*
         * Rendering parameters
         * 
         *  A MapTileData object is used to pass data loaded from file to the MapTile object
         *  and unpacked into the variables below
         */        

        private int tileSize;
        public Color ElevationColor { get; set; }

        /*
         * Biome parameters
         */
        public double Elevation { get; set; }
        public double Perlin { get; set; }
        public double Voronoi { get; set; }
        public double Gaussian { get; set; }

        public Vector2i DownslopeDir { get; set; }

        public double Moisture { get;  set; }        
        public Color MoistureColor { get; private set; }

        public WaterType Water { get; set; }        
        public int RiverVolume { get; set; }
        public bool RiverSource = false;
        public bool Coast = false;
        public bool DownhillToSea = false;

        public Biome Biome { get; set; }

        public MapTile(WorldMap parentMap, Vector2i globalIndex, int tileSize)
        {
            this.ParentMap = parentMap;
            this.tileSize = tileSize;
            this.GlobalIndex = globalIndex;          
            
            Water = WaterType.Unassigned;

            if(globalIndex.X <= 0 
                || globalIndex.Y <= 0 
                || globalIndex.X >= ParentMap.MapSize.X - 1
                || globalIndex.Y >= ParentMap.MapSize.Y - 1)
            {
                border = true;
            }            
        }    
       
        public void UpdateElevationColor()
        {
            Color color = new Color();
            color.A = 255;

            double interpolate = 0;
            switch(Water)
            {                     
                case WaterType.Land:
                    interpolate = MathHelper.Scale(0, ParentMap.MaxElevation, 0, 255, Elevation);
                    color.R = (byte)Math.Floor(interpolate);
                    color.G = (byte)Math.Floor(255d - interpolate);
                    break;
                case WaterType.Ocean:
                    interpolate = MathHelper.Scale(0, ParentMap.SeaLevel, 0, 255, Elevation);
                    color.B = (byte)Math.Floor(interpolate-255d);
                    break;
                case WaterType.Lake:
                    interpolate = MathHelper.Scale(0, ParentMap.SeaLevel, 0, 255, Elevation);
                    color.B = (byte)Math.Floor(interpolate - 255d);
                    color.G = (byte)Math.Floor(interpolate - 255d);
                    break;
            }       

            if (Coast)
            {
                color = Color.Yellow;
            }
            if (RiverVolume > 0)
            {                
                color = Color.Cyan;
                color.G /= 2;
                color.B /= 2;
                
            }
            if(RiverSource)
            {
                color = Color.Cyan;
            }            
    
            ElevationColor = color;
        }  

        public void UpdateMoistureColor()
        {
            if (Water != WaterType.Ocean)
            {
                Color c = new Color();
                c.A = 255;
                double interpolate = MathHelper.Scale(0, 1, 0, 255, Moisture);
                c.R = (byte)(255 - interpolate);
                c.G = (byte)(interpolate);

                MoistureColor = c;
            }            
        }
       
        public void Update(float dt)
        {

        }
    }
}
