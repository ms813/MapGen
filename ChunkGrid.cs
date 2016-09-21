using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.System;

namespace ReSource
{
    class ChunkGrid
    {        
        private List<Chunk> chunks = new List<Chunk>();
        private int width;
        private int height;
        private int chunkSize; //tiles per side
        
        
        public ChunkGrid(int width, int height, int chunkSize)
        {
            this.width = (int)Math.Ceiling((double)width/chunkSize);
            this.height = (int)Math.Ceiling((double)height/chunkSize);            
        }

        public void Add(MapTile t)
        {
        }

    }

    internal class Chunk
    {
        private List<MapTile> tiles;
        public Vector2i Index { get; private set; }

        public Chunk(int left, int top)
        {
            this.Index = new Vector2i(left, top);
        }
    }
}
