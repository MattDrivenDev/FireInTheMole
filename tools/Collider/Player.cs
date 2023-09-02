using FireInTheMole.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static FireInTheMole.Game.Collision;

namespace Collider
{
    public class Player
    {
        const int Speed = 500;
        RigidBody _rigidBody;

        public Player(Vector2 position, float radius, Texture2D texture, Color color)
        {
            Radius = radius;
            Color = color;
            Texture = texture;

            _rigidBody = Collision.createCircle(
                position,
                HalfRadius,
                Vector2.Zero);
        }

        public Texture2D Texture { get; init; }

        public float Radius { get; init; }

        public float HalfRadius => Radius / 2;

        public Color Color { get; init; }

        public void Update(GameTime gameTime, IEnumerable<Terrain> terrain)
        {
            var velocity = Vector2.Zero;
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) velocity += new Vector2(-1, 0);
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) velocity += new Vector2(1, 0);
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) velocity += new Vector2(0, -1);
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) velocity += new Vector2(0, 1);
            if (velocity != Vector2.Zero) velocity.Normalize();
            velocity *= (float)gameTime.ElapsedGameTime.TotalSeconds * Speed;

            _rigidBody = Collision.update(_rigidBody, velocity);
            var contacts = Collision.collisions(terrain.Select(x => x.RigidBody), _rigidBody);            
            _rigidBody = Collision.resolve(_rigidBody, contacts);
            _rigidBody = Collision.updatePosition(_rigidBody);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, _rigidBody.position, Color);
            Collision.draw(spriteBatch, Texture, _rigidBody, Color.Black);
        }
    }
}
