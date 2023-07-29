using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;

namespace FireInTheHole.Screens;

public abstract class MenuScreen : GameScreen
{
    private readonly GameEngine _engine;

    public MenuScreen(GameEngine engine) : base(engine)
    {
        _engine = engine;
    }

    public List<MenuEntry> MenuEntries { get; } = new();

    public SpriteFont Font => _engine.GameFont;

    public SpriteBatch SpriteBatch => _engine.SpriteBatch;

    public float PreviousMenuSelection { get; set; }

    public bool CanSelectMenu { get; set; } = true;

    public override void Update(GameTime gameTime)
    {
        ApplyMenuSelectionCooldown(gameTime);
        var ks = Keyboard.GetState();

        if (NoneSelected())
        {
            SelectFirst();
        }
        else
        {
            if (!CanSelectMenu)
            {
                return;
            }

            CanSelectMenu = false;

            if (ks.IsKeyDown(Keys.Up))
            {
                SelectPrevious();
            }

            if (ks.IsKeyDown(Keys.Down))
            {
                SelectNext();
            }

            if (ks.IsKeyDown(Keys.Escape))
            {
                SelectLast();
            }

            if (ks.IsKeyDown(Keys.Enter))
            {
                var selected = MenuEntries.First(x => x.Selected);
                selected.OnSelected(this);
            }
        }
    }

    private bool NoneSelected()
    {
        return MenuEntries.All(x => !x.Selected);
    }

    private void SelectFirst()
    {
        for (var i = 0; i < MenuEntries.Count; i++)
        {
            MenuEntries[i].Selected = i == 0;
        }
    }

    private void SelectLast()
    {
        for (var i = 0; i < MenuEntries.Count; i++)
        {
            MenuEntries[i].Selected = i == MenuEntries.Count - 1;
        }
    }

    private void SelectPrevious()
    {
        var i = MenuEntries.FindIndex(x => x.Selected);
        if (i == 0)
        {
            SelectLast();
        }
        else
        {
            MenuEntries[i].Selected = false;
            MenuEntries[i - 1].Selected = true;
        }
    }

    private void SelectNext()
    {
        var i = MenuEntries.FindIndex(x => x.Selected);
        if (i == MenuEntries.Count - 1)
        {
            SelectFirst();
        }
        else
        {
            MenuEntries[i].Selected = false;
            MenuEntries[i + 1].Selected = true;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        SpriteBatch.Begin();

        for (var i = 0; i < MenuEntries.Count; i++)
        {   
            MenuEntries[i].Draw(this, gameTime, i);
        }

        SpriteBatch.End();
    }

    private void ApplyMenuSelectionCooldown(GameTime gameTime)
    {
        PreviousMenuSelection += (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (PreviousMenuSelection > 0.08f)
        {
            PreviousMenuSelection = 0;
            CanSelectMenu = true;
        }
    }
}