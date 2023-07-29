using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FireInTheHole.Renderers;

public struct Renderable
{
    public RenderableType Type { get; init; }
    public Texture2D Texture { get; init; }
    public Rectangle TargetRectangle { get; init; }
    public Rectangle SourceRectangle { get; init; }
    public Color Color { get; init; }
    public float Depth { get; init; }
    public bool Reverse { get; init; }
}

public enum RenderableType
{
    Texture,
    Sprite
}