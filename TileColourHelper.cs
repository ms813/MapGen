using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;

namespace ReSource
{
    class TileColourHelper
    {
        public void SetElevationColor(MapTile t)
        {
            Color color = new Color();
            color.A = 255;

            double interpolate = 0;
            switch (t.Water)
            {
                case WaterType.Land:
                    interpolate = MathHelper.Scale(0, t.ParentMap.MaxElevation, 0, 255, t.Elevation);
                    color.R = (byte)Math.Floor(interpolate);
                    color.G = (byte)Math.Floor(255d - interpolate);
                    break;
                case WaterType.Ocean:
                    interpolate = MathHelper.Scale(0, t.ParentMap.SeaLevel, 0, 255, t.Elevation);
                    color.B = (byte)Math.Floor(interpolate - 255d);
                    break;
                case WaterType.Lake:
                    interpolate = MathHelper.Scale(0, t.ParentMap.SeaLevel, 0, 255, t.Elevation);
                    color.B = (byte)Math.Floor(interpolate - 255d);
                    color.G = (byte)Math.Floor(interpolate - 255d);
                    break;
            }

            if (t.Coast)
            {
                color = Color.Yellow;
            }
            if (t.RiverVolume > 0)
            {
                color = Color.Cyan;
                color.G /= 2;
                color.B /= 2;

            }
            if (t.RiverSource)
            {
                color = Color.Cyan;
            }

            t.DisplayColour = color;
        }

        public void SetBiomeColor(MapTile t)
        {
            if (t.Coast)
            {
                t.DisplayColour = Color.Black;
            }
            else
            {
                t.DisplayColour = t.Biome.Color;
            }
        }

        public void SetWindColor(MapTile t)
        {
            //Yellow    - south
            //Red       - west            
            //Green     - north
            //Blue      - east
            t.DisplayColour = WindHelper.GetWindColor(t);

            //brighter colors = higher wind strength

            t.DisplayColour.A = (byte)(255 * t.WindStrength);
        }

        public void SetLandmassColor(MapTile t)
        {
            if (t.LandmassId == -1)
            {
                t.DisplayColour = Color.Black;
            }
            else
            {
                t.DisplayColour = ColorLookup.Color[t.LandmassId % ColorLookup.Color.Count];
            }
        }

        public void SetTemperatureColor(MapTile t)
        {
            Color c = new Color();
            c.A = 255;
            double interpolate = MathHelper.Scale(0, 1, 0, 255, t.Temperature);
            c.B = (byte)(255 - interpolate);
            c.R = (byte)(interpolate);

            t.DisplayColour = c;
        }

        public void SetRainShadowColor(MapTile t)
        {
            if (t.Water == WaterType.Ocean)
            {
                double interpolate = MathHelper.Scale(0, t.ParentMap.SeaLevel, 0, 255, t.Elevation);
                byte b = (byte)Math.Floor(interpolate - 255d);
                t.DisplayColour = new Color(0, 0, b, 255);
            }
            else
            {
                byte i = (byte)MathHelper.Scale(0, 1, 0, 255, t.RainShadow);
                t.DisplayColour = new Color(i, i, i, 255);
            }
        }

        public void SetRainfallColor(MapTile t)
        {
            Color c = new Color();
            c.A = 255;
            double interpolate = MathHelper.Scale(0, 1, 0, 255, t.Rainfall);
            c.R = (byte)(255 - interpolate);
            c.B = (byte)(interpolate);

            t.DisplayColour = c;
        }

        public void SetElevationZoneColor(MapTile t)
        {
            if (t.Coast)
            {
                t.DisplayColour = Color.Black;
            }
            else
            {
                t.DisplayColour = t.ElevationZone.Color;
            }
        }

        public void SetTemperatureZoneColor(MapTile t)
        {
            if (t.Coast)
            {
                t.DisplayColour = Color.Black;
            }
            else
            {
                t.DisplayColour = t.TemperatureZone.Color;
            }
        }

        public void SetHumidityZoneColor(MapTile t)
        {
            if (t.Coast)
            {
                t.DisplayColour = Color.Black;
            }
            else
            {
                t.DisplayColour = t.HumidityZone.Color;
            }
        }
    }
}
