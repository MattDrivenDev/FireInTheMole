namespace FireInTheMole.Game
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content


[<RequireQualifiedAccess>]
module TileMap = 

    type TileMap = {
        layers: Map<string, TileMapLayer>
    }
    and TileMapLayer = {
        name: string
        tiles: Map<int * int, Tile>
    }
    and Tile = {
        key: int * int
    }

    // --------------------------------------------------------------------
    /// To avoid a leaky abstraction, we'll lock away the integration
    /// MonoGame.Extended.Tiled in a private module that is used by
    /// the public module.
    module private TiledIntegration = 
        open MonoGame.Extended.Tiled

        let mapTile (tiledTile : TiledMapTile) = 
            let key = int tiledTile.X, int tiledTile.Y
            { key = key }

        let mapLayer (tiledLayer : TiledMapTileLayer) =            
            let tiles =
                tiledLayer.Tiles 
                |> Seq.map mapTile 
                |> Seq.map (fun t -> t.key, t)
                |> Map.ofSeq            
            { name = tiledLayer.Name
              tiles = tiles }

        let tiledMapTileLayer (layer : TiledMapLayer) =
            if layer :? TiledMapTileLayer then Some (layer :?> TiledMapTileLayer) else None

        let loadMap (cm : ContentManager) map = 
            let tmx = cm.Load<TiledMap>(map)
            let layers = 
                tmx.Layers 
                |> Seq.choose tiledMapTileLayer
                |> Seq.map mapLayer 
                |> Seq.map (fun l -> l.name, l) 
                |> Map.ofSeq
            let tilesets = Array.ofSeq tmx.Tilesets
            { layers = layers }
        
    // --------------------------------------------------------------------

    let create (cm : ContentManager) map = 
        TiledIntegration.loadMap cm map