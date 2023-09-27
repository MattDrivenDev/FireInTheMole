namespace FireInTheMole.Game

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open System


[<RequireQualifiedAccess>]
module RayCasting = 

    [<Struct>] 
    type Ray = 
        {
            sin: float32
            cos: float32
            depth: float32
            tile: TileMap.Tile
            tileCoords: TileMap.TileCoords
            tileOffset: float32
        }

    type Options = 
        {
            count: int
            halfCount: int
            maxLengthInTiles: int
            fov: float32
            correctFishEye: bool
        }

    type RayCaster = 
        {
            options: Options
            origin: Vector2
            angleInDegrees: float32
            rays: Ray array
            map: TileMap.TileMap
        }

    let createOptions fov count maxLengthInTiles correctFishEye = 
        {
            count = count
            halfCount = count / 2
            maxLengthInTiles = maxLengthInTiles
            fov = fov
            correctFishEye = correctFishEye
        }

    let create (options : Options) map origin angleInDegrees =
        {
            options = options
            origin = origin
            angleInDegrees = angleInDegrees
            rays = Array.zeroCreate options.count
            map = map
        }

    /// Draws the rays to the screen which is useful for debugging
    let draw (sb : SpriteBatch) (c : Color) (rayCaster : RayCaster) = 
        let tx = createPixelTexture2D sb.GraphicsDevice
        let mutable rectColor = Color.Magenta
        let rect = Rectangle(MAP_TILE_SIZE, 0, MAP_TILE_SIZE, MAP_TILE_SIZE)
        let drawOne (ray : Ray) =
            let finish = rayCaster.origin + Vector2(ray.depth * ray.cos, ray.depth * ray.sin)
            drawLine sb tx rayCaster.origin finish 2 c
        Seq.iter drawOne rayCaster.rays

    /// Updates the rayCaster with new rays calculated based on the given origin and angle
    /// There is quite a lot of mutability in this function, but: 
    /// 1. It's a performance critical function
    /// 2. It's a translation of a C# version which was also performance critical
    let update (rayCaster : RayCaster) (map : TileMap.TileMap) (origin : Vector2) angleInDegrees =
        let avoidDivisionByZero = 0.0001f
        let originX = origin.X / float32 map.tileWidth
        let originY = origin.Y / float32 map.tileHeight
        let originCoords = TileMap.toTileCoords map origin
        let mapX, mapY = originCoords.x, originCoords.y
        let angleInRadians = toRadians angleInDegrees
        let fovInRadians = toRadians rayCaster.options.fov
        let halfFovInRadians = fovInRadians / 2.0f
        let rayAngleDeltaInRadians = fovInRadians / float32 rayCaster.options.count
        let mutable rayAngleInRadians = angleInRadians - halfFovInRadians + avoidDivisionByZero
        for i in 0 .. rayCaster.options.count - 1 do
            // Setup variables for the ray
            let sin = MathF.Sin(rayAngleInRadians)
            let cos = MathF.Cos(rayAngleInRadians)
            let mutable horizontalX = 0f
            let mutable horizontalY = 0f
            let mutable horizontalDepth = 0f
            let mutable verticalX = 0f
            let mutable verticalY = 0f
            let mutable verticalDepth = 0f
            let mutable deltaX = 0f
            let mutable deltaY = 0f
            let mutable deltaDepth = 0f
            let mutable depth = 0f
            let mutable tile = Unchecked.defaultof<TileMap.Tile>
            let mutable textureOffset = 0f
            let mutable horizontalTile : TileMap.Tile option = None
            let mutable verticalTile : TileMap.Tile option = None
            // Perform horizontal calculations for the ray
            horizontalY <- if sin > 0f then (float32 mapY) + 1f else (float32 mapY) - avoidDivisionByZero
            deltaY <- if sin > 0f then 1f else -1f
            horizontalDepth <- (horizontalY - originY) / sin
            horizontalX <- originX + (horizontalDepth * cos)
            deltaDepth <- deltaY / sin
            deltaX <- deltaDepth * cos
            for j in 0 .. rayCaster.options.maxLengthInTiles do
                if horizontalTile.IsNone then
                    let horizontalMapX = Math.Clamp(int horizontalX, 0, map.width - 1)
                    let horizontalMapY = Math.Clamp(int horizontalY, 0, map.height - 1)
                    match TileMap.getCollidableTile map { x = horizontalMapX; y = horizontalMapY } with
                    | Some t -> horizontalTile <- Some t
                    | None -> 
                        horizontalX <- horizontalX + deltaX
                        horizontalY <- horizontalY + deltaY
                        horizontalDepth <- horizontalDepth + deltaDepth
            // Perform vertical calculations for the ray
            verticalX <- if cos > 0f then (float32 mapX) + 1f else (float32 mapX) - avoidDivisionByZero
            deltaX <- if cos > 0f then 1f else -1f
            verticalDepth <- (verticalX - originX) / cos
            verticalY <- originY + (verticalDepth * sin)
            deltaDepth <- deltaX / cos
            deltaY <- deltaDepth * sin
            for j in 0 .. rayCaster.options.maxLengthInTiles do
                if verticalTile.IsNone then
                    let verticalMapX = Math.Clamp(int verticalX, 0, map.width - 1)
                    let verticalMapY = Math.Clamp(int verticalY, 0, map.height - 1)
                    match TileMap.getCollidableTile map { x = verticalMapX; y = verticalMapY } with
                    | Some t -> verticalTile <- Some t
                    | None -> 
                        verticalX <- verticalX + deltaX
                        verticalY <- verticalY + deltaY
                        verticalDepth <- verticalDepth + deltaDepth
            // Determine which of the two tiles is closer to the origin
            if verticalDepth < horizontalDepth then
                tile <- verticalTile.Value
                depth <- verticalDepth
                verticalY <- verticalY % 1f
                textureOffset <- if cos > 0f then verticalY else 1f - verticalY
            else
                tile <- horizontalTile.Value
                depth <- horizontalDepth
                horizontalX <- horizontalX % 1f
                textureOffset <- if sin > 0f then 1f - horizontalX else horizontalX
            // Fix the fish-eye effect
            //if rayCaster.options.correctFishEye then 
            //    depth <- depth * MathF.Cos(angleInRadians - rayAngleInRadians)
            let ray = 
                {
                    sin = sin
                    cos = cos
                    depth = depth * float32 map.tileWidth
                    tile = tile
                    tileOffset = textureOffset
                    tileCoords = tile.key
                }
            rayCaster.rays.[i] <- ray
            rayAngleInRadians <- rayAngleInRadians + rayAngleDeltaInRadians
        { rayCaster with origin = origin; angleInDegrees = angleInDegrees; map = map }

    /// Here we go, a re-implementation of the raycasting algorithm using
    /// forward and right vectors - which doesn't use equal angles for each ray
    /// so that we don't get the fish-eye effect.
    /// https://gamedev.stackexchange.com/questions/169546/understanding-the-rendering-of-the-raycasting-on-flat-screen/169548#169548
    let updateByDirection rayCaster map origin (forward : Vector2) =
        let right = Vector2(forward.Y, -forward.X)
        let fov = toRadians rayCaster.options.fov
        let halfFov = fov / 2.0f
        let halfHeight = 10f
        let halfWidth = 10f
        for i in 0 .. rayCaster.rays.Length - 1 do
            let offset = (2f * float32 i) / (float32 rayCaster.rays.Length - 1f) - 1f
            let rayStart = origin
            let rayEnd = forward + right * halfWidth * offset
            ()
        let angleInDegrees = MathF.Atan2(forward.Y, forward.X) |> toDegrees
        update rayCaster map origin angleInDegrees