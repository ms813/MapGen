using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace ReSource
{
    class Game
    {
        private Stack<GameState> gameStates = new Stack<GameState>();
        public static Vector2u WindowSize;

        public void Start()
        {
            Styles windowStyle = Styles.Close;

            //get the size of the primary system monitor and set the window to half that size
            System.Drawing.Rectangle screenSize = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            WindowSize = new Vector2u((uint)screenSize.Width / 2, (uint)screenSize.Height/2);
            
            //create the game window
            RenderWindow window = new RenderWindow(new VideoMode(WindowSize.X, WindowSize.Y), "", windowStyle);

            /*
             * Bind event handlers
             */
            window.Closed += new EventHandler(OnClose);
            window.MouseButtonPressed += new EventHandler<MouseButtonEventArgs>(OnMouseButtonPressed);
            window.MouseMoved += new EventHandler<MouseMoveEventArgs>(OnMouseMoved);
            window.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPressed);
            window.MouseButtonReleased += new EventHandler<MouseButtonEventArgs>(OnMouseButtonReleased);
            window.MouseWheelMoved += new EventHandler<MouseWheelEventArgs>(OnMouseWheelMoved);
           
            window.SetFramerateLimit(60);

            gameStates.Push(new TestState(window));

            Clock gameClock = new Clock();
            
            while(window.IsOpen)
            {
                GameState currentState = gameStates.Peek();
                
                float dt = gameClock.Restart().AsSeconds();
                //Console.Clear();
                //Console.WriteLine(1/dt);
                window.DispatchEvents();

                currentState.Update(dt);

                window.Clear();
                currentState.Draw(window);
                window.Display();
            }
        }

        public void OnClose(object sender, EventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }

        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            gameStates.Peek().OnMouseButtonPressed(sender, e);
        }

        private void OnMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            gameStates.Peek().OnMouseButtonReleased(sender, e);
        }
        
        private void OnMouseMoved(object sender, MouseMoveEventArgs e)
        {            
            gameStates.Peek().OnMouseMoved(sender, e);
        }

        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            gameStates.Peek().OnKeyPressed(sender, e);
        }

        private void OnMouseWheelMoved(object sender, MouseWheelEventArgs e)
        {
            gameStates.Peek().OnMouseWheelMoved(sender, e);
        }
    }
}
