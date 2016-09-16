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
        /*
         * For any map
         */
        //General
        public Vector2i GlobalIndex { get; private set; }   //id in the whole world
        private WorldMap ParentMap;
        public List<MapTile> OrthogonalNeighbours = new List<MapTile>();
        public List<MapTile> DiagonalNeighbours = new List<MapTile>();
        private int tileSize;

        //rivers
        public Vector2i DownslopeDir { get; set; }
        public int RiverVolume { get; set; }
        public bool RiverSource = false;
        public bool DownhillToSea = false;

        //water & moisture
        public double Moisture { get; set; }
        public WaterType Water { get; set; }
        public bool Coast = false;

        //Biome
        public Biome Biome { get; set; }

        //colors for the map view
        public Color DisplayColour;

        /*
         * For 'noise' maps
         */
        public double Elevation { get; set; }
        public double Perlin { get; set; }
        public double Voronoi { get; set; }
        public double Gaussian { get; set; }

        public MapTile(WorldMap parentMap, Vector2i globalIndex, int tileSize)
        {
            this.ParentMap = parentMap;
            this.tileSize = tileSize;
            this.GlobalIndex = globalIndex;          
            
            Water = WaterType.Unassigned;                       
        }    
       
        public void SetElevationColor()
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
    
            DisplayColour = color;
        }  

        public void SetMoistureColor()
        {
            if (Water != WaterType.Ocean)
            {
                Color c = new Color();
                c.A = 255;
                double interpolate = MathHelper.Scale(0, 1, 0, 255, Moisture);
                c.R = (byte)(255 - interpolate);
                c.G = (byte)(interpolate);

                DisplayColour = c;
            }            
        }

        public void SetBiomeColor()
        {
            DisplayColour = Biome.Color;
        }
       
        public void Update(float dt)
        {

        }
    }
}
