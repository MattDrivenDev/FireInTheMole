using FireInTheMole.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Stantz
{
    public class Player
    {
        const float Speed = 500;
        const float RotationSpeed = 100f;

        Texture2D _pixel;

        public Player(
            RayCasting.RayCaster rayCaster, 
            Vector2 position, 
            Vector2 size,
            Vector2 forward)
        {
            Position = position;
            Size = size;
            Forward = forward;
            RayCaster = rayCaster;
        }

        public Vector2 Position { get; private set; }
        public Vector2 Size { get; private set; }
        public Vector2 Forward { get; private set; }
        public RayCasting.RayCaster RayCaster { get; private set; }

        public void Update(GameTime gameTime, float cameraZoom, TileMap.TileMap map)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var angleInRadians = MathF.Atan2(Forward.Y, Forward.X);
            var angleInDegrees = MathHelper.ToDegrees(angleInRadians);
            var rotation = 0f;
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) rotation -= 1f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) rotation += 1f;
            rotation *= delta * RotationSpeed;
            angleInDegrees += rotation;
            angleInDegrees = Helpers.normAngle(angleInDegrees);

            var velocity = Vector2.Zero;
            if (Keyboard.GetState().IsKeyDown(Keys.W)) velocity += -Vector2.UnitY;  
            if (Keyboard.GetState().IsKeyDown(Keys.S)) velocity += Vector2.UnitY;
            if (Keyboard.GetState().IsKeyDown(Keys.A)) velocity += -Vector2.UnitX;
            if (Keyboard.GetState().IsKeyDown(Keys.D)) velocity += Vector2.UnitX;
            if (velocity != Vector2.Zero) velocity.Normalize();
            velocity *= delta * Speed / cameraZoom;
            Position += velocity;
            angleInRadians = MathHelper.ToRadians(angleInDegrees);
            Forward = new Vector2((float)Math.Cos(angleInRadians), (float)Math.Sin(angleInRadians));

            RayCaster = RayCasting.updateByDirection(RayCaster, map, Position, Forward);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_pixel == null) _pixel = Helpers.createPixelTexture2D(spriteBatch.GraphicsDevice);
            var angleInRadians = MathF.Atan2(Forward.Y, Forward.X);
            var x = MathF.Cos(angleInRadians) * 100;
            var y = MathF.Sin(angleInRadians) * 100;
            var finish = Position + new Vector2(x, y);
            RayCasting.draw(spriteBatch, Color.Yellow, RayCaster);
            Helpers.drawLine(spriteBatch, _pixel, Position, finish, 2, Color.Black);
            Helpers.drawCircle(spriteBatch, Position, Size.X, Color.Red);
        }
    }
}
