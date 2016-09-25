using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;

namespace ReSource
{
    class Biome
    {       
        public static readonly Biome Snow = new Biome("Snow", Color.White);                                     //white
        public static readonly Biome Tundra = new Biome("Tundra", new Color(128, 255, 255, 255));                    //pale cyan
        public static readonly Biome Bare = new Biome("Bare", new Color(218, 218, 218, 255));                   //grey
        public static readonly Biome Scorched = new Biome("Scorched", Color.Red);                                   //red        
        public static readonly Biome Taiga = new Biome("Taiga", new Color(0, 128, 128, 255));                    //turqoise       
        public static readonly Biome Shrubland = new Biome("Shrubland", new Color(128, 255, 128, 255));              //pale green
        public static readonly Biome Grassland = new Biome("Grassland", Color.Green);                                //green
        public static readonly Biome ColdDesert = new Biome("ColdDesert", new Color(255, 255, 128, 255));             //pale yellow
        public static readonly Biome HotDesert = new Biome("HotDesert", Color.Yellow);                               //yellow
        public static readonly Biome TemperateRainforest = new Biome("TemperateRainforest", new Color(255, 128, 255, 255));    //light pink
        public static readonly Biome TropicalRainforest = new Biome("TropicalRainforest", Color.Magenta);                     //magenta
        public static readonly Biome Swamp = new Biome("Swamp", new Color(0, 128, 0, 255));                      //dark green
        public static readonly Biome TropicalSeasonalForest = new Biome("TropicalSeasonalForest", new Color(128, 128, 255, 255)); //pale blue
        public static readonly Biome Shallows = new Biome("Shallows", Color.Cyan);                                  //cyan
        public static readonly Biome Ocean = new Biome("Ocean", Color.Blue);                                     //blue
        public static readonly Biome Depths = new Biome("Depths", Color.Black);                                   //black
        public static readonly Biome Lake = new Biome("Lake", Color.Blue);                                      //blue
        public static readonly Biome AlpineLake = new Biome("AlpineLake", Color.Blue);                                //blue

        public int Count { get; set; }

        public static List<Biome> biomeList = new List<Biome> 
        { 
            Snow, Tundra, Bare, Scorched, Taiga, Shrubland, Grassland,
            ColdDesert, HotDesert, TemperateRainforest, TropicalRainforest,
            Swamp, TropicalSeasonalForest, Shallows, Ocean, Depths, Lake, AlpineLake
        };

        public Color Color { get; private set; }
        public String Name { get; private set; }
        public Biome(String name, Color c)
        {
            this.Name = name;
            this.Color = c;
            this.Count = 0;
        }

        //lookup table for biomes
        //first index is the moisture level
        //second index is the elevation level
        public static readonly Biome[,] BiomeTable = new Biome[,]        
        {
/*Elevation Zone*/      /*0*/                       /*1*/                       /*2*/               /*3*/
/*Moisture Zone*/
            /*0*/ {Biome.HotDesert,                 Biome.ColdDesert,            Biome.ColdDesert,   Biome.Scorched},
            /*1*/ {Biome.Grassland,                 Biome.Grassland,            Biome.ColdDesert,   Biome.Bare},
            /*2*/ {Biome.TropicalSeasonalForest,    Biome.Grassland,            Biome.Shrubland,    Biome.Tundra},
            /*3*/ {Biome.TropicalSeasonalForest,    Biome.Swamp,                Biome.Shrubland,    Biome.Snow},
            /*4*/ {Biome.TropicalRainforest,        Biome.Swamp,                Biome.Taiga,        Biome.Snow},
            /*5*/ {Biome.TropicalRainforest,        Biome.TemperateRainforest,  Biome.Taiga,        Biome.Snow}
        };
    }
}