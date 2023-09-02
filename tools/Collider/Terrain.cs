using FireInTheMole.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static FireInTheMole.Game.Collision;

namespace Collider
{
    public class Terrain
    {
        RigidBody _rigidBody;

        public Terrain(Vector2 position, Vector2 size, Texture2D texture, Color color)
        {
            Texture = texture;
            Color = color;

            _rigidBody = Collision.createRectangle(
                position,
                size,
                Vector2.Zero);
        }

        public Texture2D Texture { get; init; }

        public Color Color { get; init; }

        public RigidBody RigidBody => _rigidBody;

        public void Draw(SpriteBatch spriteBatch)
        {
            var boundingRectangle = ((Bounds.Rectangle)_rigidBody.bounds).Item;
            var position = boundingRectangle.position.ToPoint();
            var size = boundingRectangle.size.ToPoint();

            var destination = new Rectangle(
                location: new Point(position.X - size.X / 2, position.Y - size.Y / 2),
                size: size);

            var centerSize = new Point(5, 5);
            var center = new Rectangle(
                location: new Point(position.X - centerSize.X / 2, position.Y - centerSize.Y / 2),
                size: centerSize);

            spriteBatch.Draw(Texture, destination, Color);
            spriteBatch.Draw(Texture, center, Color.Black);

            Collision.draw(spriteBatch, Texture, RigidBody, Color.Black);
        }
    }
}
