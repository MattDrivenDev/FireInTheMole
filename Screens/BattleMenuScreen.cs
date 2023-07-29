namespace FireInTheHole.Screens;

public class BattleMenuScreen : MenuScreen
{
    public BattleMenuScreen(GameEngine engine) : base(engine)
    {
        CreateMenuEntries();
    }

    public GameEngine Engine => base.Game as GameEngine;

    private void CreateMenuEntries()
    {
        MenuEntries.Clear();        

        MenuEntries.Add(new MenuEntry { 
            Text = "Start", 
            Height = 20,
            Action = () => Engine.StartBattle() });

        MenuEntries.Add(new MenuEntry { 
            Text = $"Player Count: {Settings.PlayerCount}", 
            Height = 20,
            Action = TogglePlayerCount });
        
        MenuEntries.Add(new MenuEntry {
            Text = $"Map: {Engine.Map.Name}",
            Height = 20,
            Action = ToggleMap });

        MenuEntries.Add(new MenuEntry { 
            Text = "Exit", 
            Height = 20,
            Action = ExitToMainMenu });
    }

    private void ExitToMainMenu()
    {
        Engine.LoadMainMenuScreen();
    }

    private void ToggleMap()
    {
        Engine.Map.NextMap();
        MenuEntries[2].Text = $"Map: {Engine.Map.Name}";
    }

    private void TogglePlayerCount()
    {
        Settings.PlayerCount++;
        if (Settings.PlayerCount > 4)
            Settings.PlayerCount = 2;

        MenuEntries[1].Text = $"Player Count: {Settings.PlayerCount}";
    }
}