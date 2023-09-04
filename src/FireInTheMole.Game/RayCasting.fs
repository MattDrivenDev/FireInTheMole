namespace FireInTheMole.Game

open Microsoft.Xna.Framework


[<RequireQualifiedAccess>]
module RayCasting = 

    [<Literal>]
    let MaxRayCount = 320

    [<Struct>] 
    type Ray = 
        {
            sin: float32
            cos: float32
            depth: float32
            tile: TileMap.TileCoords
            tileOffset: float32
        }

    [<Struct>]
    type Options = 
        {
            count: int
            halfCount: int
            maxLength: float32
        }

    type RayCaster = 
        {
            options: Options
            origin: Vector2
            angleInDegrees: float32
            rays: Ray array
            map: TileMap.TileMap
        }

    let createOptions count maxLength = 
        {
            count = count
            halfCount = count / 2
            maxLength = maxLength
        }

    let create (options : Options) map origin angleInDegrees =
        {
            options = options
            origin = origin
            angleInDegrees = angleInDegrees
            rays = Array.zeroCreate options.count
            map = map
        }

    let update (rayCaster : RayCaster) map origin angleInDegrees =
        debug "CASTING RAYS ON YOU!"
        {
            rayCaster with
                origin = origin
                angleInDegrees = angleInDegrees
                map = map
        }