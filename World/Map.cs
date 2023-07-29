using System;
using System.Collections.Generic;
using System.Linq;
using FireInTheHole.Objects;
using FireInTheHole.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FireInTheHole.World;

public class Map
{   
    private readonly GameEngine _engine;

    public int[,] CurrentMap { get; set; }

    public String Name { get; set; }

    public Map(GameEngine engine)
    {
        _engine = engine;

        LoadContent();
        CurrentMap = Pillars;
        Name = nameof(Pillars);
    }

    private void LoadContent()
    {
        LoadTextures();
    }

    private void LoadTextures()
    {
        WallTexture1 = _engine.Content.Load<Texture2D>("256_earth-grass");
        WallTexture2 = _engine.Content.Load<Texture2D>("256_stone-grass");
    }

    public void NextMap()
    {
        if (CurrentMap == Pillars)
        {
            CurrentMap = BigEmpty;
            Name = nameof(BigEmpty);
        }
        else
        {
            CurrentMap = Pillars;
            Name = nameof(Pillars);
        }
    }

    public Color FloorColor => new Color(r: 54, g: 35, b: 23);
    
    public Color SkyColor => Color.SkyBlue;

    public Texture2D WallTexture1 { get; private set; }

    public Texture2D WallTexture2 { get; private set; }

    public int Width => CurrentMap.GetLength(1);

    public int Height => CurrentMap.GetLength(0);

    public List<Pickup> Pickups { get; } = new List<Pickup>();

    public List<Grave> Graves { get; } = new List<Grave>();

    public int? GetTileAt(Vector2 position)
    {
        var mapCoordinatesOrNull = GetMapCoordinates(position);
        if (!mapCoordinatesOrNull.HasValue)
        {
            return null;
        }

        var (x, y) = mapCoordinatesOrNull.Value;
        return CurrentMap[y, x];
    }

    public int? GetTileAt(int mapX, int mapY)
    {
        if (mapX < 0 || mapX >= Width || mapY < 0 || mapY >= Height)
        {
            return null;
        }

        return CurrentMap[mapY, mapX];
    }

    public void DestroyWall(int x, int y)
    {
        CurrentMap[y, x] = 0;
        var maybePickup = Pickup.Maybe(
            _engine, 
            new Vector2((x * 50) + 25, (y * 50) + 25));
        if (maybePickup != null) AddPickup(maybePickup);
    }

    public void RemovePickup(Pickup pickup)
    {
        Pickups.Remove(pickup);
    }

    public void AddPickup(Pickup pickup)
    {
        Pickups.Add(pickup);
    }

    public void AddGrave(Mole mole)
    {
        var grave = new Grave(_engine, mole.Position, mole.PlayerIndex);
        Graves.Add(grave);
    }

    public bool CanDestroyWall(int wall)
    {
        return wall == 1;
    }

    public (int, int)? GetMapCoordinates(Vector2 position)
    {
        var x = (int)position.X / 50;
        var y = (int)position.Y / 50;        

        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return null;
        }

        return (x, y);
    }

    public bool IsPickupAt(Vector2 position, out Pickup pickup)
    {
        pickup = Pickups.FirstOrDefault(x => 
            GetMapCoordinates(new Vector2(x.Position.X - 25, x.Position.Y -25)) == GetMapCoordinates(position));
        return pickup != null;
    }

    public bool IsWall(int x, int y)
    {
        return CurrentMap[y, x] == 1 || CurrentMap[y, x] == 2;
    }

    public int KillPlayersAt(Mole owner, int x, int y)
    {
        var players = new[] {_engine.Player1, _engine.Player2, _engine.Player3, _engine.Player4};
        var activePlayers = players.Where(x => x != null);
        var alivePlayers = activePlayers.Where(x => !x.IsDead);
        var xs = alivePlayers.ToArray();
        var kills = 0;
        for (var i = 0; i < xs.Length; i++)
        {
            var player = xs[i];
            var coordinates = GetMapCoordinates(player.Position);
            if (!coordinates.HasValue)
            {
                continue;
            }
            var (px, py) = coordinates.Value;
            if (px == x && py == y)
            {
                player.Kill();
                kills = player != owner ? kills + 1 : kills - 1;
            }
        }
        return kills;
    }    

    public Vector2 GetStartingPosition(PlayerIndex playerNumber)
    {
        // Player One will start at the tile indicated with a 9.
        // Player Two will start at the tile indicated with a 8.
        // Player Three will start at the tile indicated with a 7.
        // Player Four will start at the tile indicated with a 6.
        // Find the X and Y coordinates of the tile with the player number.
        var tile = playerNumber switch
        {
            PlayerIndex.One => 9,
            PlayerIndex.Two => 8,
            PlayerIndex.Three => 7,
            PlayerIndex.Four => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(playerNumber), playerNumber, null)
        };
        
        for (var y = 0; y < CurrentMap.GetLength(0); y++)
        {
            for (var x = 0; x < CurrentMap.GetLength(1); x++)
            {
                if (CurrentMap[y, x] == tile)
                {
                    var v = new Vector2(x * 50 + 25, y * 50 + 25);
                    return v;
                }
            }
        }

        throw new Exception("Player tile not on map");
    }

    public readonly int[,] Pillars = 
    {            
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,},
        {2,9,0,0,2,0,2,0,2,0,2,0,2,0,2,0,0,2,0,2,0,2,0,2,0,2,0,2,0,0,6,2,},
        {2,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,2,},
        {2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,},
        {2,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,2,},
        {2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,},
        {2,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,2,},
        {2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,},
        {2,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,2,},
        {2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,},
        {2,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,2,},
        {2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,},
        {2,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,2,},
        {2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,},
        {2,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,2,},
        {2,7,0,0,2,0,2,0,2,0,2,0,2,0,2,0,0,2,0,2,0,2,0,2,0,2,0,2,0,0,8,2,},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,},
    };

    public readonly int[,] BigEmpty = 
    {            
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,},
        {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,},
        {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,},
        {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,},
        {2,0,0,0,9,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,7,0,0,0,2,},
        {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,},
        {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,},
        {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,},
        {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,},
        {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,},
        {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,},
        {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,},
        {2,0,0,0,6,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,0,0,0,2,},
        {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,},
        {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,},
        {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,},
    };
}