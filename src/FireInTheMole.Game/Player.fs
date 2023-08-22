namespace FireInTheMole.Game
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open System
open Microsoft.Xna.Framework.Input


module Player = 

    [<Literal>] 
    let size = 64

    [<Measure>] type degrees
    [<Measure>] type radians

    type Player = {
        position: Vector2
        angle: float32<degrees>
        speed: float32
        texture: Texture2D
        size: Point
        offset: Point
        index: PlayerIndex
        active: bool
        color : Color
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

    let create tx idx active p = {
        position = p
        angle = 0f<degrees>
        speed = 166f
        texture = tx
        size = Point(size, size)
        offset = Point(0, 0)
        index = idx
        active = active
        color = color idx
    }

    let draw sb player = 
        let draw' (sb : SpriteBatch) player =
            let sourceRect = Rectangle(player.offset, player.size)
            sb.Draw(player.texture, player.position, Nullable.op_Implicit sourceRect, player.color)
        if player.active then draw' sb player

    /// We're only getting input for player one at the moment
    let getInput player =
        let ks = Keyboard.GetState()
       
        let (|KeyDown|_|) key (ks : KeyboardState) = 
            if ks.IsKeyDown(key) then Some() else None

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

        match input with
        | Some input -> update' input
        | None -> player