using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.System;

namespace ReSource
{
    class MathHelper
    {
        public static Random rnd = new Random();

        public static Vector2i[] CardinalDirections = {
            new Vector2i(0, -1),     //north
            new Vector2i(1, 0),      //east            
            new Vector2i(0, 1),     //south
            new Vector2i(-1, 0)     //west            
        };

        public static Vector2i[] OrdinalDirections = {
            new Vector2i(1,-1),     //northeast
            new Vector2i(1, 1),      //southeast            
            new Vector2i(-1, 1),    //southwest
            new Vector2i(-1, -1)    //northwest
        };
        
        public static double NormalDistribution(double mean = 0, double stdDev = 1)
        {
            double u1 = rnd.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = rnd.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return mean + stdDev * randStdNormal;
        }

        public static double Lerp(double a, double b, double x)
        {         
            return a + x * (b - a);
        }

        public static double SmoothStep(double n)
        {
            return n * n * n * (n * (n * 6 - 15) + 10);
        }

        public static double Scale(double oldMin, double oldMax, double newMin, double newMax, double x)
        {
            return ((newMax - newMin) * (x - oldMin)) / (oldMax - oldMin) + newMin;
        }

        public static T Clamp<T>(T value, T max, T min)
         where T : System.IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        } 


        //Vector math
        public static float Magnitude(Vector2f v)
        {
            return (float)Math.Sqrt(Math.Pow(v.X, 2) + Math.Pow(v.Y, 2));
        }

        public static float Dot(Vector2f a, Vector2f b)
        {
            return (a.X * b.X) + (a.Y * b.Y);
        }

        public static float Dot(Vector2i a, Vector2i b)
        {
            return Dot(new Vector2f(a.X, a.Y), new Vector2f(b.X, b.Y));
        }

        public static Vector2i ToCardinalDirection(Vector2f v)
        {
            if(v.X >= v.Y)
            {
                if (v.X < 0)
                {
                    return new Vector2i(-1, 0);
                }
                else
                {
                    return new Vector2i(1, 0);
                }
            } else{
                if (v.Y < 0)
                {
                    return new Vector2i(0, -1);
                }
                else
                {
                    return new Vector2i(0, 1);
                }
            }
        }

        public static Vector2f Normalise(Vector2i v)
        {
            return Normalise((Vector2f)v);
        }

        public static Vector2f Normalise(Vector2f v)
        {
            float mag = Magnitude(v);
            return new Vector2f(v.X / mag, v.Y / mag);
        }

        public static Vector2f UnitNormal(Vector2f v)
        {
            return Normalise(new Vector2f(-v.Y, v.X));
        }
        
        public static Vector2f UnitNormal(Vector2i v)
        {
            return UnitNormal(new Vector2f(v.X, v.Y));
        }        
    }
}
