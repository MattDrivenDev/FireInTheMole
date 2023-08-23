namespace FireInTheMole.Game
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open System


module Animation = 

    type AnimationAngle = 
        | Up            // 12 o'clock
        | Down          // 6 o'clock
        | Left          // 9 o'clock
        | Right         // 3 o'clock
        | UpLeft        // 10:30
        | UpRight       // 1:30
        | DownLeft      // 7:30
        | DownRight     // 4:30

    type AnimationKey = 
        | IdleAnimation
        | MoveAnimation
        | DeadAnimation

    type Animation = {
        texture: Texture2D
        frames: Rectangle array
        fps: int
        currentFrame: int
        frameTimer: float32
        frameLength: float32
        size: Point
    }

    // Active Pattern to convert a float32<degrees> to an AnimationAngle
    let (|Angle|_|) angle = 
        let angle = angle % 360f<degrees>
        if angle >= 337.5f<degrees> || angle < 22.5f<degrees> then Some(Up)
        elif angle >= 22.5f<degrees> && angle < 67.5f<degrees> then Some(UpRight)
        elif angle >= 67.5f<degrees> && angle < 112.5f<degrees> then Some(Right)
        elif angle >= 112.5f<degrees> && angle < 157.5f<degrees> then Some(DownRight)
        elif angle >= 157.5f<degrees> && angle < 202.5f<degrees> then Some(Down)
        elif angle >= 202.5f<degrees> && angle < 247.5f<degrees> then Some(DownLeft)
        elif angle >= 247.5f<degrees> && angle < 292.5f<degrees> then Some(Left)
        elif angle >= 292.5f<degrees> && angle < 337.5f<degrees> then Some(UpLeft)
        else None

    let horizontalFrames count (size : Point) (offset : Point) = 
        let cell i = 
            let offset' = offset + Point(i * size.X, 0)
            Rectangle(offset', size)
        Array.init count cell

    let create tx frameCount fps size offset = {
        texture = tx
        frames = horizontalFrames frameCount size offset
        fps = fps
        currentFrame = 0
        frameTimer = 0f
        frameLength = 1f / float32 fps
        size = size
    }

    let currentFrame animation = 
        animation.frames.[animation.currentFrame]

    let reset animation = 
        { animation with 
            currentFrame = 0; frameTimer = 0f }

    let update (gametime: GameTime) animation = 
        let frameTimer, frame = 
            match animation.frameTimer + float32 gametime.ElapsedGameTime.TotalSeconds with
            | t when t >= animation.frameLength -> 0f, (animation.currentFrame + 1) % animation.frames.Length
            | t -> t, animation.currentFrame
        { animation with
            frameTimer = frameTimer
            currentFrame = frame }

    let draw (sb : SpriteBatch) animation color (destination : Rectangle) =
        let source = currentFrame animation
        sb.Draw(animation.texture, destination, source, color)