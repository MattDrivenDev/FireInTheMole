namespace FireInTheMole.Game
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input


module Player = 

    [<Literal>] 
    let size = 64

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
        animation: Animation.Animation
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

    let create animation idx active p = {
        position = p
        angle = 0f<degrees>
        speed = 166f
        size = Point(size, size)
        offset = Point(0, 0)
        index = idx
        active = active
        color = color idx
        animation = animation
        state = Idle
    }

    let chooseAnimation = function
        | Idle -> Animation.IdleAnimation
        | Moving -> Animation.MoveAnimation
        | Dead -> Animation.DeadAnimation

    let draw sb player =
        if player.active then
            Rectangle(player.position.ToPoint(), player.size)
            |> Animation.draw sb player.animation Color.White 

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
            if ks.IsKeyDown(Keys.Left) then -1f<degrees>
            elif ks.IsKeyDown(Keys.Right) then 1f<degrees>
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

        let update' input = 
            let newPosition = player.position + input.movement * player.speed * float32(deltatime)
            let newAngle = player.angle + input.rotate * float32(deltatime)
            { player with 
                position = newPosition
                angle = newAngle
            }

        let animation = Animation.update gametime player.animation

        match input with
        | Some input -> { update' input with animation = animation }
        | None -> { player with animation = animation }