using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;

namespace ReSource
{
    class TextureManager
    {
        private static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        public static Texture GetTexture(string textureName)
        {        
            foreach(string tName in textures.Keys)
            {
                if(tName == textureName)
                {                    
                    return textures[textureName];
                }
            }

            //if texture isn't already loaded then go ahead and get it
            Texture t = new Texture(@"..\..\..\resources\" + textureName);
            textures.Add(textureName, t);
            return t;
        }
    }
}
