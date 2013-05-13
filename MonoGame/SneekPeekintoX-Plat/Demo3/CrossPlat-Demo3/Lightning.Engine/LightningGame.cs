using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Lightning.Engine
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class LightningGame
	{
		enum Mode { SimpleLightning, BranchLightning, LightningText }
		Mode mode;

        GraphicsDevice graphicsDevice;
        Point screenSize;

		SpriteBatch spriteBatch;
		SpriteFont lightningFont, infoFont;

		KeyboardState keyState, lastKeyState;
		MouseState mouseState, lastMouseState;
        TouchCollection touches, previousTouches;

		List<ILightning> bolts = new List<ILightning>();
		LightningText lightningText;
		RenderTarget2D lastFrame, currentFrame;

        int displayWidth, displayHeight;

        double updateFrequency = 1.7f;
        double runningclock;

        //string platform;

		//public LightningGame(string platform)
		public LightningGame()
		{
            //this.platform = platform;
		}

        public void Initialize(GraphicsDevice graphicsDevice)
		{
            TouchPanel.EnabledGestures =
                GestureType.Flick;

            this.graphicsDevice = graphicsDevice;

            displayWidth = graphicsDevice.Viewport.Width;
            displayHeight = graphicsDevice.Viewport.Height;
		}

		public void LoadContent(ContentManager Content)
		{
            spriteBatch = new SpriteBatch(graphicsDevice);
            infoFont = Content.Load<SpriteFont>("InfoFont");
            lightningFont = Content.Load<SpriteFont>("LightningFont");

            //switch (platform)
            //{
            //    case "WP7":
            //        lightningFont = Content.Load<SpriteFont>("LightningFontSm");
            //        break;
            //    case "WP8":
            //        lightningFont = Content.Load<SpriteFont>("LightningFontMed");
            //        break;

            //    default:
            //        lightningFont = Content.Load<SpriteFont>("LightningFontLrg");
            //        break;
            //}


            Art.Load(Content);
            OrientationChanged("Landscape");
		}

        public void OrientationChanged(string orientation)
        {
            displayWidth = graphicsDevice.Viewport.Width;
            displayHeight = graphicsDevice.Viewport.Height;

            screenSize = new Point(displayWidth, displayHeight);

            lastFrame = new RenderTarget2D(graphicsDevice, screenSize.X, screenSize.Y, false, SurfaceFormat.Color, DepthFormat.None);
            currentFrame = new RenderTarget2D(graphicsDevice, screenSize.X, screenSize.Y, false, SurfaceFormat.Color, DepthFormat.None);
            lightningText = new LightningText(graphicsDevice, screenSize, spriteBatch, lightningFont, "Lightning");

            // Initialize lastFrame to be solid black
            graphicsDevice.SetRenderTarget(lastFrame);
            graphicsDevice.Clear(Color.Black);
            graphicsDevice.SetRenderTarget(null);


        }

        public void Update(double elapsedTime)
		{
            
    		lastKeyState = keyState;
			keyState = Keyboard.GetState();
			lastMouseState = mouseState;
			mouseState = Mouse.GetState();

            if (WasPressed(Keys.Space))
                ChangeDisplayMode();

            HandleGestures();

            var screenSize = new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
			var mousePosition = new Vector2(mouseState.X, mouseState.Y);

            previousTouches = touches;
            touches = TouchPanel.GetState();
            if (touches.Count > 0 && CanAddNewBolt(elapsedTime))
            {
                for (int i = 0; i < touches.Count; i++)
                {
                    if (touches.Count == 1)
                    {

                        AddBolt(screenSize / 2, touches[i].Position);
                    }
                    else
                    {
                        if (i > 0)
                            AddBolt(touches[i - 1].Position, touches[i].Position);
                    }
                }
            }
            else
            {
                if (WasClicked())
                    AddBolt(screenSize / 2, mousePosition);
            }

            if (mode == Mode.LightningText)
            {
                lightningText.Update();
            }


			foreach (var bolt in bolts)
				bolt.Update();

			bolts = bolts.Where(x => !x.IsComplete).ToList();
		}

		// return true if a key was pressed down this frame
		bool WasPressed(Keys key)
		{
			return keyState.IsKeyDown(key) && lastKeyState.IsKeyUp(key);
		}

		// return true if the left mouse button was clicked down this frame
		bool WasClicked()
		{
			return mouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released;
		}

        public void Draw()
		{
            graphicsDevice.Clear(Color.Black);

			// The lightning text is drawn a bit differently due to our optimization with the render targets.
			if (mode == Mode.LightningText)
                DrawLightningText(graphicsDevice);
			else
                DrawLightning(graphicsDevice);

			spriteBatch.Begin();
			spriteBatch.DrawString(infoFont, "" + mode, new Vector2(5), Color.White);
            spriteBatch.DrawString(infoFont, "Swipe or press space to change mode", new Vector2(5, 30), Color.White);

			if (mode != Mode.LightningText)
				spriteBatch.DrawString(infoFont, "Tap/Click to make lightning", new Vector2(5, 55), Color.White);

			spriteBatch.End();
		}

        void DrawLightningText(GraphicsDevice graphicsDevice)
		{
			graphicsDevice.SetRenderTarget(currentFrame);

			// draw our last frame at 96% of its original brightness
			spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null);
			spriteBatch.Draw(lastFrame, Vector2.Zero, Color.White * 0.96f);
			spriteBatch.End();

			// draw the new lightning bolts
			spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive);
			lightningText.Draw();
			spriteBatch.End();

			// draw currentFrame to the backbuffer
            graphicsDevice.SetRenderTarget(null);
			spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null);
			spriteBatch.Draw(currentFrame, Vector2.Zero, Color.White);
			spriteBatch.End();

			Swap(ref currentFrame, ref lastFrame);
		}

        void DrawLightning(GraphicsDevice graphicsDevice)
		{

			// we use SpriteSortMode.Texture to improve performance
			spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive);

			foreach (var bolt in bolts)
				bolt.Draw(spriteBatch);

			spriteBatch.End();
		}

		void Swap<T>(ref T a, ref T b)
		{
			T temp = a;
			a = b;
			b = temp;
		}

        public void HandleGestures()
        {
            while (TouchPanel.IsGestureAvailable)
            {
                // read the next gesture from the queue
                GestureSample gesture = TouchPanel.ReadGesture();

                // we can use the type of gesture to determine our behavior
                switch (gesture.GestureType)
                {
                    case GestureType.DoubleTap:
                        break;
                    case GestureType.DragComplete:
                        break;
                    case GestureType.Flick:
                        ChangeDisplayMode();
                        break;
                    case GestureType.FreeDrag:
                        break;
                    case GestureType.Hold:
                        break;
                    case GestureType.HorizontalDrag:
                        break;
                    case GestureType.None:
                        break;
                    case GestureType.Pinch:
                        break;
                    case GestureType.PinchComplete:
                        break;
                    case GestureType.Tap:
                        break;
                    case GestureType.VerticalDrag:
                        break;
                    default:
                        break;
                }
            }
        }

        private void ChangeDisplayMode()
        {
                mode = (Mode)(((int)mode + 1) % 3);
        }

        public bool CanAddNewBolt(double elapsedTime)
        {
            runningclock += elapsedTime;
            if (runningclock / 100 >= updateFrequency)
            {
                runningclock = 0;
                return true;
            }

            return false;
        }

        public void AddBolt(Vector2 source, Vector2 dest)
        {
            switch (mode)
            {
                case Mode.SimpleLightning:
                        bolts.Add(new LightningBolt(source, dest));

                    break;
                case Mode.BranchLightning:
                        bolts.Add(new BranchLightning(source, dest));

                    break;
                //default:
                //    bolts.Add(new BranchLightning(source, dest));
                //    break;
            }
        }
	}
}
