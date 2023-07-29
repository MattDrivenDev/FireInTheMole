namespace FireInTheHole.Screens;

public class OptionsMenuScreen : MenuScreen
{
    public OptionsMenuScreen(GameEngine engine) : base(engine)
    {
        CreateMenuEntries();
    }

    public GameEngine Engine => base.Game as GameEngine;

    private void CreateMenuEntries()
    {
        MenuEntries.Clear();        

        MenuEntries.Add(new MenuEntry { 
            Text = $"Fullscreen: {(Settings.IsFullScreen ? "On" : "Off")}", 
            Height = 20,
            Action = ToggleFullscreen });

        MenuEntries.Add(new MenuEntry { 
            Text = "Main Menu", 
            Height = 20,
            Action = ExitToMainMenu });
    }

    private void ExitToMainMenu()
    {
        Engine.LoadMainMenuScreen();
    }

    private void ToggleFullscreen()
    {
        Settings.IsFullScreen = !Settings.IsFullScreen;
        Engine.SetFullScreen(Settings.IsFullScreen);

        MenuEntries[0].Text = $"Fullscreen: {(Settings.IsFullScreen ? "On" : "Off")}";
    }
}