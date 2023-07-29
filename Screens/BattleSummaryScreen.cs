using FireInTheHole.Player;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Screens;

namespace FireInTheHole.Screens;

public class BattleSummaryScreen : MenuScreen
{
    private readonly Mole _winner;

    public BattleSummaryScreen(
        GameEngine engine,
        Mole winner) : base(engine)
    {
        _winner = winner;
        CreateMenuEntries();
    }

    private void CreateMenuEntries()
    {
        MenuEntries.Clear();

        MenuEntries.Add(new MenuEntry { 
            Text = "Replay", 
            Height = 20,
            Action = () => Engine.StartBattle() });

        MenuEntries.Add(new MenuEntry { 
            Text = "New Battle", 
            Height = 20,
            Action = () => Engine.ShowBattleMenu() });

        MenuEntries.Add(new MenuEntry { 
            Text = "Main Menu", 
            Height = 20,
            Action = ExitToMainMenu });
    }

    public GameEngine Engine => base.Game as GameEngine;

    private void ExitToMainMenu() => Engine.LoadMainMenuScreen();

    public override void Draw(GameTime gameTime)
    {        
        base.Draw(gameTime);

        SpriteBatch.Begin();
        
        SpriteBatch.DrawString(
            Engine.TitleFont, 
            $"{_winner.Name} is the Winner!", 
            new Vector2(100, 40), 
            Color.Red);

        SpriteBatch.End();
    }
}