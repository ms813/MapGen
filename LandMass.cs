using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReSource
{
    class Landmass
    {
        public List<MapTile> Tiles { get; private set; }
        
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

        public IEnumerable<MapTile> GetCoastTiles()
        {
            return Tiles.Where(t => t.Coast);
        }
    }
}
