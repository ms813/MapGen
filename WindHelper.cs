using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;

namespace ReSource
{
    class WindHelper
    {
        //controls how much the wind deviates from its circumglobal paths
        //0.0 means wind flows in perfect pattern
        private static readonly double WindConst = 0.2d;

        private static readonly WindLatitude[] WorldLatitudes = new WindLatitude[]{
            new WindLatitude(LatitudeName.NorthPole,    Math.PI,        0.8d,   0d),            
            new WindLatitude(LatitudeName.NorthCircle,  0d,             1.0d,   0.1d),
            new WindLatitude(LatitudeName.NorthTropic,  0.5d * Math.PI, 0.6d,   0.35d),
            new WindLatitude(LatitudeName.Equator,      1.5d * Math.PI, 0.4d,  0.5d),
            new WindLatitude(LatitudeName.SouthTropic,  0.5d * Math.PI, 0.6d,   0.65d),
            new WindLatitude(LatitudeName.SouthCircle,  Math.PI,        1.0d,   0.9d),
            new WindLatitude(LatitudeName.SouthPole,    0d,             0.8d,   1d)
        };

        private static readonly WindColorMap[] WindColors = new WindColorMap[]
        {
            new WindColorMap(Math.PI * -0.5d, Color.Red),    //west   
            new WindColorMap(0, Color.Green),               //north
            new WindColorMap(Math.PI * 0.5d, Color.Blue),   //east    
            new WindColorMap(Math.PI, Color.Yellow),        //south
            new WindColorMap(Math.PI * 1.5d, Color.Red),    //west            
            new WindColorMap(2d * Math.PI, Color.Green),    //north
            new WindColorMap(2.5d * Math.PI, Color.Blue)    //east
        };        
        
        //get the base wind direction on the passed tile
        public static double GetPrevailingWindDirection(MapTile t)
        {
            //get the tiles latitude as a fraction of the map height
            double tileLatitude = (double)t.GlobalIndex.Y/t.ParentMap.MapSize.Y;

            //get the defined latitudes that the tile lies between
            WindLatitude upper = GetUpperLatitude(tileLatitude);
            WindLatitude lower = GetLowerLatitude(tileLatitude);

            //convert the tile latitude to a fraction of the distance between the upper and lower
            tileLatitude = MathHelper.Scale(upper.latitude, lower.latitude, 0, 1, tileLatitude);

            //lerp between the wind directions of the upper and lower latitude
            //to find the wind direction on the tile
            return MathHelper.Lerp(upper.WindDirection, lower.WindDirection, tileLatitude);
        }

        //return a value between 0 and 1 to represent the base wind strength on the 
        //passed tile
        public static double GetBaseWindStrength(MapTile t)
        {
            //get the tiles latitude as a fraction of the map height
            double tileLatitude = (double)t.GlobalIndex.Y/t.ParentMap.MapSize.Y;

            //get the defined latitudes that the tile lies between
            WindLatitude upper = GetUpperLatitude(tileLatitude);
            WindLatitude lower = GetLowerLatitude(tileLatitude);

            //convert the tile latitude to a fraction of the distance between the upper and lower
            tileLatitude  = MathHelper.Scale(upper.latitude, lower.latitude, 0, 1, tileLatitude);

            //lerp between the wind directions of the upper and lower latitude
            //to find the wind strength on the tile
            return MathHelper.Lerp(upper.WindStrength, lower.WindStrength, tileLatitude);
        }
               
        public static double GetWindDirection(MapTile t)
        {
            return t.PrevailingWindDir + t.WindNoise * WindConst;
        }

        private static WindLatitude GetUpperLatitude(double tileLatitude)
        {
            for(int i = 1; i < WorldLatitudes.Length; i++)
            {
                if(tileLatitude < WorldLatitudes[i].latitude)
                {
                    return WorldLatitudes[i - 1];
                }
            }
            return WorldLatitudes[WorldLatitudes.Length - 2];
        }

        private static WindLatitude GetLowerLatitude(double tileLatitude)
        {
            for (int i = 0; i < WorldLatitudes.Length; i++)
            {
                if (tileLatitude < WorldLatitudes[i].latitude)
                {
                    return WorldLatitudes[i];
                }
            }
            return WorldLatitudes[WorldLatitudes.Length - 1];
        }

        public static Color GetWindColor(MapTile t)
        {
            double windDirA;
            double windDirB;
            int quadrant = ResolveQuadrant(t.WindDirection);
            if(quadrant == 1)
            {
                windDirA = 0;
                windDirB = 0.5d * Math.PI;

            } else if(quadrant == 2)
            {
                windDirA = 0.5d * Math.PI;
                windDirB = Math.PI;
            }
            else if (quadrant == 3)
            {
                windDirA = Math.PI;
                windDirB = 1.5d * Math.PI;
            } 
            else
            {
                windDirA = 1.5d * Math.PI;
                windDirB = 2d * Math.PI;
            }
            //Console.WriteLine(windDirA);
            Color uC = Array.Find(WindColors, wc => wc.Direction == windDirA).Color;
            Color lC = Array.Find(WindColors, wc => wc.Direction == windDirB).Color;

            double p = MathHelper.Scale(windDirA, windDirB, 0, 1, t.WindDirection);
            
            return new Color(
                (byte)(uC.R * (1 - p) + lC.R * p),
                (byte)(uC.G * (1 - p) + lC.G * p),
                (byte)(uC.B * (1 - p) + lC.B * p),
                255);
        }            

        private static int ResolveQuadrant(double dir)
        {
            dir %= Math.PI * 2;
            if (dir < 0) dir += 2 * Math.PI;
            return (int)Math.Floor((2 * dir / Math.PI) % 4 + 1);
        }
    }
}
