using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;


namespace Trinket
{
    public class TrinketGame : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        SpriteFont _titleFont;
        SpriteFont _textFont;

        string _title = "Trinket";
        Rectangle _titleRectangle = Rectangle.Empty;
        float _titleColorTransitionSpeed = 10f;
        Color _titleStartColor;
        Color _titleEndColor;
        Color _titleColor;
        Color _titleMouseOverColor;
        bool _titleMouseOver = false;

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
            _titleFont = Content.Load<SpriteFont>("fonts/title");
            _textFont = Content.Load<SpriteFont>("fonts/text");

            _titleStartColor = Color.Yellow;
            _titleEndColor = Color.Red;
            _titleMouseOverColor = Color.Red;
            _titleColor = _titleStartColor;
            var titleSize = _titleFont.MeasureString(_title).ToPoint();
            _titleRectangle = new Rectangle(10, 10, titleSize.X, titleSize.Y);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            _titleColor = Color.Lerp(_titleStartColor, _titleEndColor, (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * _titleColorTransitionSpeed));
            _titleMouseOver = _titleRectangle.Contains(Mouse.GetState().Position);
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Window.Title = $"Trinket - FPS: {Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds)}";
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            _spriteBatch.Begin();
            
            // Title "shadow" effect which changes color
            if(!_titleMouseOver) _spriteBatch.DrawString(_titleFont, _title, _titleRectangle.Location.ToVector2(), _titleColor);
            // Title main color
            _spriteBatch.DrawString(_titleFont, _title, _titleRectangle.Location.ToVector2() - new Vector2(1, 1),
                _titleMouseOver ? _titleMouseOverColor : Color.White);            
            
            _spriteBatch.DrawString(_textFont, "Texty text textualizer", new Vector2(10, 50), Color.White);
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
