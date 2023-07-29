using System;
using FireInTheHole.Player;
using Microsoft.Xna.Framework;

namespace FireInTheHole.Player;

public class RayCaster
{
    private readonly GameEngine _engine;

    public RayCaster(GameEngine engine, Mole player)
    {
        _engine = engine;
        Player = player;
    }

    public Mole Player { get; set; }

    public Ray[] Rays { get; set; } = new Ray[Settings.PlayerRayCount];

    public void Update(GameTime gameTime)
    {
        CastThineRays();
    }

    private void CastThineRays()
    {
        var rayAngleDeltaInRadians = MathHelper.ToRadians(Settings.PlayerDeltaFovInDegrees);
        var origin = Player.Position;
        var originX = (origin.X / 50);
        var originY = (origin.Y / 50);
        var mapCoordinatesOrNull = _engine.Map.GetMapCoordinates(origin);
        if (!mapCoordinatesOrNull.HasValue)
        {
            // Should never happen... but just in case...
            return;
        }
        var (mapX, mapY) = mapCoordinatesOrNull.Value;
        var angleInRadians = MathHelper.ToRadians(Player.AngleInDegrees);
        var halfFovInRadians = MathHelper.ToRadians(Settings.PlayerHalfFovInDegrees);
        
        // Add a tiny fraction to the angle to avoid a divide by zero error
        var rayAngleInRadians = angleInRadians - halfFovInRadians + 0.0001f;

        for (var i = 0; i < Settings.PlayerRayCount; i++)
        {
            var sin = MathF.Sin(rayAngleInRadians);
            var cos = MathF.Cos(rayAngleInRadians);
            var horizontalX = 0f;
            var horizontalY = 0f;
            var horizontalDepth = 0f;
            var verticalX = 0f;
            var verticalY = 0f;
            var verticalDepth = 0f;
            var deltaX = 0f;
            var deltaY = 0f;
            var deltaDepth = 0f;
            var depth = 0f;
            var tile = 0;
            var textureOffset = 0f;
            int? horizontalTile = null;
            int? verticalTile = null;

            // Perform the horizontal calculations
            horizontalY = sin > 0 ? mapY + 1 : mapY - 0.000001f;
            deltaY = sin > 0 ? 1 : -1;
            horizontalDepth = (horizontalY - originY) / sin;
            horizontalX = (originX + horizontalDepth * cos);
            deltaDepth = deltaY / sin;
            deltaX = deltaDepth * cos;
            for (var j = 0; j < Player.RayMaxLength; j++)
            {
                var horizontalMapX = Math.Clamp((int)horizontalX, 0, _engine.Map.Width - 1);
                var horizontalMapY = Math.Clamp((int)horizontalY, 0, _engine.Map.Height - 1);

                if (_engine.Map.IsWall(horizontalMapX, horizontalMapY))
                {
                    horizontalTile = _engine.Map.CurrentMap[horizontalMapY, horizontalMapX];
                    break;
                }

                horizontalX += deltaX;
                horizontalY += deltaY;
                horizontalDepth += deltaDepth;
            }
            
            // Perform the vertical calculations
            verticalX = cos > 0 ? mapX + 1 : mapX - 0.000001f;
            deltaX = cos > 0 ? 1 : -1;
            verticalDepth = (verticalX - originX) / cos;
            verticalY = (originY + verticalDepth * sin);
            deltaDepth = deltaX / cos;
            deltaY = deltaDepth * sin;
            for (var j = 0; j < Player.RayMaxLength; j++)
            {
                var verticalMapX = Math.Clamp((int)verticalX, 0, _engine.Map.Width - 1);
                var verticalMapY = Math.Clamp((int)verticalY, 0, _engine.Map.Height - 1);

                if (_engine.Map.IsWall(verticalMapX, verticalMapY))
                {
                    verticalTile =_engine.Map.CurrentMap[verticalMapY, verticalMapX];
                    break;
                }

                verticalX += deltaX;
                verticalY += deltaY;
                verticalDepth += deltaDepth;
            }

            // Pick the shortest distance
            if (verticalDepth < horizontalDepth)
            {
                // Vertical hit
                tile = verticalTile.Value;
                depth = verticalDepth;
                verticalY %= 1;
                textureOffset = cos > 0 ? verticalY : 1 - verticalY;
            }
            else
            {
                // Horizontal hit
                tile = horizontalTile.Value;
                depth = horizontalDepth;
                horizontalX %= 1;
                textureOffset = sin > 0 ? 1 - horizontalX : horizontalX;
            }

            // Fix fish eye effect
            depth *= MathF.Cos(angleInRadians - rayAngleInRadians);

            // Add the ray to the array
            Rays[i] = new Ray 
            {  
                Sin = sin,
                Cos = cos,
                Depth = depth * 50,
                Tile = tile,
                TextureOffset = textureOffset
            };

            // Increment to the next ray angle
            rayAngleInRadians += rayAngleDeltaInRadians;
        }
    }
}