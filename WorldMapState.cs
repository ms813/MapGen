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
    class WorldMapState : GameState
    {
        private WorldMap worldMap;        
        public View gameView { get; private set; }        

        public event EventHandler<MouseMoveEventArgs> mouseMoved;
        public event EventHandler<MouseButtonEventArgs> mouseButtonPressed;
        public event EventHandler<MouseButtonEventArgs> mouseButtonReleased;
        public event EventHandler<MapMoveEventArgs> mapMoved;
        public event EventHandler<KeyEventArgs> keyPressed;

        private ActionState actionState = ActionState.NONE;
        private float ZoomLevel = 1f;      
        private float MaxZoom;

        public WorldMapState(RenderWindow window)
        {
            double aspectRatio = (double)Game.WindowSize.X / (double)Game.WindowSize.Y;

            MapIO mapIO = new MapIO();
            worldMap = new WorldMap(mapIO.Load());

            //hook up input listeners
            BindListeners();            

            gameView = new View();            
            gameView.Center = new Vector2f(
                worldMap.MapSize.X / 2 * worldMap.TileSize,
                worldMap.MapSize.Y / 2 * worldMap.TileSize);
            gameView.Size = new Vector2f(
                worldMap.TileSize * window.Size.X / window.Size.Y,
                worldMap.TileSize);

            MaxZoom = worldMap.MapSize.Y * 2;
            ZoomLevel = worldMap.MapSize.Y;
            gameView.Zoom(ZoomLevel);
            mapMoved(window, new MapMoveEventArgs(ZoomLevel, gameView));
        }

        public void Draw(RenderWindow window)
        {
            window.SetView(gameView);         
            worldMap.Draw(window);            
        }

        public void Update(float dt)
        {
            worldMap.Update(dt, gameView.Center);
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
                
                if(actionState == ActionState.PANNING)
                {
                    actionState = ActionState.NONE;
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

            mouseButtonReleased(sender, e);
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

        public void BindListeners()
        {
            mouseMoved += new EventHandler<MouseMoveEventArgs>(worldMap.OnMouseMoved);
            mouseButtonPressed += new EventHandler<MouseButtonEventArgs>(worldMap.OnMouseButtonPressed);
            mouseButtonReleased += new EventHandler<MouseButtonEventArgs>(worldMap.OnMouseButtonReleased);
            mapMoved += new EventHandler<MapMoveEventArgs>(worldMap.OnMapMoved);
            keyPressed += new EventHandler<KeyEventArgs>(worldMap.OnKeyPressed);
        }

        public void UnbindListeners()
        {            
            mouseMoved -= worldMap.OnMouseMoved;
            mouseButtonPressed -= worldMap.OnMouseButtonPressed;
            mouseButtonReleased -= worldMap.OnMouseButtonReleased;
            mapMoved -= worldMap.OnMapMoved;
            keyPressed -= worldMap.OnKeyPressed;
        }
    }
}
