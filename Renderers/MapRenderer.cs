using System;
using FireInTheHole.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FireInTheHole.Renderers;

public class MapRenderer
{
    private readonly GameEngine _engine;
    private readonly int _verticalOffset;

    public MapRenderer(GameEngine engine, int verticalOffset = 0)
    {
        _engine = engine;
        _verticalOffset = verticalOffset;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        DrawWorld(spriteBatch, _engine.Map.CurrentMap);

        if (_engine.Player1 != null) DrawPlayer(spriteBatch, _engine.Player1);
        if (_engine.Player2 != null) DrawPlayer(spriteBatch, _engine.Player2);
        if (_engine.Player3 != null) DrawPlayer(spriteBatch, _engine.Player3);
        if (_engine.Player4 != null) DrawPlayer(spriteBatch, _engine.Player4);
    }

    private void DrawPlayer(SpriteBatch spriteBatch, Mole player)
    {
        DrawPlayerPosition(spriteBatch, player);
        DrawPlayerDirection(spriteBatch, player);
        DrawPlayerRayCasting(spriteBatch, player);
        DrawDynamites(spriteBatch, player);
    }

    private void DrawWorld(SpriteBatch spriteBatch, int[,] world)
    {
        var rows = world.GetLength(0);
        var columns = world.GetLength(1);

        for (var y = 0; y < rows; y++)
        for (var x = 0; x < columns; x++)
        {
            if (world[y, x] == 0)
            {
                continue;
            }
            
            if (world[y, x] == 1)
            {
                spriteBatch.DrawRectangle(
                    new Rectangle(x * 50, (y * 50) + _verticalOffset, 50, 50),
                    Color.SandyBrown);
            }

            if (world[y, x] == 2)
            {
                spriteBatch.DrawRectangle(
                    new Rectangle(x * 50, (y * 50) + _verticalOffset, 50, 50),
                    Color.SaddleBrown);
            }            
        }
    }

    private void DrawPlayerPosition(SpriteBatch spriteBatch, Mole player)
    {
        if (player == null)
        {
            return;
        }

        spriteBatch.DrawCircle(
            new Vector2(player.Position.X, player.Position.Y + _verticalOffset), 
            5, player.PlayerColor);
    }

    private void DrawDynamites(SpriteBatch spriteBatch, Mole player)
    {
        for (var i = 0; i < player.Dynamites.Count; i++)
        {
            var dynamite = player.Dynamites[i];

            var x = dynamite.Position.X;
            var y = dynamite.Position.Y + _verticalOffset;

            // Dynamite is 50x50, so we need to offset it by half 
            // of its size so that it doesn't get drawn from the
            // top left corner.
            x -= dynamite.TileTexture.Width / 2;
            y -= dynamite.TileTexture.Height / 2;

            if (dynamite.FuseLit)
            {                
                spriteBatch.Draw(
                    dynamite.TileTexture,
                    new Vector2(x, y),
                    null,
                    Color.White,
                    0,
                    new Vector2(0, 0),
                    1,
                    SpriteEffects.None,
                    0);
            }

            if (dynamite.Exploding)
            {
                spriteBatch.Draw(
                    dynamite.ExplosionTexture,
                    new Vector2(x, y),
                    null,
                    Color.White,
                    0,
                    new Vector2(0, 0),
                    1,
                    SpriteEffects.None,
                    0);

                // This is somewhat of a hack just to render a larger explosion.
                for (var n = 1; n <= dynamite.Size; n++)
                {
                    // Left side
                    x = dynamite.Position.X - (n * 50);
                    y = dynamite.Position.Y + _verticalOffset;
                    x -= dynamite.TileTexture.Width / 2;
                    y -= dynamite.TileTexture.Height / 2;
                    spriteBatch.Draw(
                        dynamite.ExplosionTexture,
                        new Vector2(x, y),
                        null,
                        Color.White,
                        0,
                        new Vector2(0, 0),
                        1,
                        SpriteEffects.None,
                        0);

                    // Left side
                    x = dynamite.Position.X + (n * 50);
                    y = dynamite.Position.Y + _verticalOffset;
                    x -= dynamite.TileTexture.Width / 2;
                    y -= dynamite.TileTexture.Height / 2;
                    spriteBatch.Draw(
                        dynamite.ExplosionTexture,
                        new Vector2(x, y),
                        null,
                        Color.White,
                        0,
                        new Vector2(0, 0),
                        1,
                        SpriteEffects.None,
                        0);                    

                    // Top side
                    x = dynamite.Position.X;
                    y = dynamite.Position.Y + _verticalOffset - (n * 50);
                    x -= dynamite.TileTexture.Width / 2;
                    y -= dynamite.TileTexture.Height / 2;
                    spriteBatch.Draw(
                        dynamite.ExplosionTexture,
                        new Vector2(x, y),
                        null,
                        Color.White,
                        0,
                        new Vector2(0, 0),
                        1,
                        SpriteEffects.None,
                        0);               

                    // Bottom side
                    x = dynamite.Position.X;
                    y = dynamite.Position.Y + _verticalOffset + (n * 50);
                    x -= dynamite.TileTexture.Width / 2;
                    y -= dynamite.TileTexture.Height / 2;
                    spriteBatch.Draw(
                        dynamite.ExplosionTexture,
                        new Vector2(x, y),
                        null,
                        Color.White,
                        0,
                        new Vector2(0, 0),
                        1,
                        SpriteEffects.None,
                        0);
                }
            }
        }
    }

    private void DrawPlayerDirection(SpriteBatch spriteBatch, Mole player)
    {
        var angleInRadians = MathHelper.ToRadians(player.AngleInDegrees);
        var cos = MathF.Cos(angleInRadians);
        var sin = MathF.Sin(angleInRadians);
        var start = new Vector2(player.Position.X, player.Position.Y + _verticalOffset);
        var end = start + new Vector2(cos * 20, sin * 20);
        spriteBatch.DrawLine(start, end, player.PlayerColor, 1);
    }

    private void DrawPlayerRayCasting(SpriteBatch spriteBatch, Mole player)
    {
        for (var i = 0; i < Settings.PlayerRayCount; i++)
        {
            var ray = player.RayCaster.Rays[i];
            var start = new Vector2(player.Position.X, player.Position.Y + _verticalOffset);
            var end = start + new Vector2(ray.Cos * ray.Depth, ray.Sin * ray.Depth);
            spriteBatch.DrawLine(start, end, player.PlayerColor, 1);
        }
    }
}