using System.Linq;
using FireInTheHole.Player;
using FireInTheHole.Renderers;
using FireInTheHole.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;

namespace FireInTheHole.Screens;

public class GameplayScreen : GameScreen
{
    public GameplayScreen(GameEngine engine) : base(engine)
    {
        
    }

    public GameEngine Engine => base.Game as GameEngine;

    public override void LoadContent()
    {
        base.LoadContent();
        Restart();
    }

    public override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Engine.Exit();
        }

        // TODO: Add your update logic here

        // Interesting... I wonder how proper games
        // resolve the possibility that the game loop
        // preferences players in a specific order?
        if (Engine.Player1 != null) Engine.Player1.Update(gameTime);
        if (Engine.Player2 != null) Engine.Player2.Update(gameTime);
        if (Engine.Player3 != null) Engine.Player3.Update(gameTime);
        if (Engine.Player4 != null) Engine.Player4.Update(gameTime);

        var players = new[] 
        { 
            Engine.Player1, 
            Engine.Player2, 
            Engine.Player3, 
            Engine.Player4 
        };
        var activePlayers = players.Where(x => x != null);
        var alivePlayers = activePlayers.Where(x => !x.IsDead);

        if (Settings.PlayerCount > 1 && alivePlayers.Count() <= 1)
        {
            var winner = alivePlayers.Single();
            Engine.ShowBattleSummaryScreen(winner);
        }
        else if (Settings.PlayerCount == 1 && alivePlayers.Count() == 0)
        {
            Engine.ShowGameOverScreen();
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Engine.SpriteBatch.Begin();

        if (Settings.UseMapRenderer)
        {
            GraphicsDevice.Clear(Color.LightGray);
            Engine.MapRenderer.Draw(Engine.SpriteBatch);
        }
        else
        {
            GraphicsDevice.Clear(Color.Black);
            Engine.ProjectionRenderer.Draw(Engine.SpriteBatch);
        }

        Engine.SpriteBatch.End();
    }

    private void Restart()
    {        
        Engine.Map.Graves.Clear();
        Engine.Map.Pickups.Clear();
        Engine.MapRenderer = new MapRenderer(Engine, 50);
        Engine.ProjectionRenderer = new ProjectionRenderer(Engine);

        if (Settings.PlayerCount > 0)
        {
            Engine.Player1 = new Mole(
                Engine, 
                Engine.Collider,
                "Player One", 
                PlayerIndex.One,
                Engine.Map.GetStartingPosition(PlayerIndex.One), 
                90, 
                Color.Blue);
        }

        if (Settings.PlayerCount > 1)
        {
            Engine.Player2 = new Mole(
                Engine, 
                Engine.Collider,
                "Player Two", 
                PlayerIndex.Two,
                Engine.Map.GetStartingPosition(PlayerIndex.Two), 
                90, 
                Color.Red);
        }

        if (Settings.PlayerCount > 2)
        {
            Engine.Player3 = new Mole(
                Engine, 
                Engine.Collider,
                "Player Three", 
                PlayerIndex.Three,
                Engine.Map.GetStartingPosition(PlayerIndex.Three), 
                270, 
                Color.Green);
        }
        
        if (Settings.PlayerCount > 3)
        {
            Engine.Player4 = new Mole(
                Engine, 
                Engine.Collider,
                "Player Four", 
                PlayerIndex.Four,
                Engine.Map.GetStartingPosition(PlayerIndex.Four), 
                270, 
                Color.Yellow);
        }
    }
}