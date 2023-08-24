namespace FireInTheMole.Game
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics


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
        | IdleAnimation of AnimationAngle
        | MoveAnimation of AnimationAngle
        | DeadAnimation of AnimationAngle

    type Animation = {
        key: AnimationKey
        texture: Texture2D
        frames: Rectangle array
        fps: int
        currentFrame: int
        frameTimer: float32
        frameLength: float32
        size: Point
    }

    let horizontalFrames count (size : Point) (offset : Point) = 
        let cell i = 
            let offset' = offset + Point(i * size.X, 0)
            Rectangle(offset', size)
        Array.init count cell

    let create key tx frameCount fps size offset = {
        key = key
        texture = tx
        frames = horizontalFrames frameCount size offset
        fps = fps
        currentFrame = 0
        frameTimer = 0f
        frameLength = 1f / float32 fps
        size = size
    }

    let currentFrame animation = 
        let sourceRect = animation.frames.[animation.currentFrame]        
        sourceRect

    let reset animation = 
        { animation with 
            currentFrame = 0; frameTimer = 0f }

    let update (gametime: GameTime) animation = 
        let dt = float32 gametime.ElapsedGameTime.TotalSeconds
        let frameTimer, frame =             
            match animation.frameTimer + dt with
            | t when t >= animation.frameLength -> 
                0f, (animation.currentFrame + 1) % animation.frames.Length
            | t -> 
                t + dt, animation.currentFrame
        let newAnimation = 
            { animation with
                frameTimer = frameTimer
                currentFrame = frame }
        newAnimation

    let draw (sb : SpriteBatch) animation color rev (destination : Rectangle) =
        let source = currentFrame animation
        let origin = Vector2.Zero
        let effects = 
            if rev then SpriteEffects.FlipHorizontally
            else SpriteEffects.None
        sb.Draw(animation.texture, destination, source, color, 0f, origin, effects, 0f)