namespace FireInTheHole.Screens;

public class MainMenuScreen : MenuScreen
{
    public MainMenuScreen(GameEngine engine) : base(engine)
    {
        CreateMenuEntries();
    }

    public GameEngine Engine => base.Game as GameEngine;

    private void CreateMenuEntries()
    {
        MenuEntries.Clear();

        MenuEntries.Add(new MenuEntry { 
            Text = "Battle", 
            Height = 20,
            Action = () => Engine.ShowBattleMenu() });

        MenuEntries.Add(new MenuEntry { 
            Text = "Options", 
            Height = 20,
            Action = () => Engine.ShowOptions() });

        MenuEntries.Add(new MenuEntry { 
            Text = "Credits", 
            Height = 20,
            Action = () => Engine.ShowCredits() });

        MenuEntries.Add(new MenuEntry { 
            Text = "Exit", 
            Height = 20,
            Action = () => Engine.Exit() });
    }
}