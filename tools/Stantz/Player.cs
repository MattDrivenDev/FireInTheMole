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
            float angleInDegrees)
        {
            Position = position;
            Size = size;
            AngleInDegrees = angleInDegrees;
            RayCaster = rayCaster;
        }

        public Vector2 Position { get; private set; }
        public Vector2 Size { get; private set; }
        public float AngleInDegrees { get; private set; }
        public RayCasting.RayCaster RayCaster { get; private set; }

        public void Update(GameTime gameTime, float cameraZoom, TileMap.TileMap map)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var rotation = 0f;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))   rotation += -1f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))  rotation -= -1f;
            rotation *= delta * RotationSpeed;
            AngleInDegrees += rotation;
            AngleInDegrees = Helpers.normAngle(AngleInDegrees);

            var velocity = Vector2.Zero;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))     velocity += -Vector2.UnitY;  
            if (Keyboard.GetState().IsKeyDown(Keys.Down))   velocity += Vector2.UnitY;   
            if (velocity != Vector2.Zero)                   velocity.Normalize();
            velocity *= delta * Speed / cameraZoom;
            Position += velocity;

            RayCaster = RayCasting.update(RayCaster, map, Position, AngleInDegrees);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_pixel == null) _pixel = Helpers.createPixelTexture2D(spriteBatch.GraphicsDevice);
            var angleInRadians = MathHelper.ToRadians(AngleInDegrees);
            var cos = MathF.Cos(angleInRadians);
            var sin = MathF.Sin(angleInRadians);            
            var finish = Position - new Vector2(sin * 100, cos * 100);
            RayCasting.draw(spriteBatch, Color.Yellow, RayCaster);
            Helpers.drawLine(spriteBatch, _pixel, Position, finish, 2, Color.Black);
            Helpers.drawCircle(spriteBatch, Position, Size.X, Color.Red);
        }
    }
}
