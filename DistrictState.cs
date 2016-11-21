using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;

namespace ReSource
{
    class DistrictState : GameState
    {
        private District district;

        public DistrictState(District district)
        {
            this.district = district;
        }        

        public void Draw(RenderWindow window)
        {
            
        }

        public void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if(e.Code == Keyboard.Key.Escape)
            {
                Game.PopState();
            }
        }

        public void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            
        }

        public void OnMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
        }

        public void OnMouseMoved(object sender, MouseMoveEventArgs e)
        {
        }

        public void OnMouseWheelMoved(object sender, MouseWheelEventArgs e)
        {
        }

        public void BindListeners()
        {
        }

        public void UnbindListeners()
        {
        }

        public void Update(float dt)
        {
        }
    }
}