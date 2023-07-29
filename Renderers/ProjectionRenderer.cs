using System;
using System.Collections.Generic;
using System.Linq;
using FireInTheHole.Player;
using FireInTheHole.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FireInTheHole;

public class ProjectionRenderer
{
    private readonly GameEngine _engine;

    public ProjectionRenderer(GameEngine engine)
    {
        _engine = engine;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        switch (Settings.PlayerCount)
        {
            case 4: RenderFourPlayerSplit(spriteBatch); break;
            case 3: RenderThreePlayerSplit(spriteBatch); break;
            case 2: RenderTwoPlayerSplit(spriteBatch); break;
            
            default:
            case 1: RenderSinglePlayerSplit(spriteBatch); break;
        }
    }

    private void RenderSinglePlayerSplit(SpriteBatch spriteBatch)
    {
        RenderFullScreen(spriteBatch, _engine.Player1);
    }

    private void RenderTwoPlayerSplit(SpriteBatch spriteBatch)
    {
        RenderTop(spriteBatch, _engine.Player1);
        RenderBottom(spriteBatch, _engine.Player2);        
        DrawHorizontalSplitLine(spriteBatch);
    }

    private void RenderThreePlayerSplit(SpriteBatch spriteBatch)
    {
        RenderTopLeft(spriteBatch, _engine.Player1);
        RenderTopRight(spriteBatch, _engine.Player2);
        RenderBottomLeft(spriteBatch, _engine.Player3);
        DrawHorizontalSplitLine(spriteBatch);
        DrawVerticalSplitLine(spriteBatch);
    }

    private void RenderFourPlayerSplit(SpriteBatch spriteBatch)
    {
        RenderTopLeft(spriteBatch, _engine.Player1);
        RenderTopRight(spriteBatch, _engine.Player2);
        RenderBottomLeft(spriteBatch, _engine.Player3);
        RenderBottomRight(spriteBatch, _engine.Player4);
        DrawHorizontalSplitLine(spriteBatch);
        DrawVerticalSplitLine(spriteBatch);
    }

    private void RenderTopLeft(SpriteBatch spriteBatch, Mole player)
    {
        if (!player.IsDead)
        {
            // Render a sky as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(0, 0, Settings.ScreenHalfWidth, Settings.ScreenHalfHeight / 2),
                _engine.Map.SkyColor);

            // Render a floor as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(0, Settings.ScreenHalfHeight / 2, Settings.ScreenHalfWidth, Settings.ScreenHalfHeight / 2),
                _engine.Map.FloorColor);

            var buffer = BufferPlayerFov(player);

            for (var i = 0; i < buffer.Length; i++)
            {
                var renderable = buffer[i];
                var target = renderable.TargetRectangle;
                var source = renderable.SourceRectangle;

                target.Width /= 2;
                target.Height /= 2;
                target.X /= 2;
                target.Y /= 2;

                var scale = (float)source.Height / (float)target.Height;
                var diffY = 0 - target.Y;
                var scaledDiffY = diffY * scale;

                if (target.Y < 0)
                {
                    target.Y = 0;
                    target.Height = Settings.ScreenHalfHeight;    
                    source.Y = (int)scaledDiffY;
                    source.Height -= (int)scaledDiffY * 2;
                }

                if (renderable.Type == RenderableType.Sprite)
                {
                    // Is this out of bounds of what we're drawing for this player?
                    if ((target.X + target.Width < 0 || target.X > Settings.ScreenHalfWidth)
                        || (target.Y + target.Height < 0 || target.Y > Settings.ScreenHalfHeight))
                    {
                        continue;
                    }

                    // Okay, how about partially out of bounds?
                    if (target.X < 0)
                    {
                        var diffX = 0 - target.X;
                        var scaledDiffX = diffX * scale;
                        target.X = 0;
                        target.Width = target.Width - (int)scaledDiffX;
                        source.X = (int)scaledDiffX;
                        source.Width -= (int)scaledDiffX;
                    }
                }

                spriteBatch.Draw(
                    buffer[i].Texture,
                    target,
                    source,
                    buffer[i].Color,
                    0f,
                    Vector2.Zero,
                    renderable.Reverse ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    1f);
            }
        }

        var score = $"Score: {player.Score}";
        var size = _engine.GameFont.MeasureString(score);
        var position = new Vector2(10, 10);
        spriteBatch.DrawString(
            _engine.GameFont,
            score,
            position,
            Color.White);
    }

    private void RenderTopRight(SpriteBatch spriteBatch, Mole player)
    {
        if (!player.IsDead)
        {
            // Render a sky as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(Settings.ScreenHalfWidth, 0, Settings.ScreenHalfWidth, Settings.ScreenHalfHeight / 2),
                _engine.Map.SkyColor);

            // Render a floor as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(Settings.ScreenHalfWidth, Settings.ScreenHalfHeight / 2, Settings.ScreenHalfWidth, Settings.ScreenHalfHeight / 2),
                _engine.Map.FloorColor);

            var buffer = BufferPlayerFov(player);

            for (var i = 0; i < buffer.Length; i++)
            {
                var renderable = buffer[i];
                var target = renderable.TargetRectangle;
                var source = renderable.SourceRectangle;

                target.Width /= 2;
                target.Height /= 2;
                target.X /= 2;
                target.Y /= 2;
                target.X += Settings.ScreenHalfWidth;

                var scale = (float)source.Height / (float)target.Height;
                var diffY = 0 - target.Y;
                var scaledDiffY = diffY * scale;

                if (target.Y < 0)
                {
                    target.Y = 0;
                    target.Height = Settings.ScreenHalfHeight;    
                    source.Y = (int)scaledDiffY;
                    source.Height -= (int)scaledDiffY * 2;
                }

                if (renderable.Type == RenderableType.Sprite)
                {
                    // Is this out of bounds of what we're drawing for this player?
                    if ((target.X + target.Width < Settings.ScreenHalfWidth || target.X > Settings.ScreenWidth)
                        || (target.Y + target.Height < 0 || target.Y > Settings.ScreenHalfHeight))
                    {
                        continue;
                    }

                    // Okay, how about partially out of bounds?
                    if (target.X < Settings.ScreenHalfWidth)
                    {
                        var diffX = Settings.ScreenHalfWidth - target.X;
                        var scaledDiffX = diffX * scale;
                        target.X = Settings.ScreenHalfWidth;
                        target.Width = target.Width - (int)scaledDiffX;
                        source.X = (int)scaledDiffX;
                        source.Width -= (int)scaledDiffX;
                    }
                }

                spriteBatch.Draw(
                    buffer[i].Texture,
                    target,
                    source,
                    buffer[i].Color,
                    0f,
                    Vector2.Zero,
                    renderable.Reverse ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    1f);
            }
        }

        var score = $"Score: {player.Score}";
        var size = _engine.GameFont.MeasureString(score);
        var position = new Vector2(Settings.ScreenHalfWidth + 10, 10);
        spriteBatch.DrawString(
            _engine.GameFont,
            score,
            position,
            Color.White);
    }

    private void RenderBottomLeft(SpriteBatch spriteBatch, Mole player)
    {
        if (!player.IsDead)
        {
            // Render a sky as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(0, Settings.ScreenHalfHeight, Settings.ScreenHalfWidth, Settings.ScreenHalfHeight / 2),
                _engine.Map.SkyColor);

            // Render a floor as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(0, (Settings.ScreenHalfHeight + Settings.ScreenHalfHeight / 2), Settings.ScreenHalfWidth, Settings.ScreenHalfHeight / 2),
                _engine.Map.FloorColor);

            var buffer = BufferPlayerFov(player);

            for (var i = 0; i < buffer.Length; i++)
            {
                var renderable = buffer[i];
                var target = renderable.TargetRectangle;
                var source = renderable.SourceRectangle;

                target.Width /= 2;
                target.Height /= 2; 
                target.X /= 2;
                target.Y /= 2;
                target.Y += Settings.ScreenHalfHeight;

                var scale = (float)source.Height / (float)target.Height;
                var diffY = Settings.ScreenHalfHeight - target.Y;
                var scaledDiffY = diffY * scale;

                if (target.Y < Settings.ScreenHalfHeight)
                {
                    target.Y = Settings.ScreenHalfHeight;
                    target.Height = Settings.ScreenHalfHeight;    
                    source.Y = (int)scaledDiffY;
                    source.Height -= (int)scaledDiffY * 2;
                }

                if (renderable.Type == RenderableType.Sprite)
                {
                    // Is this out of bounds of what we're drawing for this player?
                    if ((target.X + target.Width < 0 || target.X > Settings.ScreenHalfWidth)
                        || (target.Y + target.Height < Settings.ScreenHalfHeight || target.Y > Settings.ScreenHeight))
                    {
                        continue;
                    }

                    // Okay, how about partially out of bounds?
                    if (target.X < 0)
                    {
                        var diffX = 0 - target.X;
                        var scaledDiffX = diffX * scale;
                        target.X = 0;
                        target.Width = target.Width - (int)scaledDiffX;
                        source.X = (int)scaledDiffX;
                        source.Width -= (int)scaledDiffX;
                    }
                }
                
                spriteBatch.Draw(
                    buffer[i].Texture,
                    target,
                    source,
                    buffer[i].Color,
                    0f,
                    Vector2.Zero,
                    renderable.Reverse ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    1f);
            }
        }

        var score = $"Score: {player.Score}";
        var size = _engine.GameFont.MeasureString(score);
        var position = new Vector2(10, Settings.ScreenHalfHeight + 10);
        spriteBatch.DrawString(
            _engine.GameFont,
            score,
            position,
            Color.White);
    }

    private void RenderBottomRight(SpriteBatch spriteBatch, Mole player)
    {                
        if (!player.IsDead)
        {
            // Render a sky as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(Settings.ScreenHalfWidth, Settings.ScreenHalfHeight, Settings.ScreenHalfWidth, Settings.ScreenHalfHeight / 2),
                _engine.Map.SkyColor);

            // Render a floor as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(Settings.ScreenHalfWidth, (Settings.ScreenHalfHeight + Settings.ScreenHalfHeight / 2), Settings.ScreenHalfWidth, Settings.ScreenHalfHeight / 2),
                _engine.Map.FloorColor);

            var buffer = BufferPlayerFov(player);

            for (var i = 0; i < buffer.Length; i++)
            {
                var renderable = buffer[i];
                var target = renderable.TargetRectangle;
                var source = renderable.SourceRectangle;

                target.Width /= 2;
                target.Height /= 2; 
                target.X /= 2;
                target.X += Settings.ScreenHalfWidth;
                target.Y /= 2;
                target.Y += Settings.ScreenHalfHeight;

                var scale = (float)source.Height / (float)target.Height;
                var diffY = Settings.ScreenHalfHeight - target.Y;
                var scaledDiffY = diffY * scale;

                if (target.Y < Settings.ScreenHalfHeight)
                {
                    target.Y = Settings.ScreenHalfHeight;
                    target.Height = Settings.ScreenHalfHeight;    
                    source.Y = (int)scaledDiffY;
                    source.Height -= (int)scaledDiffY * 2;
                }

                if (renderable.Type == RenderableType.Sprite)
                {
                    // Is this out of bounds of what we're drawing for this player?
                    if ((target.X + target.Width < Settings.ScreenHalfWidth || target.X > Settings.ScreenWidth)
                        || (target.Y + target.Height < Settings.ScreenHalfHeight || target.Y > Settings.ScreenHeight))
                    {
                        continue;
                    }

                    // Okay, how about partially out of bounds?
                    if (target.X < Settings.ScreenHalfWidth)
                    {
                        var diffX = Settings.ScreenHalfWidth - target.X;
                        var scaledDiffX = diffX * scale;
                        target.X = Settings.ScreenHalfWidth;
                        target.Width = target.Width - (int)scaledDiffX;
                        source.X = (int)scaledDiffX;
                        source.Width -= (int)scaledDiffX;
                    }
                }
                
                spriteBatch.Draw(
                    buffer[i].Texture,
                    target,
                    source,
                    buffer[i].Color,
                    0f,
                    Vector2.Zero,
                    renderable.Reverse ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    1f);
            }
        }

        var score = $"Score: {player.Score}";
        var size = _engine.GameFont.MeasureString(score);
        var position = new Vector2(Settings.ScreenHalfWidth + 10, Settings.ScreenHalfHeight + 10);
        spriteBatch.DrawString(
            _engine.GameFont,
            score,
            position,
            Color.White);
    }

    private void RenderTop(SpriteBatch spriteBatch, Mole player)
    {
        if (!player.IsDead)
        {
            // Render a sky as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(0, 0, Settings.ScreenWidth, Settings.ScreenHalfHeight / 2),
                _engine.Map.SkyColor);

            // Render a floor as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(0, Settings.ScreenHalfHeight / 2, Settings.ScreenWidth, Settings.ScreenHalfHeight / 2),
                _engine.Map.FloorColor);

            var buffer = BufferPlayerFov(player);

            for (var i = 0; i < buffer.Length; i++)
            {
                var renderable = buffer[i];
                var target = renderable.TargetRectangle;
                var source = renderable.SourceRectangle;

                target.Height /= 2;
                target.Y /= 2;

                var scale = (float)source.Height / (float)target.Height;
                var diffY = 0 - target.Y;
                var scaledDiffY = diffY * scale;

                if (target.Y < 0)
                {
                    target.Y = 0;
                    target.Height = Settings.ScreenHalfHeight;    
                    source.Y = (int)scaledDiffY;
                    source.Height -= (int)scaledDiffY * 2;
                }

                spriteBatch.Draw(
                    buffer[i].Texture,
                    target,
                    source,
                    buffer[i].Color,
                    0f,
                    Vector2.Zero,
                    renderable.Reverse ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    1f);
            }
        }

        var score = $"Score: {player.Score}";
        var size = _engine.GameFont.MeasureString(score);
        var position = new Vector2(10, 10);
        spriteBatch.DrawString(
            _engine.GameFont,
            score,
            position,
            Color.White);
    }

    private void RenderBottom(SpriteBatch spriteBatch, Mole player)
    {
        if (!player.IsDead)
        {
            // Render a sky as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(0, Settings.ScreenHalfHeight, Settings.ScreenWidth, Settings.ScreenHalfHeight / 2),
                _engine.Map.SkyColor);

            // Render a floor as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(0, (Settings.ScreenHalfHeight + Settings.ScreenHalfHeight / 2), Settings.ScreenWidth, Settings.ScreenHalfHeight / 2),
                _engine.Map.FloorColor);
                
            var buffer = BufferPlayerFov(player);

            for (var i = 0; i < buffer.Length; i++)
            {
                var renderable = buffer[i];
                var target = renderable.TargetRectangle;
                var source = renderable.SourceRectangle;

                target.Height /= 2;
                target.Y /= 2;
                target.Y += Settings.ScreenHalfHeight;

                var scale = (float)source.Height / (float)target.Height;
                var diffY = Settings.ScreenHalfHeight - target.Y;
                var scaledDiffY = diffY * scale;

                if (target.Y < Settings.ScreenHalfHeight)
                {
                    target.Y = Settings.ScreenHalfHeight;
                    target.Height = Settings.ScreenHalfHeight;    
                    source.Y = (int)scaledDiffY;
                    source.Height -= (int)scaledDiffY * 2;
                }
                
                spriteBatch.Draw(
                    buffer[i].Texture,
                    target,
                    source,
                    buffer[i].Color,
                    0f,
                    Vector2.Zero,
                    renderable.Reverse ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    1f);
            }
        }

        var score = $"Score: {player.Score}";
        var size = _engine.GameFont.MeasureString(score);
        var position = new Vector2(10, Settings.ScreenHalfHeight + 10);
        spriteBatch.DrawString(
            _engine.GameFont,
            score,
            position,
            Color.White);
    }

    private void RenderFullScreen(SpriteBatch spriteBatch, Mole player)
    {
        if (!player.IsDead)
        {
            // Render a sky as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(0, 0, Settings.ScreenWidth, Settings.ScreenHalfHeight),
                _engine.Map.SkyColor);

            // Render a floor as a solid color
            spriteBatch.DrawRectangle(
                new Rectangle(0, Settings.ScreenHalfHeight, Settings.ScreenWidth, Settings.ScreenHalfHeight),
                _engine.Map.FloorColor);

            var buffer = BufferPlayerFov(player);

            for (var i = 0; i < buffer.Length; i++)
            {
                var renderable = buffer[i];
                spriteBatch.Draw(
                    buffer[i].Texture,
                    buffer[i].TargetRectangle,
                    buffer[i].SourceRectangle,
                    buffer[i].Color,
                    0f,
                    Vector2.Zero,
                    renderable.Reverse ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    1f);
            }
        }

        var score = $"Score: {player.Score}";
        var size = _engine.GameFont.MeasureString(score);
        var position = new Vector2(10, 10);
        spriteBatch.DrawString(
            _engine.GameFont,
            score,
            position,
            Color.White);
    }
    
    private Renderable[] BufferPlayerFov(Mole player)
    {
        var buffer = new List<Renderable>();

        if (player.IsDead)
        {
            return buffer.ToArray();
        }

        for (var i = 0; i < player.RayCaster.Rays.Length; i++)
        {
            var ray = player.RayCaster.Rays[i];
            var rayDepthInTiles = ray.Depth / 50;
            var projectionHeight = Settings.PlayerScreenDistance / (rayDepthInTiles + 0.0001f);
            var color = Color.White;
            var offset = ray.TextureOffset;
            var texture = ray.Tile == 1 ?_engine.Map.WallTexture1 : _engine.Map.WallTexture2;

            var target = new Rectangle(
                i * Settings.TileScale, 
                (int)(Settings.ScreenHalfHeight - projectionHeight / 2), 
                Settings.TileScale, 
                (int)projectionHeight);

            var source = new Rectangle(
                (int)(ray.TextureOffset * (256 - Settings.TileScale)),
                0, 
                Settings.TileScale,
                256);

            buffer.Add(new Renderable
            {
                Type = RenderableType.Texture,
                Texture = texture,
                TargetRectangle = target,
                SourceRectangle = source,
                Color = color,
                Depth = ray.Depth
            });
        }

        var players = (new [] { _engine.Player1, _engine.Player2, _engine.Player3, _engine.Player4 }).Where(x => x != null).ToArray();
        var dynamites = players.SelectMany(x => x.Dynamites).Where(x => x.FuseLit).ToArray();
        var maybeRenderables = dynamites.SelectMany(x => x.GetSpriteProjection(player)).ToArray();
        var renderables = maybeRenderables.Where(x => x.HasValue).Select(x => x.Value).ToArray();
        buffer.AddRange(renderables);
        
        var explosions = players.SelectMany(x => x.Dynamites).Where(x => x.Exploding).ToArray();
        var maybeExplosionRenderables = explosions.SelectMany(x => x.GetSpriteProjection(player)).ToArray();
        var explosionRenderables = maybeExplosionRenderables.Where(x => x.HasValue).Select(x => x.Value).ToArray();
        buffer.AddRange(explosionRenderables);

        var moles = players.Where(x => x != player).ToArray();
        var maybeMoles = moles.Select(x => x.GetSpriteProjection(player)).ToArray();
        var moleRenderables = maybeMoles.Where(x => x.HasValue).Select(x => x.Value).ToArray();
        buffer.AddRange(moleRenderables);

        var pickups = _engine.Map.Pickups;
        var maybePickups = pickups.Select(x => x.GetSpriteProjection(player)).ToArray();
        var pickupRenderables = maybePickups.Where(x => x.HasValue).Select(x => x.Value).ToArray();
        buffer.AddRange(pickupRenderables);

        var graves = _engine.Map.Graves;
        var maybeGraves = graves.Select(x => x.GetSpriteProjection(player)).ToArray();
        var graveRenderables = maybeGraves.Where(x => x.HasValue).Select(x => x.Value).ToArray();
        buffer.AddRange(graveRenderables);

        // Painter's algorithm (draw from farthest to nearest). This didn't
        // take me so long to remember/figure out this time! ;)
        var orderedBuffer = buffer.OrderByDescending(x => x.Depth).ToArray();
        return orderedBuffer;
    }

    private void DrawHorizontalSplitLine(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawRectangle(
            new Rectangle(0, Settings.ScreenHalfHeight, Settings.ScreenWidth, 2),
            Color.Gray);
    }

    private void DrawVerticalSplitLine(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawRectangle(
            new Rectangle(Settings.ScreenHalfWidth, 0, 2, Settings.ScreenHeight),
            Color.Gray);
    }
}