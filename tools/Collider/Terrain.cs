using FireInTheMole.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Collider
{
    public class Terrain
    {

        public Terrain(Vector2 position, Vector2 size, Texture2D texture, Color color)
        {
            Texture = texture;
            Color = color;

            Bounds = Collisions.createBoundingRectangle(position, size);
        }

        public Texture2D Texture { get; init; }

        public Color Color { get; init; }

        public Collisions.BoundingRectangle Bounds { get; private init; }

        public void Draw(SpriteBatch spriteBatch)
        {
            Collisions.drawBoundingRectangle(spriteBatch, Texture, Color, 2, Bounds);
        }
    }
}
