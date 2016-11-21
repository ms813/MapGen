using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.System;

namespace ReSource
{
    class LocalTile
    {
        public int TileSize { get; private set; }
        public District ParentDistrict { get; private set; }
        public Vector2i LocalIndex { get; private set; }

        public List<LocalTile> OrthogonalNeighbours = new List<LocalTile>();
        public List<LocalTile> DiagonalNeighbours = new List<LocalTile>();

        public LocalTile(District parentDistrict, Vector2i localIndex, int tileSize)
        {
            this.ParentDistrict = parentDistrict;
            this.LocalIndex = localIndex;
            this.TileSize = tileSize;
        }
    }
}
