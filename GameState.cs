using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;
using SFML.Window;

namespace ReSource
{
    interface GameState
    {
        void Draw(RenderWindow window);
        void Update(float dt);
        void OnMouseButtonPressed(object sender, MouseButtonEventArgs e);
        void OnMouseMoved(object sender, MouseMoveEventArgs e);
        void OnKeyPressed(object sender, KeyEventArgs e);
        void OnMouseButtonReleased(object sender, MouseButtonEventArgs e);
        void OnMouseWheelMoved(object sender, MouseWheelEventArgs e);
    }
}
