using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FireInTheHole.Renderers;

public static class SpriteBatchExtensions
{
    private static Texture2D _pixel;
    private static Texture2D GetPixel(SpriteBatch spriteBatch)
    {
        if (_pixel == null)
        {
            _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _pixel.SetData(new[] { Color.White });
        }

        return _pixel;
    }

    public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color) => 
        spriteBatch.Draw(GetPixel(spriteBatch), rectangle, color);

    public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 position, int radius, Color color)
    {
        var pixel = GetPixel(spriteBatch);
        for (int y = -radius; y <= radius; y++)
        for (int x = -radius; x <= radius; x++)
        {
            if (x * x + y * y <= radius * radius)
            {
                spriteBatch.Draw(pixel, position + new Vector2(x, y), color);
            }
        }
    }

    public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness, float layerDepth = 0)
    {
        var pixel = GetPixel(spriteBatch);

        var edge = end - start;
        var rotation = MathF.Atan2(edge.Y, edge.X);
        var lineScale = new Vector2(edge.Length(), thickness);

        spriteBatch.Draw(
            _pixel, 
            start, 
            null, 
            color, 
            rotation, 
            Vector2.Zero, 
            lineScale, 
            SpriteEffects.None, 
            layerDepth);
    }
}