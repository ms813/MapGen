using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.System;

namespace ReSource
{
    class TectonicPlate
    {
        public List<MapTile> PlateTiles { get; private set; } //list of tiles that make up this plate
        public MapTile Centertile { get; private set; }
        public double Rotation { get; private set; }    //rotation around the center tile between -1 and 1
        public Vector2f Drift { get; private set; }     //random unit vector
        public bool Oceanic { get; private set; }       //chance for plate to be oceanic or land     
        public int PlateId { get; private set; }
        public double MaxElevation { get; private set; }
        public double MinElevation { get; private set; }

        private readonly double OceanicWeight = 0.6d;
        private readonly double RotationWeight = 1.5d;

        public TectonicPlate(int plateId, MapTile centertile)
        {
            this.PlateId = plateId;
            this.Centertile =  CheckTilePlateId(centertile);
            Initialise();            
        }

        private void Initialise()
        {
            PlateTiles = new List<MapTile>();
            this.Rotation = RotationWeight * MathHelper.rnd.NextDouble() - RotationWeight / 2;
            this.Drift = new Vector2f((float)MathHelper.rnd.NextDouble(), (float)MathHelper.rnd.NextDouble());
            this.Drift = MathHelper.Normalise(Drift);
            
            //chance for plate to be oceanic or land
            this.Oceanic = MathHelper.rnd.NextDouble() < OceanicWeight;

            //Elevation
            if (Oceanic)
            {
                MaxElevation = MathHelper.rnd.NextDouble() * WorldMap.SeaLevel;
                MinElevation = 0;
            }
            else
            {
                do
                {
                    MaxElevation = MathHelper.rnd.NextDouble() * (WorldMap.MaxElevation - WorldMap.SeaLevel) + WorldMap.SeaLevel;
                    MinElevation = MathHelper.rnd.NextDouble() * (WorldMap.MinElevation + WorldMap.SeaLevel) + WorldMap.SeaLevel;
                } while (MaxElevation < MinElevation);
            }
        }

        public void AddTile(MapTile t)
        {            
            PlateTiles.Add(CheckTilePlateId(t));
        }

        //check incoming tiles to ensure their plateId matches up with their plate
        private MapTile CheckTilePlateId(MapTile t)
        {
            if (t.PlateId != this.PlateId)
            {
                t.PlateId = this.PlateId;
            }

            return t;
        }
    }
}