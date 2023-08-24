namespace FireInTheMole.Game
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input


module Player = 

    [<Literal>] 
    let size = 64
    [<Literal>]
    let speed = 166f
    [<Literal>]
    let turnSpeed = 10f 

    type PlayerState = 
        | Idle
        | Moving
        | Dead

    type Player = {
        position: Vector2
        angle: float32<degrees>
        speed: float32
        size: Point
        offset: Point
        index: PlayerIndex
        active: bool
        color : Color
        animations: Map<Animation.AnimationKey, Animation.Animation>
        currentAnimation: Animation.Animation
        state: PlayerState
    }

    type Input = {
        movement: Vector2
        rotate: float32<degrees>
        act: bool
    }

    let color = function
        | PlayerIndex.One -> Color.Red
        | PlayerIndex.Two -> Color.Blue
        | PlayerIndex.Three -> Color.Green
        | PlayerIndex.Four -> Color.Yellow
        | _ -> failwith "Invalid player index"

    let moveRightAnimation k tx = 
        let s = Point(256, 512)
        let o = Point(0, 0)
        Animation.create k tx 2 4 s o

    let moveUpRightAnimation k tx = 
        let s = Point(256, 512)
        let o = Point(0, 512)
        Animation.create k tx 2 4 s o

    let moveDownRightAnimation k tx = 
        let s = Point(256, 512)
        let o = Point(0, 1024)
        Animation.create k tx 2 4 s o

    let moveDownAnimation k tx =
        let s = Point(256, 512)
        let o = Point(0, 1536)
        Animation.create k tx 2 4 s o

    let moveUpAnimation k tx = 
        let s = Point(256, 512)
        let o = Point(0, 2048)
        Animation.create k tx 2 4 s o

    let loadAnimations tx = 
        let walkRight = moveRightAnimation (Animation.MoveAnimation Animation.AnimationAngle.Right) tx
        let walkUpRight = moveUpRightAnimation (Animation.MoveAnimation Animation.AnimationAngle.UpRight) tx
        let walkUp = moveUpAnimation (Animation.MoveAnimation Animation.AnimationAngle.Up) tx
        let walkUpLeft = moveUpRightAnimation (Animation.MoveAnimation Animation.AnimationAngle.UpLeft) tx        
        let walkLeft = moveRightAnimation (Animation.MoveAnimation Animation.AnimationAngle.Left) tx  
        let walkDownLeft = moveDownRightAnimation (Animation.MoveAnimation Animation.AnimationAngle.DownLeft) tx
        let walkDown = moveDownAnimation (Animation.MoveAnimation Animation.AnimationAngle.Down) tx
        let walkDownRight = moveDownRightAnimation (Animation.MoveAnimation Animation.AnimationAngle.DownRight) tx
        [| walkRight; walkUpRight; walkUp; walkUpLeft; walkLeft; walkDownLeft; walkDown; walkDownRight |] 
        |> Seq.map (fun a -> a.key, a)
        |> Map.ofSeq

    let animationKey angle =         
        if angle >= 337.5f<degrees> || angle < 22.5f<degrees> then Animation.MoveAnimation Animation.AnimationAngle.Right
        elif angle >= 22.5f<degrees> && angle < 67.5f<degrees> then Animation.MoveAnimation Animation.AnimationAngle.UpRight
        elif angle >= 67.5f<degrees> && angle < 112.5f<degrees> then Animation.MoveAnimation Animation.AnimationAngle.Up
        elif angle >= 112.5f<degrees> && angle < 157.5f<degrees> then Animation.MoveAnimation Animation.AnimationAngle.UpLeft
        elif angle >= 157.5f<degrees> && angle < 202.5f<degrees> then Animation.MoveAnimation Animation.AnimationAngle.Left
        elif angle >= 202.5f<degrees> && angle < 247.5f<degrees> then Animation.MoveAnimation Animation.AnimationAngle.DownLeft
        elif angle >= 247.5f<degrees> && angle < 292.5f<degrees> then Animation.MoveAnimation Animation.AnimationAngle.Down
        elif angle >= 292.5f<degrees> && angle < 337.5f<degrees> then Animation.MoveAnimation Animation.AnimationAngle.DownRight
        else failwith "Invalid angle"

    let create tx idx active p = 
        let animations = loadAnimations tx
        {
            position = p
            angle = 0f<degrees>
            speed = speed
            size = Point(size, size)
            offset = Point(0, 0)
            index = idx
            active = active
            color = color idx
            animations = animations
            currentAnimation = animations.[Animation.MoveAnimation Animation.AnimationAngle.Right]
            state = Idle
        }

    let chooseAnimation angle = function
        | Idle -> Animation.IdleAnimation angle
        | Moving -> Animation.MoveAnimation angle
        | Dead -> Animation.DeadAnimation angle

    let draw sb player =
        if player.active then  
            let rev = 
                match animationKey player.angle with
                | Animation.MoveAnimation Animation.AnimationAngle.Left -> true
                | Animation.MoveAnimation Animation.AnimationAngle.UpLeft -> true
                | Animation.MoveAnimation Animation.AnimationAngle.DownLeft -> true
                | _ -> false
            Rectangle(player.position.ToPoint(), player.size)
            |> Animation.draw sb player.currentAnimation Color.White rev

    /// We're only getting input for player one at the moment
    let getInput player =
        let ks = Keyboard.GetState()

        let movement = 
            match ks with
            | KeyDown Keys.W & KeyDown Keys.A -> Vector2.Normalize(Vector2(-1f, -1f))
            | KeyDown Keys.W & KeyDown Keys.D -> Vector2.Normalize(Vector2(1f, -1f))
            | KeyDown Keys.S & KeyDown Keys.A -> Vector2.Normalize(Vector2(-1f, 1f))
            | KeyDown Keys.S & KeyDown Keys.D -> Vector2.Normalize(Vector2(1f, 1f))
            | KeyDown Keys.W -> Vector2(0f, -1f)
            | KeyDown Keys.S -> Vector2(0f, 1f)
            | KeyDown Keys.A -> Vector2(-1f, 0f)
            | KeyDown Keys.D -> Vector2(1f, 0f)
            | _ -> Vector2.Zero

        let rotate =
            if ks.IsKeyDown(Keys.Left) then 45f<degrees>
            elif ks.IsKeyDown(Keys.Right) then -45f<degrees>
            else 0f<degrees>

        let act = ks.IsKeyDown(Keys.LeftControl)

        if player.index = PlayerIndex.One 
            then Some { 
                movement = movement
                rotate = rotate
                act = act
            }
            else None

    let update (gametime : GameTime) (player, input) =
        let deltatime = gametime.ElapsedGameTime.TotalSeconds
        let apply input = 
            let newPosition = player.position + input.movement * player.speed * float32(deltatime)
            let newAngle = player.angle + input.rotate * turnSpeed * float32(deltatime) |> normAngle
            let animationKey = animationKey newAngle
            let newAnimation = player.animations.[animationKey]
            let animation = 
                if newAnimation.key = player.currentAnimation.key
                    then Animation.update gametime player.currentAnimation 
                    else newAnimation
            { player with 
                position = newPosition
                angle = newAngle
                currentAnimation = animation }
        let newPlayer = 
            match input with
            | Some input -> apply input
            // No input means player is dead or inactive etc... not that there is no input from a player.
            | None -> player
        newPlayer