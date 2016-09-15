using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.System;
using SFML.Graphics;

namespace ReSource
{
    class MapMoveEventArgs : EventArgs
    {
        public float ZoomLevel;
        public View View;

        public MapMoveEventArgs(float zoomLevel, View view)
        {
            this.ZoomLevel = zoomLevel;
            this.View = view;
        }
    }
}
