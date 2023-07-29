using System;
using System.Collections.Generic;
using System.Linq;
using FireInTheHole.Objects;
using FireInTheHole.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace FireInTheHole.Player;

public class Mole
{
    public const int FrameWidth = 256;

    public const int FrameHeight = 512;

    private readonly GameEngine _engine;

    public Mole(
        GameEngine engine, 
        Collider collider, 
        string name, 
        PlayerIndex playerIndex,
        Vector2 position, 
        float angleInDegrees, 
        Color color)
    {
        _engine = engine;
        Name = name;
        PlayerIndex = playerIndex;
        Position = position;
        AngleInDegrees = angleInDegrees;
        PlayerColor = color;
        RayCaster = new RayCaster(_engine, this);
        Dynamites.Add(new Dynamite(_engine, this)
        {
            Position = new Vector2(-100, -100),
            FuseLit = false
        });

        Collider = collider;
        Collider.RegisterForCollision(this);

        LoadContent();
    }

    public string Name { get; set; }

    public int Score { get; set; }

    public PlayerIndex PlayerIndex { get; set; }

    public Color PlayerColor { get; set; }

    public Vector2 Position { get; set; }

    public IShapeF Bounds => new CircleF(Position, Settings.PlayerSize);

    public List<Dynamite> Dynamites { get; set; } = new List<Dynamite>();

    public float AngleInDegrees { get; set; }

    public RayCaster RayCaster { get; set; }

    public int RayMaxLength { get; set; } = Settings.PlayerRayMaxLength;

    public Texture2D FrontTexture { get; set; }
    
    public Texture2D BackTexture { get; set; }
    
    public Texture2D SideTexture { get; set; }
    
    public Texture2D SideFrontTexture { get; set; }
    
    public Texture2D SideBackTexture { get; set; }

    public LinkedList<Rectangle> IdleProjectionFrames { get; set; } = new LinkedList<Rectangle>();

    public LinkedList<Rectangle> WalkingProjectionFrames { get; set; } = new LinkedList<Rectangle>();

    public PlayerCharacterAnimation Animation { get; set; } = PlayerCharacterAnimation.Idle;

    public float PreviousAnimationTime { get; set; }

    public float AnimationTime { get; set; } = Settings.PlayerAnimationTime;

    public float PreviousDynamiteDropTime { get; set; }

    public bool CanDropDynamite { get; set; } = true;

    public bool Animate { get; set; }

    public Rectangle CurrentAnimationFrame { get; set; } = new Rectangle(0, 0, FrameWidth, FrameHeight);

    public bool IsDead { get; set; }

    public float SpeedBonus { get; set; } = 1;

    public Collider Collider { get; init; }

    private void LoadContent()
    {
        LoadTextures();
        LoadProjectionFrames();
    }

    private void LoadTextures()
    {
        var color = PlayerIndex switch
        {
            PlayerIndex.One => "yellow",
            PlayerIndex.Two => "blue",
            PlayerIndex.Three => "red",
            PlayerIndex.Four => "green",
            _ => throw new ArgumentOutOfRangeException()
        };

        FrontTexture = _engine.Content.Load<Texture2D>($"Mole/mole_front_{color}");
        BackTexture = _engine.Content.Load<Texture2D>($"Mole/mole_back_{color}");
        SideTexture = _engine.Content.Load<Texture2D>($"Mole/mole_side_{color}");
        SideFrontTexture = _engine.Content.Load<Texture2D>($"Mole/mole_sidefront_{color}");
        SideBackTexture = _engine.Content.Load<Texture2D>($"Mole/mole_sideback_{color}");
    }

    private void LoadProjectionFrames()
    {
        IdleProjectionFrames.AddLast(new Rectangle(FrameWidth * 0, 0, FrameWidth, FrameHeight));

        WalkingProjectionFrames.AddLast(new Rectangle(FrameWidth * 0, 0, FrameWidth, FrameHeight));
        WalkingProjectionFrames.AddLast(new Rectangle(FrameWidth * 1, 0, FrameWidth, FrameHeight));
    }

    public void Update(GameTime gameTime)
    {
        UpdateDynamites(gameTime);
        ApplyDropDynamiteCooldown(gameTime);

        // Player 1 can use the keyboard as well as the gamepad
        if (PlayerIndex == PlayerIndex.One)
        {
            var ks = Keyboard.GetState();
            Move(gameTime, ks);
            Act(gameTime, ks);            
        }

        var gp = GamePad.GetState(PlayerIndex);       
        Move(gameTime, gp);
        Act(gameTime, gp);

        RayCast(gameTime);

        UpdateAnimationTime(gameTime);
        UpdateAnimationFrame();
    }

    private void RayCast(GameTime gameTime)
    {        
        if (IsDead)
        {
            return;
        }

        RayCaster.Update(gameTime);
    }

    private void UpdateAnimationFrame()
    {
        var frames = Animation switch
        {
            PlayerCharacterAnimation.Idle => IdleProjectionFrames,
            PlayerCharacterAnimation.Walk => WalkingProjectionFrames,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (Animate)
        {
            var frame = frames.First;
            frames.RemoveFirst();
            frames.AddLast(frame);
            CurrentAnimationFrame = frame.Value;
        }
    }

    private void UpdateAnimationTime(GameTime gameTime)
    {
        Animate = false;
        PreviousAnimationTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (PreviousAnimationTime > AnimationTime)
        {
            Animate = true;
            PreviousAnimationTime = 0;
        }
    }

    private void UpdateDynamites(GameTime gameTime)
    {
        for (var i = 0; i < Dynamites.Count; i++)
        {
            Dynamites[i].Update(gameTime);
        }
    }

    private void Act(GameTime gameTime, KeyboardState ks)
    {
        if (IsDead)
        {
            return;
        }

        if (ks.IsKeyDown(Keys.RightControl))
        {
            if (CanDropDynamite) DropDynamite(gameTime);
        }
    }

    private void Act(GameTime gameTime, GamePadState gp)
    {
        if (IsDead)
        {
            return;
        }

        if (gp.Buttons.A == ButtonState.Pressed)
        {
            if (CanDropDynamite) DropDynamite(gameTime);
        }
    }

    private void ApplyDropDynamiteCooldown(GameTime gameTime)
    {
        PreviousDynamiteDropTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (PreviousDynamiteDropTime > Settings.PlayerDropDynamiteTime)
        {
            PreviousDynamiteDropTime = 0;
            CanDropDynamite = true;
        }
    }

    private void DropDynamite(GameTime gameTime)
    {        
        if (Dynamites.Any(x => !x.FuseLit && !x.Exploding))
        {
            Vector2 position;
            
            if (Settings.DynamiteOrthoganalExplosions)
            {
                // Drop the dynamite into a position that is
                // in the centre of the player's tile.
                var x = (int)Position.X / 50;
                x = (x * 50) + 25;
                var y = (int)Position.Y / 50;
                y = (y * 50) + 25;
                position = new Vector2(x, y);
            }
            else
            {
                // Drop the dynamite into the player's current position.
                position = Position;
            }

            var dynamite = Dynamites.First(x => !x.FuseLit);
            dynamite.LightFuse(position);
            CanDropDynamite = false;
        }
    }

    private void Move(GameTime gameTime, KeyboardState ks)
    {
        if (IsDead)
        {
            return;
        }

        var rotationSpeed = Settings.PlayerRotationSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        var movementSpeed = Settings.PlayerMoveSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds * SpeedBonus;
        
        Animation = PlayerCharacterAnimation.Idle;

        if (ks.IsKeyDown(Keys.Left))
        {
            RotateLeft(rotationSpeed);
        }

        if (ks.IsKeyDown(Keys.Right))
        {
            RotateRight(rotationSpeed);
        }

        var angleInRadians = MathHelper.ToRadians(AngleInDegrees);
        var sin = MathF.Sin(angleInRadians) * movementSpeed;
        var cos = MathF.Cos(angleInRadians) * movementSpeed;

        if (ks.IsKeyDown(Keys.W))
        {
            Walk(new Vector2(cos, sin));
        }

        if (ks.IsKeyDown(Keys.S))
        {
            Walk(new Vector2(-cos, -sin));
        }

        if (ks.IsKeyDown(Keys.A))
        {
            Walk(new Vector2(sin, -cos));
        }

        if (ks.IsKeyDown(Keys.D))
        {
            Walk(new Vector2(-sin, cos));
        }
    }

    private void Move(GameTime gameTime, GamePadState gp)
    {
        if (IsDead)
        {
            return;
        }

        var rotationSpeed = Settings.PlayerRotationSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        var movementSpeed = Settings.PlayerMoveSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds * SpeedBonus;

        if (gp.ThumbSticks.Right.X < 0)
        {
            RotateLeft(rotationSpeed);
        }

        if (gp.ThumbSticks.Right.X > 0)
        {
            RotateRight(rotationSpeed);
        }

        var angleInRadians = MathHelper.ToRadians(AngleInDegrees);
        var sin = (MathF.Sin(angleInRadians) * movementSpeed);
        var cos = (MathF.Cos(angleInRadians) * movementSpeed);

        if (gp.ThumbSticks.Left.Y > 0)
        {
            Walk(new Vector2(cos, sin));
        }

        if (gp.ThumbSticks.Left.Y < 0)
        {
            Walk(new Vector2(-cos, -sin));
        }

        if (gp.ThumbSticks.Left.X < 0)
        {
            Walk(new Vector2(sin, -cos));
        }

        if (gp.ThumbSticks.Left.X > 0)
        {
            Walk(new Vector2(-sin, cos));
        }
    }

    private void Walk(Vector2 delta)
    {
        Animation = PlayerCharacterAnimation.Walk;

        // TODO: There is still a bug here where the player can 
        // walk into a wall when they travel at a fairly narrow
        // angle to the wall.
        var deltaWithSize = delta * Settings.PlayerScale;
        if (CanWalk(Position + deltaWithSize))
        {            
            Position += delta;
            if (_engine.Map.IsPickupAt(Position, out var pickup))
            {
                Apply(pickup);
                RemoveFromMap(pickup);
            }
        }
    }

    private void Apply(Pickup pickup)
    {
        pickup.Apply(this);
    }

    public void RemoveFromMap(Pickup pickup)
    {
        _engine.Map.RemovePickup(pickup);
    }

    private bool CanWalk(Vector2 position)
    {
        var tile = _engine.Map.GetTileAt(position);
        
        var newBounds = new CircleF(position, Settings.PlayerSize);

        return tile != 1 && tile != 2 && !Collider.HasCollision(this, newBounds);
    }

    private void RotateLeft(float rotationSpeed)
    {
        AngleInDegrees -= rotationSpeed;
        NormalizeAngle();
    }

    private void RotateRight(float rotationSpeed)
    {
        AngleInDegrees += rotationSpeed;
        NormalizeAngle();
    }

    private void NormalizeAngle()
    {
        if (AngleInDegrees > 360)
        {
            AngleInDegrees -= 360;
        }

        if (AngleInDegrees < 0)
        {
            AngleInDegrees += 360;
        }
    }

    public Renderable? GetSpriteProjection(Mole player)
    {
        if (IsDead)
        {
            return null;
        }

        var delta = Position - player.Position;
        var theta = MathF.Atan2(delta.Y, delta.X);
        var playerAngleInRadians = MathHelper.ToRadians(player.AngleInDegrees);
        var angleDeltaInRadians = theta - playerAngleInRadians;

        var myAngleInRadians = MathHelper.ToRadians(AngleInDegrees);
        var theirAngleInRadians = MathHelper.ToRadians(player.AngleInDegrees);
        var diffAngleInRadians = myAngleInRadians - theirAngleInRadians;
        if (diffAngleInRadians > MathF.Tau)
        {
            diffAngleInRadians -= MathF.Tau;
        }
        if (diffAngleInRadians < 0)
        {
            diffAngleInRadians += MathF.Tau;
        }

        // There are 8 possible textures to use for the sprite, when 
        // the diffAngleInRadians is pi then they are at 180 degrees,
        // so they will be face to face, so we can use the front texture.
        var tauOver8 = MathF.Tau / 8;
        var tauOver16 = MathF.Tau / 16;
        var piOver2 = MathF.PI / 2;

        var texture = diffAngleInRadians switch
        {
            var x when x > MathF.PI - tauOver16 && x <= MathF.PI + tauOver16 => FrontTexture,
            var x when x > tauOver8 - tauOver16 && x <= tauOver8 + tauOver16 => SideBackTexture,
            var x when x > piOver2 - tauOver16 && x <= piOver2 + tauOver16 => SideTexture,
            var x when x > (tauOver8 * 3) - tauOver16 && x <= (tauOver8 * 3) + tauOver16 => SideFrontTexture,
            var x when x > MathF.PI - tauOver16 && x <= MathF.PI + tauOver16 => BackTexture,
            var x when x > (tauOver8 * 5) - tauOver16 && x <= (tauOver8 * 5) + tauOver16 => SideFrontTexture,
            var x when x > (tauOver8 * 6) - tauOver16 && x <= (tauOver8 * 6) + tauOver16 => SideTexture,
            var x when x > (tauOver8 * 7) - tauOver16 && x <= (tauOver8 * 7) + tauOver16 => SideBackTexture,
            _ => BackTexture
        };

        // The side-facing textures need to be flipped depending on whether
        // they are facing left or right (relatively speaking).
        var reverse = diffAngleInRadians > (tauOver8 * 5) - tauOver16 
            && diffAngleInRadians <= (tauOver8 * 7) + tauOver16;
        
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
        var halfWidth = FrameWidth / 2;
        if (-halfWidth < screenX
            && screenX < (Settings.ScreenWidth + halfWidth)
            && normalizedDistance > 0.5f)
        {
            var projection = Settings.PlayerScreenDistance / normalizedDistance * 40;
            var projectionWidth = projection;
            var projectionHeight = projection;
            var halfProjectionWidth = (int)projectionWidth / 2;
            var heightShift = 25;
            var projectionX = screenX - halfProjectionWidth;
            var projectionY = Settings.ScreenHalfHeight - (int)projectionHeight / 2 + heightShift; 

            var targetRectangle = new Rectangle(projectionX, projectionY, (int)projectionWidth, (int)projectionHeight);
            var renderable = new Renderable
            {
                Type = RenderableType.Sprite,
                Texture = texture,
                TargetRectangle = targetRectangle,
                SourceRectangle = CurrentAnimationFrame,
                Color = Color.White,
                Depth = normalizedDistance,
                Reverse = reverse
            };
            return renderable;
        }
        else
        {
            return null;
        }
    }

    public void Kill()
    {
        IsDead = true;
        _engine.Map.AddGrave(this);
    }

    public enum PlayerCharacterAnimation
    {
        Idle,
        Walk
    }
}