using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;
using SFML.Window;
using SFML.System;

namespace ReSource
{
    class TestState : GameState
    {
        private WorldMap map;        
        public View gameView { get; private set; }        

        public event EventHandler<MouseMoveEventArgs> mouseMoved;
        public event EventHandler<MouseButtonEventArgs> mouseButtonPressed;
        public event EventHandler<MapMoveEventArgs> mapMoved;
        public event EventHandler<KeyEventArgs> keyPressed;

        private ActionState actionState = ActionState.NONE;
        private float ZoomLevel = 1f;        
        private int mapY = 128;
        private float MaxZoom;

        public TestState(RenderWindow window)
        {
            double aspectRatio = (double)Game.WindowSize.X / (double)Game.WindowSize.Y;
            Vector2i mapSize = new Vector2i((int)Math.Round(aspectRatio * (double)mapY), mapY);
            Console.WriteLine(mapSize);
            MapIO mapIO = new MapIO();
            map = new WorldMap(mapIO.Load());

            //hook up input listeners
            mouseMoved += new EventHandler<MouseMoveEventArgs>(map.OnMouseMoved);
            mouseButtonPressed += new EventHandler<MouseButtonEventArgs>(map.OnMouseButtonPressed);
            mapMoved += new EventHandler<MapMoveEventArgs>(map.OnMapMoved);
            keyPressed += new EventHandler<KeyEventArgs>(map.OnKeyPressed);

            gameView = new View();            
            gameView.Center = new Vector2f(
                mapSize.X / 2 * map.TileSize,
                mapSize.Y / 2 * map.TileSize);
            gameView.Size = new Vector2f(
                map.TileSize * window.Size.X / window.Size.Y,
                map.TileSize);

            MaxZoom = mapY * 2;
            ZoomLevel = mapY;
            gameView.Zoom(ZoomLevel);
            mapMoved(window, new MapMoveEventArgs(ZoomLevel, gameView));
        }

        public void Draw(RenderWindow window)
        {
            window.SetView(gameView);         
            map.Draw(window);            
        }

        public void Update(float dt)
        {
            map.Update(dt, gameView.Center);
        }

        private Vector2i panningAnchor;
        public void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            mouseButtonPressed(sender, e);
            //Console.WriteLine(e);

            //toggle on the Panning action state if the left mouse button is pressed
            if(e.Button == Mouse.Button.Left)
            {
                if(actionState != ActionState.PANNING)
                {
                    panningAnchor = Mouse.GetPosition((RenderWindow)sender);
                    actionState = ActionState.PANNING;                    
                }                
            }
        }

        public void OnMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            //toggle off the panning state if the left mouse button is pressed
            if(e.Button == Mouse.Button.Left)
            {
                if(actionState == ActionState.PANNING)
                {
                    actionState = ActionState.NONE;
                }
            }
        }

        public void OnMouseMoved(object sender, MouseMoveEventArgs e)
        {
            //if the panning state is active then pan the camera
            if (actionState == ActionState.PANNING)
            {
                Vector2i iPos = Mouse.GetPosition((RenderWindow) sender) - panningAnchor;
                Vector2f fPos = new Vector2f(iPos.X, iPos.Y);

                gameView.Move(-1.0f * fPos * (float)Math.Sqrt(ZoomLevel));
                panningAnchor = Mouse.GetPosition((RenderWindow)sender);

                mapMoved(sender, new MapMoveEventArgs(ZoomLevel, gameView));
            }
            mouseMoved(sender, e);          
        }

        public void OnMouseWheelMoved(object sender, MouseWheelEventArgs e)
        {           
            if(e.Delta < 0)
            {
                if(ZoomLevel < 1f * MaxZoom)
                {
                    gameView.Zoom(2.0f);
                    ZoomLevel *= 2f;
                    mapMoved(sender, new MapMoveEventArgs(ZoomLevel, gameView));
                }                
            }
            else
            {
                if (ZoomLevel > 1f / MaxZoom)
                {
                    gameView.Zoom(0.5f);
                    ZoomLevel *= 0.5f;
                    mapMoved(sender, new MapMoveEventArgs(ZoomLevel, gameView));
                }                
            }            
        }

        public void OnKeyPressed(object sender, KeyEventArgs e)
        {
            //Console.WriteLine(e);
            keyPressed(sender, e);
        }          
    }
}
