using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;

namespace ReSource
{
    public struct WindColor
    {
        public double Direction;
        public Color Color;
        public WindColor(double dir, Color c)
        {
            this.Direction = dir;
            this.Color = c;
        }
    }
}
