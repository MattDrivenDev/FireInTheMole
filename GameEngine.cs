using System;
using System.Linq;
using FireInTheHole.Audio;
using FireInTheHole.Objects;
using FireInTheHole.Player;
using FireInTheHole.Renderers;
using FireInTheHole.Screens;
using FireInTheHole.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;

namespace FireInTheHole;

public class GameEngine : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private readonly ScreenManager _screenManager;

    public GameEngine()
    {
        _screenManager = new ScreenManager();
        Components.Add(_screenManager);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / Settings.FramesPerSecond);
        
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = Settings.ScreenWidth;
        _graphics.PreferredBackBufferHeight = Settings.ScreenHeight;
        SetFullScreen(Settings.IsFullScreen);
        _graphics.ApplyChanges();
    }

    public Map Map { get; set; }

    public MapRenderer MapRenderer { get; set; }

    public ProjectionRenderer ProjectionRenderer { get; set; }

    public Mole Player1 { get; set; }

    public Mole Player2 { get; set; }

    public Mole Player3 { get; set; }

    public Mole Player4 { get; set; }

    public SpriteFont GameFont { get; set; }

    public SpriteFont TitleFont { get; set; }

    public SpriteBatch SpriteBatch => _spriteBatch;

    public SfxController SfxController { get; set; }

    public Collider Collider { get; set; }

    protected override void Initialize()
    {        
        base.Initialize();
        LoadIntroScreen();
        Map = new Map(this);
        Collider = new Collider(this);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        GameFont = Content.Load<SpriteFont>("text");
        TitleFont = Content.Load<SpriteFont>("title");
        SfxController = new SfxController(this);
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }

    public void LoadGameplayScreen()
    {
        _screenManager.LoadScreen(
            new GameplayScreen(this),
            new FadeTransition(GraphicsDevice, Color.Black)); 
    }

    public void LoadIntroScreen()
    {
        _screenManager.LoadScreen(
            new IntroScreen(this),
            new FadeTransition(GraphicsDevice, Color.Black));
    }

    public void LoadMainMenuScreen()
    {
        _screenManager.LoadScreen(
            new MainMenuScreen(this),
            new FadeTransition(GraphicsDevice, Color.Black));
        SfxController.PlayMusic();
    }

    public void LoadOptionsMenuScreen()
    {
        _screenManager.LoadScreen(
            new OptionsMenuScreen(this),
            new FadeTransition(GraphicsDevice, Color.Black));        
    }

    public void LoadBattleSummaryScreen(Mole winner)
    {
        _screenManager.LoadScreen(
            new BattleSummaryScreen(this, winner),
            new FadeTransition(GraphicsDevice, Color.Black));
    }

    public void LoadBattleMenu()
    {
        _screenManager.LoadScreen(
            new BattleMenuScreen(this),
            new FadeTransition(GraphicsDevice, Color.Black));
    }

    public void LoadCredits()
    {
        _screenManager.LoadScreen(
            new CreditsScreen(this),
            new FadeTransition(GraphicsDevice, Color.Black));
    }

    public void ExitIntroScreen()
    {
        LoadMainMenuScreen();
    }

    public void ShowBattleMenu()
    {
        LoadBattleMenu();
    }

    public void StartBattle()
    {
        LoadGameplayScreen();
    }

    public void ShowOptions()
    {
        LoadOptionsMenuScreen();
    }

    public void ShowBattleSummaryScreen(Mole winner)
    {
        LoadBattleSummaryScreen(winner);
    }

    public void ShowGameOverScreen()
    {
        LoadMainMenuScreen();
    }

    public void ShowCredits()
    {
        LoadCredits();
    }

    public void SetFullScreen(bool isFullScreen)
    {
        _graphics.IsFullScreen = isFullScreen;
        _graphics.ApplyChanges();
    }
}
