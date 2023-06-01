using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.IO;

namespace multi_pass_stress_test
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        GraphicsManager _gm;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _gm = new GraphicsManager(this.GraphicsDevice);
            _gm.effect = Content.Load<Effect>("effect");
            FontManager.LoadFont(Texture2D.FromStream(this.GraphicsDevice, File.Open("Content/font.png", FileMode.Open)), "Content/font.json");
            _gm.text_effect = Content.Load<Effect>("text");
            _gm.text_effect.Parameters["tex"].SetValue(FontManager.tex);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            base.Update(gameTime);
        }


        int fpscounter = 0;
        double fps = 0, fpstimer = 0;
        string fpstext = "no";
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(.2f, .2f, .2f));
            float x = 100 + (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds * .001) * 50;
            float y = 100 + (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds * .001 + 1) * 50;
            for (int i = 0; i < 10000; i++)
            {
                _gm.begin(Vector2.Zero, new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight));
                _gm.draw(new Vector2(x, y), new Vector2(100, 100), new Color(.8f, .8f, .8f));
                _gm.flush();
            }

            fpstimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            fpscounter++;
            if (fpstimer > 1000)
            {

                fpstext = $"media: {1000f / (fpstimer / fpscounter)} last: {1000f / gameTime.ElapsedGameTime.TotalMilliseconds}";
                fpstimer -= 1000;
                fpscounter = 0;
            }
            _gm.DrawString(fpstext, new Vector2(50, 50), 1, .5f);
            _gm.flush_text();
            base.Draw(gameTime);
        }
    }
}