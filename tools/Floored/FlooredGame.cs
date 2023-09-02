using FireInTheMole.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using static FireInTheMole.Game.Cameras;

namespace MapViewer
{
    public class FlooredGame : Game
    {
        const float CameraSpeed = 500f;
        const float CameraRotationSpeed = 100f;

        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        TileMap.TileMap _map;
        Texture2D _pixel;
        Camera _camera;
        Mode7Camera.Frustum _frustum;

        public FlooredGame()
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
            _map = TileMap.create(this.Content, "maps/grass/pillars");

            _camera = Cameras.createCamera(
                GraphicsDevice.Viewport,
                new Vector2(_map.widthInPixels / 2, _map.heightInPixels / 2),
                0f,
                0.5f);

            _frustum = Mode7Camera.createFrustum(
                new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                new Vector2(0, GraphicsDevice.Viewport.Height / 2),
                new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height / 2),
                new Vector2(-GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                new Vector2(GraphicsDevice.Viewport.Width * 2, GraphicsDevice.Viewport.Height));

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Add some basic controls to zoom/rotate/move the camera
            var rotationDelta = 0f;
            var zoomDelta = 0f;
            if (Keyboard.GetState().IsKeyDown(Keys.Subtract)) zoomDelta += delta * 1;
            if (Keyboard.GetState().IsKeyDown(Keys.Add)) zoomDelta -= delta * 1;
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) rotationDelta += 0.01f * CameraRotationSpeed * delta;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) rotationDelta -= 0.01f * CameraRotationSpeed * delta;
            var zoom = _camera.zoom + zoomDelta;
            var rotation = _camera.rotation + rotationDelta;

            // Use the rotation to calculate the direction we want to move in with sin and cos
            var sin = MathF.Sin(-rotation);
            var cos = MathF.Cos(-rotation);
            var positionDelta = Vector2.Zero;

            // Move the camera in the direction of the rotation
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                positionDelta.X += sin * CameraSpeed * delta;
                positionDelta.Y += -cos * CameraSpeed * delta;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                positionDelta.X += -sin * CameraSpeed * delta;
                positionDelta.Y += cos * CameraSpeed * delta;
            }

            var position = _camera.position + positionDelta;

            _camera = Cameras.updateCamera(_camera, position, rotation, zoom);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Window.Title = $"Floored - FPS: {Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds)}";

            GraphicsDevice.Clear(Color.MintCream);
            
            var viewMatrix = Mode7Camera.worldToScreen(_frustum, _camera);
            
            _spriteBatch.Begin(transformMatrix: viewMatrix);

            TileMap.draw(_spriteBatch, _map);

            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
