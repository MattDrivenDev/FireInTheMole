using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using FireInTheMole.Game;

namespace Trinket
{
    public class TrinketGame : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        UI.SliderData _slider;
        UI.MenuData _pauseMenu;

        public TrinketGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
        }

        protected override void Initialize()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Fonts.loadFonts(Content);
            UI.loadTextures(GraphicsDevice, Content);

            _slider = UI.slider(
                position: new Vector2(20, 20),
                size: new Vector2(400, 20));

            _pauseMenu = UI.pauseMenu();

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            _slider = UI.updateSlider(gameTime, Keyboard.GetState(), _slider);            
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Window.Title = $"Trinket - FPS: {Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds)}";
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            UI.drawSlider(_spriteBatch, gameTime, _slider);
            UI.drawMenu(_spriteBatch, gameTime, _pauseMenu);

            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
