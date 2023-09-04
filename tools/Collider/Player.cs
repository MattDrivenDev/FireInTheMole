using FireInTheMole.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Collider
{
    public class Player
    {
        const int Speed = 500;

        public Player(Vector2 position, Vector2 size, Texture2D texture, Color color)
        {
            Color = color;
            Texture = texture;

            Bounds = Collisions.createBoundingRectangle(position, size);
        }

        public Collisions.BoundingRectangle Bounds { get; private set; }

        public Vector2 Position => Bounds.center;

        public Vector2 Size => Bounds.size;

        public Texture2D Texture { get; init; }

        public Color Color { get; init; }

        public List<Collisions.Collision> Contacts { get; private set; } = new List<Collisions.Collision>();

        public void Update(GameTime gameTime, IEnumerable<Terrain> terrain)
        {
            Contacts.Clear();
            var velocity = Vector2.Zero;
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) velocity += new Vector2(-1, 0);
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) velocity += new Vector2(1, 0);
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) velocity += new Vector2(0, -1);
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) velocity += new Vector2(0, 1);
            if (velocity != Vector2.Zero) velocity.Normalize();
            velocity *= (float)gameTime.ElapsedGameTime.TotalSeconds * Speed;

            Bounds = Collisions.updateVelocity(Bounds, velocity);

            Contacts.AddRange(Collisions.predictCollisions(Bounds, terrain.Select(t => t.Bounds)));

            Bounds = Collisions.resolve(Bounds, Contacts);

            Bounds = Collisions.move(Bounds);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Collisions.drawBoundingRectangle(spriteBatch, Texture, Color, 2, Bounds);

            foreach(var contact in Contacts)
            {
                var destination = new Rectangle(
                    location: contact.position.ToPoint(),
                    size: contact.size.ToPoint());

                var color = contact.normal == Vector2.UnitX || contact.normal == -Vector2.UnitX
                    ? Color.Green
                    : Color.Blue;

                spriteBatch.Draw(Texture, destination, color);
            }
        }
    }
}
