using System;
using System.Collections.Generic;
using FireInTheHole.Player;
using FireInTheHole.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FireInTheHole.Objects;

public class Dynamite
{
    public const int FrameWidth = 200;
    public const int FrameHeight = 350;
    public const int ExplosionFrameWidth = 512;
    public const int ExplosionFrameHeight = 512;

    private readonly GameEngine _engine;

    public Dynamite(GameEngine engine, Mole owner)
    {
        _engine = engine;
        Owner = owner;
        
        LoadContent();
    }

    public Mole Owner { get; set; }

    public Vector2 Position { get; set; }

    public bool FuseLit { get; set; }

    public int Size { get; set; } = Settings.DynamiteSize;

    public bool Exploding { get; set; }

    public float FuseTime { get; set; } = Settings.DynamiteFuseTime;

    public float ExplosionTime { get; set; } = Settings.DynamiteExplosionTime;

    public Texture2D TileTexture { get; set; }

    public Texture2D ExplosionTexture { get; set; }

    public float OffsetY { get; set; } = 0.3f;

    public Texture2D DynamiteOne { get; set; }

    public Texture2D DynamiteThree { get; set; }

    public Texture2D DynamiteFive { get; set; }

    public Texture2D ExplosionA { get; set; }

    public Texture2D ExplosionB { get; set; }

    public Texture2D NextExplosionTexture { get; set; }

    public LinkedList<Rectangle> FuseAnimationFrames { get; set; } = new();

    public LinkedList<Rectangle> ExplosionAnimationFrames { get; set; } = new();

    public bool Animate { get; set; } 

    public float PreviousAnimationTime { get; set; }

    public float FuseAnimationTime { get; set; }

    public float ExplosionAnimationTime { get; set; }

    public Rectangle CurrentAnimationFrame { get; set; }

    public List<(int, int)> BlastRadius = new();

    private void LoadContent()
    {
        LoadTextures();
        LoadProjectionFrames();
    }

    // Yeah - no. We need to load this more sensibly than this... We're going
    // to have as many textures in memory as we have sticks of dynamite!
    private void LoadTextures()
    {
        TileTexture = _engine.Content.Load<Texture2D>("t_dynamite");
        ExplosionTexture = _engine.Content.Load<Texture2D>("t_explosion");

        ExplosionA = _engine.Content.Load<Texture2D>("Dynamite/explosion-one");
        ExplosionB = _engine.Content.Load<Texture2D>("Dynamite/explosion-two");
        NextExplosionTexture = ExplosionA;

        DynamiteOne = _engine.Content.Load<Texture2D>("Dynamite/one-stick");
        DynamiteThree = _engine.Content.Load<Texture2D>("Dynamite/three-sticks");
        DynamiteFive = _engine.Content.Load<Texture2D>("Dynamite/five-sticks");
    }

    private void LoadProjectionFrames()
    {
        CurrentAnimationFrame = new Rectangle(0, 0, 0, 0);

        // The Dynamite sprite sheets are 1400x350, and there are 7 frames, so each frame is 200x350.
        FuseAnimationFrames.Clear();
        FuseAnimationFrames.AddLast(new Rectangle(FrameWidth * 0, 0, FrameWidth, FrameHeight));
        FuseAnimationFrames.AddLast(new Rectangle(FrameWidth * 1, 0, FrameWidth, FrameHeight));
        FuseAnimationFrames.AddLast(new Rectangle(FrameWidth * 2, 0, FrameWidth, FrameHeight));
        FuseAnimationFrames.AddLast(new Rectangle(FrameWidth * 3, 0, FrameWidth, FrameHeight));
        FuseAnimationFrames.AddLast(new Rectangle(FrameWidth * 4, 0, FrameWidth, FrameHeight));
        FuseAnimationFrames.AddLast(new Rectangle(FrameWidth * 5, 0, FrameWidth, FrameHeight));
        FuseAnimationFrames.AddLast(new Rectangle(FrameWidth * 6, 0, FrameWidth, FrameHeight));
        FuseAnimationTime = FuseTime / FuseAnimationFrames.Count;

        // There are 10 frames in the explosion sprite sheet, each 512x512 - there are two rows of 4
        // and a single row of 2.
        ExplosionAnimationFrames.Clear();
        ExplosionAnimationFrames.AddLast(new Rectangle(ExplosionFrameWidth * 0, ExplosionFrameHeight * 0, ExplosionFrameWidth, ExplosionFrameHeight));
        ExplosionAnimationFrames.AddLast(new Rectangle(ExplosionFrameWidth * 1, ExplosionFrameHeight * 0, ExplosionFrameWidth, ExplosionFrameHeight));
        ExplosionAnimationFrames.AddLast(new Rectangle(ExplosionFrameWidth * 2, ExplosionFrameHeight * 0, ExplosionFrameWidth, ExplosionFrameHeight));
        ExplosionAnimationFrames.AddLast(new Rectangle(ExplosionFrameWidth * 3, ExplosionFrameHeight * 0, ExplosionFrameWidth, ExplosionFrameHeight));
        ExplosionAnimationFrames.AddLast(new Rectangle(ExplosionFrameWidth * 0, ExplosionFrameHeight * 1, ExplosionFrameWidth, ExplosionFrameHeight));
        ExplosionAnimationFrames.AddLast(new Rectangle(ExplosionFrameWidth * 1, ExplosionFrameHeight * 1, ExplosionFrameWidth, ExplosionFrameHeight));
        ExplosionAnimationFrames.AddLast(new Rectangle(ExplosionFrameWidth * 2, ExplosionFrameHeight * 1, ExplosionFrameWidth, ExplosionFrameHeight));
        ExplosionAnimationFrames.AddLast(new Rectangle(ExplosionFrameWidth * 3, ExplosionFrameHeight * 1, ExplosionFrameWidth, ExplosionFrameHeight));
        ExplosionAnimationFrames.AddLast(new Rectangle(ExplosionFrameWidth * 0, ExplosionFrameHeight * 2, ExplosionFrameWidth, ExplosionFrameHeight));
        ExplosionAnimationFrames.AddLast(new Rectangle(ExplosionFrameWidth * 1, ExplosionFrameHeight * 2, ExplosionFrameWidth, ExplosionFrameHeight));
        ExplosionAnimationTime = ExplosionTime / ExplosionAnimationFrames.Count;
    }

    public void Update(GameTime gameTime)
    {        
        if (FuseLit)
        {
            FuseTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateFuseAnimationTime(gameTime);
            UpdateFuseAnimationFrame();

            if (FuseTime <= 0)
            {
                Explode();
            }
        }
        
        if (Exploding)
        {
            ExplosionTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            Owner.Score += KillPlayers();
            UpdateExplosionAnimationTime(gameTime);
            UpdateExplosionAnimationFrame();

            if (ExplosionTime <= 0)
            {
                Reset();
            }
        }
    }    

    private void DoExplosion()
    {
        
    }

    private void UpdateFuseAnimationFrame()
    {
        if (Animate && FuseAnimationFrames.Count > 0)
        {
            var frame = FuseAnimationFrames.First;
            FuseAnimationFrames.RemoveFirst();
            CurrentAnimationFrame = frame.Value;
        }
    }

    private void UpdateExplosionAnimationFrame()
    {
        if (Animate && ExplosionAnimationFrames.Count > 0)
        {
            var frame = ExplosionAnimationFrames.First;
            ExplosionAnimationFrames.RemoveFirst();
            CurrentAnimationFrame = frame.Value;
        }
    }

    private void UpdateFuseAnimationTime(GameTime gameTime)
    {
        Animate = false;
        // Should tick down based on the FuseTime
        PreviousAnimationTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (PreviousAnimationTime > FuseAnimationTime)
        {
            Animate = true;
            PreviousAnimationTime = 0;
        }
    }

    private void UpdateExplosionAnimationTime(GameTime gameTime)
    {
        Animate = false;
        PreviousAnimationTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (PreviousAnimationTime > ExplosionAnimationTime)
        {
            Animate = true;
            PreviousAnimationTime = 0;
        }
    }

    private void Explode()
    {
        _engine.SfxController.PlayExplosion();
        
        NextExplosionTexture = NextExplosionTexture == ExplosionA ? ExplosionB : ExplosionA;

        CurrentAnimationFrame = ExplosionAnimationFrames.First.Value;
        Exploding = true;
        FuseLit = false;
        
        BlastRadius.Clear();
        // Okay we're going to work with map coordinates here.
        var groundZero = _engine.Map.GetMapCoordinates(Position);
        BlastRadius.Add(groundZero.Value);
        var (x, y) = groundZero.Value;
        bool upBlocked = false, downBlocked = false, leftBlocked = false, rightBlocked = false;
        for (int i = 1; i <= Size; i++)
        {
            var up = (x, y - i);
            var down = (x, y + i);
            var left = (x - i, y);
            var right = (x + i, y);

            if (!upBlocked)
            {
                if (_engine.Map.IsWall(up.Item1, up.Item2))
                {
                    upBlocked = true;
                    if (CanDestroyWall(up.Item1, up.Item2))
                    {
                        DestroyWall(up.Item1, up.Item2);
                    }
                }
                else 
                {
                    BlastRadius.Add(up);
                }
            }

            if (!downBlocked)
            {
                if (_engine.Map.IsWall(down.Item1, down.Item2))
                {
                    downBlocked = true;
                    if (CanDestroyWall(down.Item1, down.Item2))
                    {
                        DestroyWall(down.Item1, down.Item2);
                    }
                }
                else 
                {
                    BlastRadius.Add(down);
                }
            }

            if (!leftBlocked)
            {
                if (_engine.Map.IsWall(left.Item1, left.Item2))
                {
                    leftBlocked = true;
                    if (CanDestroyWall(left.Item1, left.Item2))
                    {
                        DestroyWall(left.Item1, left.Item2);
                    }
                }
                else 
                {
                    BlastRadius.Add(left);
                }
            }

            if (!rightBlocked)
            {
                if (_engine.Map.IsWall(right.Item1, right.Item2))
                {
                    rightBlocked = true;
                    if (CanDestroyWall(right.Item1, right.Item2))
                    {
                        DestroyWall(right.Item1, right.Item2);
                    }
                }
                else 
                {
                    BlastRadius.Add(right);
                }
            }
        }
    }

    private int KillPlayers()
    {
        var kills = 0;
        for (var i = 0; i < BlastRadius.Count; i++)
        {
            var (x, y) = BlastRadius[i];
            kills += KillPlayers(x, y);
        }
        return kills;
    }

    private int KillPlayers(int mapX, int mapY)
    {
        return _engine.Map.KillPlayersAt(Owner, mapX, mapY);
    }

    private bool CanDestroyWall(int mapX, int mapY)
    {
        var wall = _engine.Map.GetTileAt(mapX, mapY);
        return wall != null && _engine.Map.CanDestroyWall(wall.Value);
    }

    private void DestroyWall(int mapX, int mapY)
    {
        _engine.Map.DestroyWall(mapX, mapY);
    }

    public void LightFuse(Vector2 position)
    {                
        _engine.SfxController.PlayFuse();

        LoadProjectionFrames();
        CurrentAnimationFrame = FuseAnimationFrames.First.Value;
        Position = position;
        FuseLit = true;
    }

    private void Reset()
    {
        FuseLit = false;
        FuseTime = Settings.DynamiteFuseTime;
        Position = new Vector2(-100, -100);
        ExplosionTime = Settings.DynamiteExplosionTime;
        Exploding = false;
    }

    public Renderable?[] GetSpriteProjection(Mole player)
    {
        if (FuseLit)
        {
            return new [] { GetDynamiteProjection(player) };
        }
        else if (Exploding)
        {
            return GetExplosionProjection(player);
        }
        else
        {
            return null;
        }
    }

    private Texture2D GetDynamiteTexture()
    {
        return Size switch
        {
            >= 5 => DynamiteFive,
            >= 3 => DynamiteThree,
            >= 1 => DynamiteOne,
            _ => throw new Exception("Invalid dynamite size")
        };
    }

    private Renderable? GetDynamiteProjection(Mole player)
    {
        var dynamiteTexture = GetDynamiteTexture();
        var dynamiteRatio = FrameWidth / (float)FrameHeight;
        
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
        var halfWidth = dynamiteTexture.Width / 2;
        if (-halfWidth < screenX
            && screenX < (Settings.ScreenWidth + halfWidth)
            && normalizedDistance > 0.5f)
        {
            var dynamiteProjection = Settings.PlayerScreenDistance / normalizedDistance * 40;
            var dynamiteProjectionWidth = dynamiteProjection * dynamiteRatio;
            var dynamiteProjectionHeight = dynamiteProjection;
            var halfDynamiteProjectionWidth = (int)dynamiteProjectionWidth / 2;
            var offsetY = dynamiteProjectionHeight * OffsetY;
            var dynamiteProjectionX = screenX - halfDynamiteProjectionWidth;
            var dynamiteProjectionY = Settings.ScreenHalfHeight - ((int)dynamiteProjectionHeight / 2) + (int)offsetY;
            var dynamiteTargetRectangle = new Rectangle(dynamiteProjectionX, dynamiteProjectionY, (int)dynamiteProjectionWidth, (int)dynamiteProjectionHeight);            
            var renderable = new Renderable
            {
                Type = RenderableType.Sprite,
                Texture = dynamiteTexture,
                TargetRectangle = dynamiteTargetRectangle,
                SourceRectangle = CurrentAnimationFrame,
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

    private Renderable?[] GetExplosionProjection(Mole player)
    {
        var texture = NextExplosionTexture;
        var renderables = new Renderable?[BlastRadius.Count];

        for (var i = 0; i < BlastRadius.Count; i++)
        {
            var x = (BlastRadius[i].Item1 * 50) + 25;
            var y = (BlastRadius[i].Item2 * 50) + 25;
            var position = new Vector2(x, y);
            var delta = position - player.Position;
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
            var halfWidth = ExplosionFrameWidth / 2;
            if (-halfWidth < screenX
                && screenX < (Settings.ScreenWidth + halfWidth)
                && normalizedDistance > 0.5f)
            {
                var projection = Settings.PlayerScreenDistance / normalizedDistance * 60;
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
                    Texture = texture,
                    TargetRectangle = targetRectangle,
                    SourceRectangle = CurrentAnimationFrame,
                    Color = Color.White,
                    Depth = normalizedDistance
                };

                renderables[i] = renderable;
            }
            else
            {
                renderables[i] = null;
            }
        }

        return renderables;
    }
}