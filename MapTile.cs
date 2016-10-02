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
        public double Perlin { get; set; }
        public double Voronoi { get; set; }
        public double Gaussian { get; set; }

        //rivers
        public Vector2i DownslopeDir { get; set; }
        public int RiverVolume { get; set; }
        public bool RiverSource = false;
        public bool DownhillToSea = false;

        //water & moisture
        public double Moisture { get; set; }
        public WaterType Water { get; set; }
        public bool Coast = false;
        public double RainShadow { get; set; }
        public double Rainfall { get; set; }              

        //temperature
        public double Temperature { get; set; }

        //wind        
        public double WindDirection { get; set; }
        public double PrevailingWindDir { get; set; }
        public double WindNoise { get; set; }
        public double WindStrength { get; set; }
        public double BaseWindStrength { get; set; }
        public double ContinentWindStrength { get; set; }
        public int DistanceToCoast = Int32.MaxValue;

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
            if (Coast)
            {
                DisplayColour = Color.Black;
            }
            else
            {
                DisplayColour = Biome.Color;
            }
        }

        public void SetWindColor()
        {
            //Yellow    - south
            //Red       - west            
            //Green     - north
            //Blue      - east
            DisplayColour = WindHelper.GetWindColor(this);
            
            //brighter colors = higher wind strength

            DisplayColour.A = (byte)(255 * WindStrength);
            
        }            
         
        public void SetLandmassColor()
        {
            if (LandmassId == -1)
            {
                DisplayColour = Color.Black;
            }
            else
            {
                DisplayColour = ColorLookup.Color[LandmassId % ColorLookup.Color.Count];
            }
        }

        public void SetTemperatureColor()
        {
            Color c = new Color();
            c.A = 255;
            double interpolate = MathHelper.Scale(0, 1, 0, 255, Temperature);
            c.B = (byte)(255 - interpolate);
            c.R = (byte)(interpolate);

            DisplayColour = c;
        }

        public void SetRainShadowColor()
        {
            if(Water == WaterType.Ocean)
            {
                double interpolate = MathHelper.Scale(0, ParentMap.SeaLevel, 0, 255, Elevation);
                byte b = (byte)Math.Floor(interpolate - 255d);
                DisplayColour = new Color(0, 0, b, 255);
            }
            else
            {
                byte i = (byte)MathHelper.Scale(0, 1, 0, 255, RainShadow);
                DisplayColour = new Color(i, i, i, 255);
            }         
        }

        public void SetRainfallColor()
        {
            Color c = new Color();
            c.A = 255;
            double interpolate = MathHelper.Scale(0, 1, 0, 255, Rainfall);
            c.R = (byte)(255 - interpolate);
            c.B = (byte)(interpolate);

            DisplayColour = c;
        }

        public void SetElevationZoneColor()
        {            
            if(Coast)
            {
                DisplayColour = Color.Black;
            }
            else
            {
                DisplayColour = ElevationZone.Color;       
            }
        }

        public void SetTemperatureZoneColor()
        {
            if (Coast)
            {
                DisplayColour = Color.Black;
            }
            else
            {
                DisplayColour = TemperatureZone.Color;
            }
        }

        public void SetHumidityZoneColor()
        {
            if (Coast)
            {
                DisplayColour = Color.Black;
            }
            else
            {
                DisplayColour = HumidityZone.Color;
            }
        }

        public void Update(float dt)
        {

        }             
    }
}
