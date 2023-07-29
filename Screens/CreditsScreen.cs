using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;

namespace FireInTheHole;

public class CreditsScreen : GameScreen
{
    public CreditsScreen(GameEngine engine) : base(engine)
    {
        
    }

    public GameEngine Engine => base.Game as GameEngine;

    public override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Engine.LoadMainMenuScreen();
        }
    }

    public override void Draw(GameTime gameTime)
    {        
        Engine.SpriteBatch.Begin();
        
        Engine.SpriteBatch.DrawString(
            Engine.TitleFont, 
            "Credits", 
            new Vector2(100, 40), 
            Color.Red);

        Engine.SpriteBatch.DrawString(
            Engine.GameFont, 
            "Game Design by Matt & Andrew Ball", 
            new Vector2(100, 80), 
            Color.White);
            
        Engine.SpriteBatch.DrawString(
            Engine.GameFont, 
            "Programming by Matt Ball", 
            new Vector2(100, 100), 
            Color.White);
        
        Engine.SpriteBatch.DrawString(
            Engine.GameFont, 
            "Map Design by Andrew Ball", 
            new Vector2(100, 120), 
            Color.White);
        
        Engine.SpriteBatch.DrawString(
            Engine.GameFont, 
            "Voice Acting by Andrew Ball", 
            new Vector2(100, 140), 
            Color.White);
        
        Engine.SpriteBatch.DrawString(
            Engine.GameFont, 
            "Graphics & Art by Matt Ball", 
            new Vector2(100, 160), 
            Color.White);
            
        Engine.SpriteBatch.DrawString(
            Engine.GameFont, 
            "Dynamite & Explosion Graphics & Art by Robert Brooks", 
            new Vector2(100, 180), 
            Color.White);

        Engine.SpriteBatch.DrawString(
            Engine.GameFont, 
            "Music by David KBD", 
            new Vector2(100, 200), 
            Color.White);

        Engine.SpriteBatch.End();
    }
}