using System;
using FireInTheHole.Player;
using FireInTheHole.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FireInTheHole.Objects;

public abstract class Pickup
{
    public static Pickup Maybe(GameEngine engine, Vector2 position)
    {
        var random = new Random();
        var index = random.Next(0, 100);
        return index switch
        {
            < 25 => Spawn(engine, position),
            _ => null
        };
    }

    public static Pickup Spawn(GameEngine engine, Vector2 position)
    {
        var random = new Random();
        var index = random.Next(0, 9);
        return index switch
        {
            < 3 => new DynamiteBuff(engine) { Position = position },
            < 7 => new ExplosionBuff(engine) { Position = position },
            < 10 => new LollerskatesBuff(engine) { Position = position },
            _ => throw new Exception("Invalid index")
        };
    }

    protected readonly GameEngine _engine;

    public Pickup(GameEngine engine)
    {
        _engine = engine;
    }

    public Texture2D Texture { get; init; }

    public Vector2 Position { get; init; }

    public abstract void Apply(Mole player);

    public Renderable? GetSpriteProjection(Mole player)
    {
        var delta = Position - player.Position;
        var theta = MathF.Atan2(delta.Y, delta.X);
        var playerAngleInRadians = MathHelper.ToRadians(player.AngleInDegrees);
        var angleDeltaInRadians = theta - playerAngleInRadians;
        
        // Normalize the angle delta
        if ((delta.X > 0 && playerAngleInRadians > MathF.PI)
            || (delta.X <= 0 && delta.Y <= 0))
        {
            angleDeltaInRadians += MathF.Tau;
        }
        if (angleDeltaInRadians > MathF.PI)
        {
            angleDeltaInRadians -= MathF.Tau;
        }

        var angleDeltaRays = angleDeltaInRadians / MathHelper.ToRadians(Settings.PlayerDeltaFovInDegrees);
        var screenX = (int)(Settings.PlayerHalfRayCount + angleDeltaRays) * Settings.TileScale;
        var distance = MathF.Sqrt(MathF.Pow(delta.X, 2) + MathF.Pow(delta.Y, 2));
        var normalizedDistance = distance * MathF.Cos(angleDeltaInRadians);
        
        // Is it seen?
        var halfWidth = Texture.Width / 2;
        if (-halfWidth < screenX
            && screenX < (Settings.ScreenWidth + halfWidth)
            && normalizedDistance > 0.5f)
        {
            var projection = Settings.PlayerScreenDistance / normalizedDistance * 20;
            var projectionWidth = projection;
            var projectionHeight = projection;
            var halfProjectionWidth = (int)projectionWidth / 2;
            // Figure out a height shift (more of a Y shift)
            var projectionX = screenX - halfProjectionWidth;
            var projectionY = Settings.ScreenHalfHeight - (int)projectionHeight / 2;

            var targetRectangle = new Rectangle(projectionX, projectionY, (int)projectionWidth, (int)projectionHeight);
            var renderable = new Renderable
            {
                Type = RenderableType.Sprite,
                Texture = Texture,
                TargetRectangle = targetRectangle,
                SourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height),
                Color = Color.White,
                Depth = normalizedDistance
            };
            return renderable;
        }
        else
        {
            return null;
        }
    }
}