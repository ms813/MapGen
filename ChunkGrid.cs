using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.System;
using SFML.Graphics;

namespace ReSource
{
    class ChunkGrid
    {
        private Dictionary<Vector2i, Chunk> chunks = new Dictionary<Vector2i, Chunk>();
        private int cols;
        private int rows;
        private int chunkSize; //tiles per side
        
        
        public ChunkGrid(int width, int height, int chunkSize)
        {
            this.cols = (int)Math.Ceiling((double)width/chunkSize);
            this.rows = (int)Math.Ceiling((double)height/chunkSize);
            this.chunkSize = chunkSize;
        }

        public void Add(MapTile t)
        {
            Vector2i index = t.GlobalIndex / chunkSize;
            if(!chunks.ContainsKey(TileToChunkIndex(t)))
            {                
                Chunk chunk = new Chunk(index);               
                chunks.Add(index, chunk);                       
            }

            chunks[index].Add(t);                     
        }

        private Vector2i TileToChunkIndex(MapTile t)
        {       
            return new Vector2i(t.GlobalIndex.X / chunkSize, t.GlobalIndex.Y / chunkSize);
        }

        private Chunk TileToChunk(MapTile t)
        {
            return chunks[t.GlobalIndex / chunkSize];
        }

        public MapTile FindNearestCoast(MapTile sourceTile)
        {
            
            Chunk sourceChunk = TileToChunk(sourceTile);
            MapTile nearestCoastTile = null;    

            if(sourceChunk.hasCoast)
            {
                return sourceChunk.FindNearestCoastTile(sourceTile);
            }
            else
            {
                IEnumerable<Chunk> sortedChunks = chunks.Values.OrderBy(c => c.DistanceFromChunk(sourceChunk)).ToList();

                int closestCoastChunkDist = Int32.MaxValue;
                foreach(Chunk c in sortedChunks)
                {
                    int dist = c.DistanceFromChunk(sourceChunk);
                    if (c.hasCoast && dist <= closestCoastChunkDist)
                    {
                        closestCoastChunkDist = dist;    
                    }
                }      
          
                IEnumerable<Chunk> chunksAtRadius = chunks.Values.Where(c =>
                {
                    return c.DistanceFromChunk(sourceChunk) == closestCoastChunkDist;
                });

                int closestCoastTileDist = Int32.MaxValue;                    
                foreach(Chunk c in chunksAtRadius)
                {            
                    if(c.hasCoast)
                    {
                        MapTile t = c.FindNearestCoastTile(sourceTile);
                        int dist = MathHelper.TaxicabDistance(t.GlobalIndex, sourceTile.GlobalIndex);
                        if (dist < closestCoastTileDist)
                        {
                            closestCoastTileDist = dist;
                            nearestCoastTile = t;
                        }
                    }                    
                }
                return nearestCoastTile;
            }                          
        }

        public void Draw(RenderWindow window, int tileSize)
        {
            foreach(Chunk c in chunks.Values)
            {
                c.Draw(window, chunkSize, tileSize);
            }
        }

        internal List<Chunk> GetChunkNeighbours(Chunk c)
        {
            List<Chunk> n = new List<Chunk>();
            if (c.Index.X > 0) n.Add(chunks[c.Index + MathHelper.CardinalDirections[3]]);
            if (c.Index.X < cols - 1) n.Add(chunks[c.Index + MathHelper.CardinalDirections[1]]);
            if (c.Index.Y > 0) n.Add(chunks[c.Index + MathHelper.CardinalDirections[0]]);
            if (c.Index.Y < rows - -1) n.Add(chunks[c.Index + MathHelper.CardinalDirections[2]]);
            return n;
        }
    }

    internal class Chunk
    {
        private List<MapTile> tiles = new List<MapTile>();
        internal Vector2i Index { get; private set; }
        internal bool hasCoast = false;        

        internal Chunk(Vector2i index)
        {
            this.Index = index;                    
        }

        internal void Add(MapTile t)
        {
            tiles.Add(t);
            if(t.Coast)
            {
                hasCoast = true;
            }            
        }

        internal MapTile FindNearestCoastTile(MapTile sourceTile)
        {
            if(!hasCoast)
            {
                return null;
            }
            else
            {
                IEnumerable<MapTile> coastTiles = tiles.Where(t => t.Coast);
                int min = Int32.MaxValue;
                MapTile closestTile = coastTiles.First();
                foreach(MapTile c in coastTiles)
                {
                    int dist = MathHelper.TaxicabDistance(c.GlobalIndex, sourceTile.GlobalIndex);
                    if(dist < min)
                    {
                        min = dist;
                        closestTile = c;
                    }
                }
                return closestTile;
            }
        }

        internal void Draw(RenderWindow window, int chunkSize, int tileSize)
        {
            RectangleShape r = new RectangleShape(new Vector2f(chunkSize * tileSize, chunkSize * tileSize));
            r.FillColor = Color.Transparent;
            r.OutlineColor = Color.White;
            r.OutlineThickness = -8;
            r.Position = (Vector2f)Index * tileSize * chunkSize;            

            window.Draw(r);
        }

        internal int DistanceFromChunk(Chunk c)
        {
            return MathHelper.TaxicabDistance(c.Index, this.Index);
        }
    }
}
