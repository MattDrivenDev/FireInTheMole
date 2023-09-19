namespace FireInTheMole.Game

open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework


module Projection = 

    [<Struct>] 
    type ProjectionData = 
        {
            texture: Texture2D
            source: Rectangle
            destination: Rectangle
            color: Color
            depth: float32
            reverse: bool
        }

    [<Struct>]
    type ProjectionOptions = 
        {
            tileWidth: int
            screenDistance: int
            screenWidth: int
            screenHalfWidth: int
            screenHeight: int
            screenHalfHeight: int
            textureMappingWidth: int
        }

    [<Struct>] 
    type Projection =
        | TextureMapping of textureMappingData: ProjectionData
        | Sprite of spriteData: ProjectionData

    let projectTextureMapping (options : ProjectionOptions) i (ray : RayCasting.Ray) = 
        let avoidDivisionByZero = 0.0001f
        let rayDepthInTiles = ray.depth / float32 options.tileWidth
        let projectionHeight = float32 options.screenDistance / (rayDepthInTiles + avoidDivisionByZero)
        let color = Color.White
        let offset = ray.tileOffset
        let texture = ray.tile.texture
        let textureSource = ray.tile.textureSource
        let destinationX = i * options.textureMappingWidth
        let destinationY = float32 options.screenHalfHeight - projectionHeight / 2f
        let destinationW = options.textureMappingWidth
        let destinationH = projectionHeight
        let destination = Rectangle(destinationX, int destinationY, destinationW, int destinationH)
        let sourceX = float32 textureSource.X + offset * (float32 textureSource.Width - float32 options.textureMappingWidth)
        let sourceY = 0.0f
        let sourceW = options.textureMappingWidth
        let sourceH = float32 textureSource.Height
        let source = Rectangle(int sourceX, int sourceY, sourceW, int sourceH)
        {
            texture = texture
            source = source
            destination = destination
            color = color
            depth = ray.depth
            reverse = false
        }

    let project options (player : Players.Player) =        
        if not player.active then [||] else
        player.rayCaster.rays
        |> Seq.mapi (projectTextureMapping options)
        |> Array.ofSeq

    let draw (sb : SpriteBatch) (projection : ProjectionData) =
        let effects = if projection.reverse then SpriteEffects.FlipHorizontally else SpriteEffects.None
        sb.Draw(projection.texture, projection.destination, projection.source, projection.color, 0f, Vector2.Zero, effects, 1f)