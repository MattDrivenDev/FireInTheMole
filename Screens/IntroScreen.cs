using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;

namespace FireInTheHole.Screens;

public class IntroScreen : GameScreen
{
    private readonly GameEngine _engine;

    public IntroScreen(GameEngine engine) : base(engine)
    {
        _engine = engine;
        LoadTextures();
    }

    public Texture2D Logo { get; set; }

    private void LoadTextures()
    {
        Logo = Content.Load<Texture2D>("l_fireinthemole");
    }

    public override void Draw(GameTime gameTime)
    {
        // var titleText = "Fire in the Mole!";
        // var titleFont = _engine.TitleFont;
        // var titleSize = titleFont.MeasureString(titleText);
        // var position = new Vector2(
        //     Settings.ScreenHalfWidth - titleSize.X / 2f,
        //     Settings.ScreenHalfHeight - titleSize.Y / 2f);

        var text = "Press any key to start the party";
        var gameFont = _engine.GameFont;
        var size = gameFont.MeasureString(text);
        var textPosition = new Vector2(
            (Settings.ScreenHalfWidth - size.X / 2f) - 6f,
            (Settings.ScreenHalfHeight - size.Y / 2f) + 20f);
        
        _engine.SpriteBatch.Begin();
        
        // Set the background color
        _engine.GraphicsDevice.Clear(Color.SkyBlue);

        _engine.SpriteBatch.Draw(Logo, new Vector2(Settings.ScreenHalfWidth - Logo.Width / 2, Settings.ScreenHalfHeight - Logo.Height / 2), Color.White);
        //_engine.SpriteBatch.DrawString(gameFont, text, textPosition, Color.Gray);        
        _engine.SpriteBatch.End();
    }

    public override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            _engine.Exit();
        }

        var x = keyboardState.GetPressedKeyCount();
        if (x > 0)
        {
            _engine.ExitIntroScreen();
        }
    }
}