using FireInTheMole.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Collider
{
    public class ColliderGame : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        Texture2D _pixel;
        Texture2D _circle;
        Player _player;
        List<Terrain> _terrain = new List<Terrain>();

        public ColliderGame()
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
            _pixel = Helpers.createPixelTexture2D(GraphicsDevice);
            _circle = Helpers.createCircleTexture2D(GraphicsDevice, 32);
            _player = new Player(
                position: new Vector2(200, 100),
                size: new Vector2(64, 128),
                texture: _pixel,
                color: Color.Black);

            _terrain.Add(new Terrain(
                position: new Vector2(55, 55),
                size: new Vector2(100, 100),
                texture: _pixel,
                color: Color.Red));

            _terrain.Add(new Terrain(
                position: new Vector2(400, 400),
                size: new Vector2(600, 75),
                texture: _pixel,
                color: Color.Purple));

            _terrain.Add(new Terrain(
                position: new Vector2(600, 200),
                size: new Vector2(100, 300),
                texture: _pixel,
                color: Color.Yellow));

            _terrain.Add(new Terrain(
                position: new Vector2(200, 600),
                size: new Vector2(10, 100),
                texture: _pixel,
                color: Color.Green));

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
            
            _player.Update(gameTime, _terrain);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            _terrain.ForEach(t => t.Draw(_spriteBatch));
            _player.Draw(_spriteBatch);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
