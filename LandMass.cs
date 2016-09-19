using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.System;

namespace ReSource
{
    class Landmass
    {
        public List<MapTile> Tiles { get; private set; }
        private Vector2i CenterOfMass;
        
        public int LandMassId;

        public Landmass(int id)
        {
            Tiles = new List<MapTile>();
            this.LandMassId = id;            
        }

        public void AddTile(MapTile t)
        {
            Tiles.Add(t);
        }

        public List<MapTile> GetCoastTiles()
        {
            return Tiles.Where(t => t.Coast).ToList();
        }

        public Vector2i GetCenter()
        {
            if(CenterOfMass == null)
            {
                Vector2f avg = new Vector2f(0, 0);

                foreach(MapTile t in Tiles)
                {
                    avg += (Vector2f)t.GlobalIndex;
                }

                CenterOfMass = new Vector2i((int)Math.Round(avg.X / Tiles.Count()), (int)Math.Round(avg.Y / Tiles.Count()));
            }

            return CenterOfMass;
        }
    }
}
