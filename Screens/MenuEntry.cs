using System;
using Microsoft.Xna.Framework;

namespace FireInTheHole.Screens;

public class MenuEntry
{
    public string Text { get; set; }
    
    public bool Selected { get; set; }
    
    public int Height { get; set; }
    
    public Action Action { get; set; }

    public virtual void Update(
        MenuScreen menu,
        GameTime gameTime)
    {

    }

    public virtual void Draw(
        MenuScreen menu,
        GameTime gameTime,
        int index)
    {
        var color = Selected ? Color.Yellow : Color.White;
        var offset = Height * index;
        var spriteBatch = menu.SpriteBatch;
        var font = menu.Font;
        spriteBatch.DrawString(
            font,
            Text,
            new Vector2(100, 100 + offset),
            color);
    }

    public virtual void OnSelected(MenuScreen menu)
    {
        Action();
    }
}