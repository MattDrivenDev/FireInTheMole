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
            UI.loadTextures(Content);
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Window.Title = $"Trinket - FPS: {Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds)}";
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            base.Draw(gameTime);
        }
    }
}
