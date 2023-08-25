using FireInTheMole.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace MapViewer
{
    public class MapViewerGame : Game
    {
        const float CameraSpeed = 300f;

        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        TileMap.TileMap _map;
        Texture2D _pixel;
        OrthographicCamera _camera;

        public MapViewerGame()
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

            _camera = new OrthographicCamera(
                new BoxingViewportAdapter(Window, GraphicsDevice,
                    _graphics.PreferredBackBufferWidth,
                    _graphics.PreferredBackBufferHeight));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _pixel = Helpers.createPixelTexture2D(GraphicsDevice);
            _map = TileMap.create(this.Content, "maps/grass/pillars");
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if(Keyboard.GetState().IsKeyDown(Keys.Left)) _camera.Move(new Vector2(-1, 0) * delta * CameraSpeed / _camera.Zoom);
            if(Keyboard.GetState().IsKeyDown(Keys.Right)) _camera.Move(new Vector2(1, 0) * delta * CameraSpeed / _camera.Zoom);
            if(Keyboard.GetState().IsKeyDown(Keys.Up)) _camera.Move(new Vector2(0, -1) * delta * CameraSpeed / _camera.Zoom);
            if(Keyboard.GetState().IsKeyDown(Keys.Down)) _camera.Move(new Vector2(0, 1) * delta * CameraSpeed / _camera.Zoom);
            if(Keyboard.GetState().IsKeyDown(Keys.Add)) _camera.ZoomIn(delta * 1);
            if(Keyboard.GetState().IsKeyDown(Keys.Subtract)) _camera.ZoomOut(delta * 1);
            
            // Interesting....
            _camera.Rotate(0.01f * delta);


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.MintCream);
            var viewMatrix = _camera.GetViewMatrix();
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                transformMatrix: viewMatrix);
            TileMap.draw(_spriteBatch, _pixel, _map);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
