namespace FireInTheMole.Game

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open System


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
            tile: TileMap.Tile
            tileCoords: TileMap.TileCoords
            tileOffset: float32
        }

    [<Struct>]
    type Options = 
        {
            count: int
            halfCount: int
            maxLength: int
            fov: float32
        }

    type RayCaster = 
        {
            options: Options
            origin: Vector2
            angleInDegrees: float32
            rays: Ray array
            map: TileMap.TileMap
        }

    let createOptions fov count maxLength = 
        {
            count = count
            halfCount = count / 2
            maxLength = maxLength
            fov = fov
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
        let drawOne (ray : Ray) =
            let finish = rayCaster.origin + Vector2(ray.depth * ray.sin, ray.depth * ray.cos)
            drawLine sb tx rayCaster.origin finish 2 c
        Seq.iter drawOne rayCaster.rays

    /// Updates the rayCaster with new rays calculated based on the given origin and angle
    /// There is quite a lot of mutability in this function, but: 
    /// 1. It's a performance critical function
    /// 2. It's a translation of a C# version which was also performance critical
    let update (rayCaster : RayCaster) map origin angleInDegrees =
        let avoidDivisionByZero = 0.0001f
        let options = rayCaster.options
        let angleInRadians = toRadians angleInDegrees
        let fovInRadians = toRadians options.fov
        let halfFovInRadians = fovInRadians / 2.0f
        let rayAngleDeltaInRadians = fovInRadians / float32 options.count
        let coords = TileMap.toTileCoords map origin
        let mutable rayAngleInRadians = angleInRadians - halfFovInRadians + avoidDivisionByZero        
        for i in 0 .. options.count - 1 do
            let raySin = sin rayAngleInRadians
            let rayCos = cos rayAngleInRadians
            let mutable horizontal = Vector3.Zero
            let mutable vertical = Vector3.Zero
            let mutable delta = Vector3.Zero
            let mutable depth = 0.0f
            let mutable offset = 0.0f
            let mutable horizontalCoords : TileMap.TileCoords = { x = 0; y = 0 }
            let mutable verticalCoords : TileMap.TileCoords = { x = 0; y = 0 }
            let mutable horizontalTile : TileMap.Tile option = None
            let mutable verticalTile : TileMap.Tile option = None
            // Perform the horizontal calculations
            horizontal.Y <- if raySin > 0f then float32 coords.y + 1f else float32 coords.y - avoidDivisionByZero
            delta.Y <- if raySin > 0f then 1f else -1f
            horizontal.Z <- (horizontal.Y - origin.Y) / raySin
            horizontal.X <- (origin.X + horizontal.Z * rayCos)
            delta.Z <- delta.Y / raySin
            delta.X <- delta.Z * rayCos
            for j in 0 .. options.maxLength - 1 do
                if horizontalTile.IsNone then
                    horizontalCoords <-
                        {
                            x = Math.Clamp(int horizontal.X, 0, map.width - 1)
                            y = Math.Clamp(int horizontal.Y, 0, map.height - 1) 
                        }
                    match TileMap.getCollidableTile map horizontalCoords with
                    | Some t -> 
                        horizontalTile <- Some t
                    | None ->
                        horizontal.X <- horizontal.X + delta.X
                        horizontal.Y <- horizontal.Y + delta.Y
                        horizontal.Z <- horizontal.Z + delta.Z
            // Perform the vertical calculations
            vertical.X <- if rayCos > 0f then float32 coords.x + 1f else float32 coords.x - avoidDivisionByZero
            delta.X <- if rayCos > 0f then 1f else -1f
            vertical.Z <- (vertical.X - origin.X) / rayCos
            vertical.Y <- (origin.Y + vertical.Z * raySin)
            delta.Z <- delta.X / rayCos
            delta.Y <- delta.Z * raySin
            for j in 0 .. options.maxLength - 1 do
                if verticalTile.IsNone then
                    verticalCoords <- 
                        {
                            x = Math.Clamp(int vertical.X, 0, map.width - 1)
                            y = Math.Clamp(int vertical.Y, 0, map.height - 1) 
                        }
                    match TileMap.getCollidableTile map verticalCoords with
                    | Some t -> 
                        verticalTile <- Some t
                    | None ->
                        vertical.X <- vertical.X + delta.X
                        vertical.Y <- vertical.Y + delta.Y
                        vertical.Z <- vertical.Z + delta.Z
            // Traversal complete - determine which tile is closer
            let mutable tileCoords : TileMap.TileCoords = { x = 0; y = 0 }
            let mutable tile : TileMap.Tile = Unchecked.defaultof<TileMap.Tile>
            if vertical.Z < horizontal.Z then
                tileCoords <- verticalCoords
                tile <- verticalTile.Value
                depth <- vertical.Z
                vertical.Y <- vertical.Y % 1.0f
                offset <- if rayCos > 0f then vertical.Y else 1.0f - vertical.Y
            else
                tileCoords <- horizontalCoords
                tile <- horizontalTile.Value
                depth <- horizontal.Z
                horizontal.X <- horizontal.X % 1.0f
                offset <- if raySin > 0f then 1.0f - horizontal.X else horizontal.X
            // Fix the fisheye effect
            depth <- depth * cos (rayAngleInRadians - angleInRadians)
            // Scale the tile by the tile width to get the actual distance
            depth <- depth// * float32 map.tileWidth
            // Use the data we've collected to create a ray
            let ray = 
                {
                    sin = raySin
                    cos = rayCos
                    depth = depth
                    tileCoords = tileCoords
                    tile = tile
                    tileOffset = offset
                }            
            // FSharp arrays are mutable so we can just assign to them
            rayCaster.rays.[i] <- ray
            // And update the angle for the next ray
            rayAngleInRadians <- rayAngleInRadians + rayAngleDeltaInRadians        
        // Finally, return an updated copy of the rayCaster
        { rayCaster with origin = origin; angleInDegrees = angleInDegrees; map = map }