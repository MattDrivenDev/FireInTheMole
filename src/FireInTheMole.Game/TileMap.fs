namespace FireInTheMole.Game
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics


[<RequireQualifiedAccess>]
module TileMap = 

    // The use of the 'and' keyword is just some syntactic sugar
    // to let me define the types in a top-down fashion.

    type Tileset = {
        name: string
        texture: Texture2D
        tiles: Map<int, TilesetTile>
        firstTileId: int
        tileWidth: int
        tileHeight: int
        tileCount: int
        columns: int
        rows: int
    }
    and TilesetTile = {
        id: int
    }

    type TileMap = {
        name: string
        layers: TileMapLayer array
        tilesets: Map<string, Tileset>
        tileWidth: int
        tileHeight: int
        width: int
        height: int
        widthInPixels: int
        heightInPixels: int
    }
    and TileMapLayer = {
        name: string
        tiles: Map<TileCoords, Tile>
    }
    and Tile = {
        key: TileCoords
        tilesetTileId: int
        texture: Texture2D
        textureSource: Rectangle
        bounds: Collision.BoundingRectangle option
        active: bool
    }
    and TileCoords = {
        x: int
        y: int
    }

    // --------------------------------------------------------------------
    /// To avoid a leaky abstraction, we'll lock away the integration
    /// MonoGame.Extended.Tiled in a private module that is used by
    /// the public module.
    /// Why am I even doing this? Why not just use the Tiled library
    /// as is? In a previous prototype I found that the rendering wasn't
    /// perfect for my needs - so other than the data reading I'm not
    /// using the library at all.
    module private TiledIntegration = 
        open MonoGame.Extended.Tiled

        let mapTile (tmx : TiledMap) (tiledTile : TiledMapTile) = 
            let innerMapTile (tt : TiledMapTile) =
                let key = { x = int tt.X; y = int tt.Y }
                let tileset = tmx.GetTilesetByTileGlobalIdentifier tt.GlobalIdentifier
                let texture = tileset.Texture
                // This hardcoded 1 will be wrong if we have multiple tilesets in the map
                let source = tileset.GetTileRegion (tt.GlobalIdentifier - 1)
                { key = key
                  tilesetTileId = tt.GlobalIdentifier
                  bounds = None
                  texture = texture 
                  textureSource = source
                  active = true }
            if tiledTile.IsBlank then None else Some (innerMapTile tiledTile)                
                

        let mapLayer tmx (tiledLayer : TiledMapTileLayer) =
            let tiles = 
                tiledLayer.Tiles 
                |> Seq.choose (mapTile tmx)
                |> Seq.map (fun t -> t.key, t)
                |> Map.ofSeq            
            { name = tiledLayer.Name
              tiles = tiles }

        let mapTilesetTile (tile : TiledMapTilesetTile) = 
            { id = tile.LocalTileIdentifier }

        let mapTileset (tileSet : TiledMapTileset) = 
            let tiles = 
                tileSet.Tiles
                |> Seq.map mapTilesetTile
                |> Seq.map (fun t -> t.id, t)
                |> Map.ofSeq
            { name = tileSet.Name
              texture = tileSet.Texture
              tiles = tiles
              firstTileId = 1
              rows = tileSet.Rows
              columns = tileSet.Columns 
              tileWidth = tileSet.TileWidth
              tileHeight = tileSet.TileHeight
              tileCount = tileSet.TileCount }

        let tiledMapTileLayer (layer : TiledMapLayer) =
            if layer :? TiledMapTileLayer then Some (layer :?> TiledMapTileLayer) else None

        let loadMap (cm : ContentManager) map = 
            let tmx = cm.Load<TiledMap>(map)
            // Maintain the order of the layers
            let layers = 
                tmx.Layers 
                |> Seq.choose tiledMapTileLayer
                |> Seq.map (mapLayer tmx)
                |> Array.ofSeq
            let tilesets = 
                tmx.Tilesets
                |> Seq.map mapTileset
                |> Seq.map (fun t -> t.name, t)
                |> Map.ofSeq            
            { name = tmx.Name
              tilesets = tilesets
              layers = layers
              tileWidth = tmx.TileWidth
              tileHeight = tmx.TileHeight
              width = tmx.Width
              height = tmx.Height
              widthInPixels = tmx.WidthInPixels
              heightInPixels = tmx.HeightInPixels }        
    // --------------------------------------------------------------------

    let create (cm : ContentManager) map = 
        TiledIntegration.loadMap cm map

    let drawTile (sb : SpriteBatch) (tilemap : TileMap) (layer : TileMapLayer) (tile : Tile) = 
        // Only draw active tiles
        if not tile.active then ()
        let xp = tile.key.x * tilemap.tileWidth
        let yp = tile.key.y * tilemap.tileHeight
        let destination = Rectangle(xp, yp, tilemap.tileWidth, tilemap.tileHeight)
        let c = Color.White
        sb.Draw(tile.texture, destination, tile.textureSource, c)

    let drawLayer sb tilemap layer =
        layer.tiles
        |> Seq.map (fun t -> t.Value)
        |> Seq.iter (drawTile sb tilemap layer)

    let draw sb tilemap =
        // Draw the layers in the correct order
        tilemap.layers
        |> Seq.iter (drawLayer sb tilemap)

    let getTileset (tilemap : TileMap) name = 
        Map.tryFind name tilemap.tilesets

    let getLayer (tilemap : TileMap) name = 
        Seq.tryFind (fun l -> l.name = name) tilemap.layers

    let getTile (layer : TileMapLayer) key =
        Map.tryFind key layer.tiles

    let getBroadphaseTiles (tilemap : TileMap) (min : Vector2) (max : Vector2) = 
        let minTile = { x = int min.X / tilemap.tileWidth; y = int min.Y / tilemap.tileHeight }
        let maxTile = { x = int max.X / tilemap.tileWidth; y = int max.Y / tilemap.tileHeight }
        let tiles = 
            [| for x in minTile.x .. maxTile.x do
                for y in minTile.y .. maxTile.y do
                    yield { x = x; y = y } |]
        tiles

    let destroyTile tile = 
        { tile with 
               bounds = None
               active = false }

    let update (gt : GameTime) (tilemap : TileMap) = tilemap