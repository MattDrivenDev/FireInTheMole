namespace FireInTheMole.Game
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input


[<RequireQualifiedAccess>]
module Player = 

    [<Literal>] 
    let size = 192
    [<Literal>]
    let speed = 200f
    [<Literal>]
    let turnSpeed = 10f 

    type Player = {
        position: Vector2
        angle: float32
        speed: float32
        size: Point
        offset: Point
        index: PlayerIndex
        active: bool
        color : Color
        animations: Map<Animation.AnimationKey, Animation.Animation>
        currentAnimation: Animation.Animation
    }

    type MovementDirection = 
        | Forward
        | Backward
        | Left
        | Right
        | ForwardLeft
        | ForwardRight
        | BackwardLeft
        | BackwardRight

    type Input = {
        movement: MovementDirection option
        rotate: float32
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
        if angle >= 337.5f || angle < 22.5f then Animation.MoveAnimation Animation.AnimationAngle.Right
        elif angle >= 22.5f && angle < 67.5f then Animation.MoveAnimation Animation.AnimationAngle.DownRight
        elif angle >= 67.5f && angle < 112.5f then Animation.MoveAnimation Animation.AnimationAngle.Down
        elif angle >= 112.5f && angle < 157.5f then Animation.MoveAnimation Animation.AnimationAngle.DownLeft
        elif angle >= 157.5f && angle < 202.5f then Animation.MoveAnimation Animation.AnimationAngle.Left
        elif angle >= 202.5f && angle < 247.5f then Animation.MoveAnimation Animation.AnimationAngle.UpLeft
        elif angle >= 247.5f && angle < 292.5f then Animation.MoveAnimation Animation.AnimationAngle.Up
        elif angle >= 292.5f && angle < 337.5f then Animation.MoveAnimation Animation.AnimationAngle.UpRight
        else failwith "Invalid angle"

    let create tx idx active p = 
        let animations = loadAnimations tx
        {
            position = p
            angle = 0f
            speed = speed
            size = Point(size, size)
            offset = Point(0, 0)
            index = idx
            active = active
            color = color idx
            animations = animations
            currentAnimation = animations.[Animation.MoveAnimation Animation.AnimationAngle.Right]
        }

    let draw sb pixel player  =
        if player.active then  
            let radians = toRadians player.angle
            let c = (cos radians)
            let s = (sin radians) 
            let finish = player.position + Vector2(c, s) * float32 size     
            let rev = 
                match animationKey player.angle with
                | Animation.MoveAnimation Animation.AnimationAngle.Left -> true
                | Animation.MoveAnimation Animation.AnimationAngle.UpLeft -> true
                | Animation.MoveAnimation Animation.AnimationAngle.DownLeft -> true
                | _ -> false
            let destination = player.position - (player.size.ToVector2() / 2f)
            Rectangle(destination.ToPoint(), player.size)
            |> Animation.draw sb player.currentAnimation Color.White rev
            drawLine sb pixel player.position finish 1 player.color

    /// We're only getting input for player one at the moment
    let getInput player =
        let ks = Keyboard.GetState()
        let movement = 
            match ks with
            | KeyDown Keys.W & KeyDown Keys.A -> Some ForwardLeft
            | KeyDown Keys.W & KeyDown Keys.D -> Some ForwardRight
            | KeyDown Keys.S & KeyDown Keys.A -> Some BackwardLeft
            | KeyDown Keys.S & KeyDown Keys.D -> Some BackwardRight
            | KeyDown Keys.W -> Some Forward
            | KeyDown Keys.S -> Some Backward
            | KeyDown Keys.A -> Some Left
            | KeyDown Keys.D -> Some Right
            | _ -> None
        let rotate =
            if ks.IsKeyDown(Keys.Left) then -45f
            elif ks.IsKeyDown(Keys.Right) then 45f
            else 0f
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
            let newAngle = 
                player.angle + input.rotate * turnSpeed * float32(deltatime) 
                |> normAngle
            let radians = toRadians newAngle
            let c = (cos radians)
            let s = (sin radians) 
            let positionDelta = 
                match input.movement with
                | Some Forward -> Vector2(c, s)
                | Some Backward -> Vector2(-c, -s)
                | Some Left -> Vector2(s, -c)
                | Some Right -> Vector2(-s, c)
                | Some ForwardLeft -> Vector2(c, s) + Vector2(s, -c)
                | Some ForwardRight -> Vector2(c, s) + Vector2(-s, c)
                | Some BackwardLeft -> Vector2(-c, -s) + Vector2(s, -c)
                | Some BackwardRight -> Vector2(-c, -s) + Vector2(-s, c)
                | None -> Vector2.Zero                
            let newPosition = player.position + normVector2 positionDelta * player.speed * float32(deltatime)
            let newAnimationKey = animationKey newAngle
            let newAnimation = player.animations.[newAnimationKey]
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