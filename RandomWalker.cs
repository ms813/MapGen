using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.System;
using SFML.Graphics;

namespace ReSource
{
    class RandomWalker
    {
        private Random rnd;
        public RandomWalker(Random rnd)
        {
            this.rnd = rnd;
        }

        public VertexArray GridWalk(Vector2i startPos, IntRect bounds, int gridSize, int steps)
        {
            VertexArray vertices = new VertexArray(PrimitiveType.LinesStrip);           

            //start point                    
            Vector2i lastPos = startPos;
            
            //add each point in turn
            for (int i = 0; i < steps; i++)
            {
                Vertex v = new Vertex(new Vector2f(lastPos.X, lastPos.Y));
                if (i == 0)
                {
                    v.Color = Color.Magenta;
                }
                else if (i == steps - 1)
                {
                    v.Color = Color.Cyan;
                }
                else
                {
                    v.Color = Color.White;
                }
                vertices.Append(v);  

                Vector2i nextDir = NextGridStepDir(lastPos, bounds);
                lastPos = lastPos + nextDir * gridSize;                              
            }

            return vertices;
        }

        private Vector2i NextGridStepDir(Vector2i lastPos, IntRect bounds)
        {
                    
            Vector2i dir = MathHelper.CardinalDirections[rnd.Next(4)];
            while(!bounds.Contains(dir.X + lastPos.X, dir.Y + lastPos.Y))
            {
                dir = MathHelper.CardinalDirections[rnd.Next(4)];
            }            
            
            return dir;
        }     
   
        public VertexArray RandomWalk(Vector2f startPos, IntRect bounds, double maxStepSize, int steps)
        {
            VertexArray vertices = new VertexArray(PrimitiveType.LinesStrip);
            
            Vector2f lastPos = startPos;

            for(int i = 0; i < steps; i++)
            {               
                Vertex v = new Vertex(new Vector2f(lastPos.X, lastPos.Y));
                if (i == 0)
                {
                    v.Color = Color.Magenta;
                }
                else if (i == steps - 1)
                {
                    v.Color = Color.Cyan;
                }
                else
                {
                    v.Color = Color.White;
                }
                vertices.Append(v);

                lastPos = NextStep(lastPos, bounds, maxStepSize);
            }

            return vertices;
        }

        private Vector2f NextStep(Vector2f lastPos, IntRect bounds, double maxStepSize)
        {
            double stepLength = rnd.NextDouble() * maxStepSize;
            double angle = rnd.NextDouble() * 2 * Math.PI;

            double x = stepLength * Math.Cos(angle) + lastPos.X;
            double y = stepLength * Math.Sin(angle) + lastPos.Y;
            
            while(!bounds.Contains((int)x, (int)y))
            {
                stepLength = rnd.NextDouble() * maxStepSize;
                angle = rnd.NextDouble() * 2 * Math.PI;

                x = stepLength * Math.Cos(angle) + lastPos.X;
                y = stepLength * Math.Sin(angle) + lastPos.Y;
            }

            return new Vector2f((float)x, (float)y);
        }
    }
}
