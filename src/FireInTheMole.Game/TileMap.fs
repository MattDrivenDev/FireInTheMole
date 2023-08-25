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
        bounds: Collision.BoundingRectangle option
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

        let mapTile (tiledTile : TiledMapTile) = 
            let key = { x = int tiledTile.X; y = int tiledTile.Y }
            if tiledTile.IsBlank then None 
            else Some { key = key
                        tilesetTileId = tiledTile.GlobalIdentifier
                        bounds = None}

        let mapLayer (tiledLayer : TiledMapTileLayer) =
            let tiles = 
                tiledLayer.Tiles 
                |> Seq.choose mapTile
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
                |> Seq.map mapLayer 
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

    let drawTile (sb : SpriteBatch) pixel (tilemap : TileMap) (layer : TileMapLayer) (tile : Tile) = 
        let xp = tile.key.x * tilemap.tileWidth
        let yp = tile.key.y * tilemap.tileHeight
        let rectangle = Rectangle(xp, yp, tilemap.tileWidth, tilemap.tileHeight)
        let c = 
            match layer.name with
            | "Walls" -> Color.Gray
            | "Dirt" -> Color.Brown
            | _ -> Color.White
        drawRectangle sb pixel rectangle c

    let drawLayer sb pixel tilemap layer =
        layer.tiles
        |> Seq.map (fun t -> t.Value)
        |> Seq.iter (drawTile sb pixel tilemap layer)

    let draw sb pixel tilemap =
        // Draw the layers in the correct order
        tilemap.layers
        |> Seq.iter (drawLayer sb pixel tilemap)