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
        SpriteFont _font;

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
            _font = Content.Load<SpriteFont>("fonts/text");
            _pixel = Helpers.createPixelTexture2D(GraphicsDevice);
            _map = TileMap.create(this.Content, "maps/snow/bigempty");

            //var mapCenter = new Vector2(_map.widthInPixels / 2, _map.heightInPixels / 2);
            var mapCenter = new Vector2(256+128, 256 + 128);
            var zoom = 0.75f;

            _camera = Cameras.createCamera(
                GraphicsDevice.Viewport,
                mapCenter,
                0f, 
                zoom);

            var options = RayCasting.createOptions(
                fov: 90f,
                count: _graphics.PreferredBackBufferWidth / 2,
                maxLengthInTiles: Math.Max(_map.width, _map.width),
                correctFishEye: false);;

            var angleInDegrees = 0f;
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
            Window.Title = $"FPS: {(1 / gameTime.ElapsedGameTime.TotalSeconds).ToString("0.00")}" +
                $"Angle: {_player.AngleInDegrees}; " +
                $"Zoom: {_camera.zoom}";

            GraphicsDevice.Clear(Color.LightBlue);

            var viewMatrix = Cameras.OrthographicCamera.worldToScreen(_camera);
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                transformMatrix: viewMatrix);
            
            TileMap.drawWithCoords(_spriteBatch, _map, _font);
            _player.Draw(_spriteBatch);

            
            //_spriteBatch.End();


            //_spriteBatch.Begin();
            var zeroDegrees = MathHelper.ToRadians(0f);
            var ninetyDegrees = MathHelper.ToRadians(90f);
            var oneEightyDegrees = MathHelper.ToRadians(180f);
            var twoSeventyDegrees = MathHelper.ToRadians(270f);
            var zeroDegreesVector = new Vector2((float)Math.Cos(zeroDegrees), (float)Math.Sin(zeroDegrees)) * 200;
            var ninetyDegreesVector = new Vector2((float)Math.Cos(ninetyDegrees), (float)Math.Sin(ninetyDegrees)) * 200;
            var oneEightyDegreesVector = new Vector2((float)Math.Cos(oneEightyDegrees), (float)Math.Sin(oneEightyDegrees)) * 200;
            var twoSeventyDegreesVector = new Vector2((float)Math.Cos(twoSeventyDegrees), (float)Math.Sin(twoSeventyDegrees)) * 200;

            Helpers.drawCircle(_spriteBatch, _player.Position + zeroDegreesVector, 10, Color.Red);
            Helpers.drawCircle(_spriteBatch, _player.Position + ninetyDegreesVector, 10, Color.Magenta);
            Helpers.drawCircle(_spriteBatch, _player.Position + oneEightyDegreesVector, 10, Color.DarkCyan);
            Helpers.drawCircle(_spriteBatch, _player.Position + twoSeventyDegreesVector, 10, Color.Blue);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
