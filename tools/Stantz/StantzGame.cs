using FireInTheMole.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Stantz
{
    public class StantzGame : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        TileMap.TileMap _map;
        Texture2D _pixel;
        Cameras.Camera _camera;
        Player _player;

        public StantzGame()
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
            _map = TileMap.create(this.Content, "maps/snow/bigempty");

            var mapCenter = new Vector2(_map.widthInPixels / 2, _map.heightInPixels / 2);
            var angleInDegrees = 90f;
            var zoom = 0.25f;

            _camera = Cameras.createCamera(
                GraphicsDevice.Viewport,
                mapCenter,
                0f, 
                zoom);

            var options = RayCasting.createOptions(
                90f,
                _graphics.PreferredBackBufferWidth / 2,
                Math.Max(_map.widthInPixels, _map.heightInPixels));

            var rayCaster = RayCasting.create(
                options,
                _map,
                mapCenter,
                angleInDegrees);

            var playerSize = new Vector2(20f, 20f);
            _player = new Player(rayCaster, mapCenter, playerSize, angleInDegrees);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var cameraZoom = _camera.zoom;

            if (Keyboard.GetState().IsKeyDown(Keys.Add)) cameraZoom += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.Subtract)) cameraZoom -= 0.01f;

            _player.Update(gameTime, cameraZoom, _map);
            _camera = Cameras.updateCamera(_camera, _player.Position, 0f, cameraZoom);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Window.Title = $"Angle: {_player.AngleInDegrees}";

            GraphicsDevice.Clear(Color.LightBlue);

            var viewMatrix = Cameras.OrthographicCamera.worldToScreen(_camera);
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                transformMatrix: viewMatrix);
            
            TileMap.draw(_spriteBatch, _map);
            _player.Draw(_spriteBatch);
            
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
