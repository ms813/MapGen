using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.System;

namespace ReSource
{
    class District
    {
        private static int size = 32;
        public static int Size { get { return size; } private set { size = value; } }
        public static int TileSize { get; private set; }

        private MapTile parentTile;

        List<LocalTile> tiles;

        public District(MapTile parentTile)
        {
            this.parentTile = parentTile;

            tiles = CreateTiles();
        }

        private List<LocalTile> CreateTiles()
        {
            List<LocalTile> tiles = new List<LocalTile>();

            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    tiles.Add(new LocalTile(this, new Vector2i(x, y), TileSize));
                }
            }

            return tiles;
        }

        private void AssignTileNeighbours()
        {
            Parallel.ForEach(tiles, t =>
            {
                foreach (Vector2i dir in MathHelper.CardinalDirections)
                {
                    LocalTile neighbour = GetTileByIndex(t.LocalIndex + dir);
                    if (neighbour != null)
                    {
                        t.OrthogonalNeighbours.Add(neighbour);
                    }
                }

                foreach (Vector2i dir in MathHelper.OrdinalDirections)
                {
                    LocalTile neighbour = GetTileByIndex(t.LocalIndex + dir);
                    if (neighbour != null)
                    {
                        t.DiagonalNeighbours.Add(neighbour);
                    }
                }
            });
        }

        private LocalTile GetTileByIndex(int x, int y)
        {
            return GetTileByIndex(new Vector2i(x, y));
        }

        private LocalTile GetTileByIndex(Vector2i index)
        {
            int i = index.X * Size + index.Y;

            if (index.X >= 0 && index.X < Size
                && index.Y >= 0 && index.Y < Size
                && i >= 0 && i < tiles.Count())
            {
                return tiles[i];
            }
            else
            {
                //Console.WriteLine("Tile: {0}, index: {1}", Tiles[i].GlobalIndex, index);
                return null;
            }
        }
    }
}
