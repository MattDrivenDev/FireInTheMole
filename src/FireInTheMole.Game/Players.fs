namespace FireInTheMole.Game
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open System


[<RequireQualifiedAccess>]
module Players = 

    type Player = 
        {
            bounds: Collisions.BoundingRectangle
            direction: Vector2
            speed: float32
            offset: Point
            index: PlayerIndex
            active: bool
            color : Color
            animations: Map<Animations.AnimationKey, Animations.Animation>
            currentAnimation: Animations.Animation
            rayCaster: RayCasting.RayCaster 
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

    type Input = 
        {
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
        Animations.create k tx 2 4 s o

    let moveUpRightAnimation k tx = 
        let s = Point(256, 512)
        let o = Point(0, 512)
        Animations.create k tx 2 4 s o

    let moveDownRightAnimation k tx = 
        let s = Point(256, 512)
        let o = Point(0, 1024)
        Animations.create k tx 2 4 s o

    let moveDownAnimation k tx =
        let s = Point(256, 512)
        let o = Point(0, 1536)
        Animations.create k tx 2 4 s o

    let moveUpAnimation k tx = 
        let s = Point(256, 512)
        let o = Point(0, 2048)
        Animations.create k tx 2 4 s o

    let loadAnimations tx = 
        let walkRight = moveRightAnimation (Animations.MoveAnimation Animations.AnimationAngle.Right) tx
        let walkUpRight = moveUpRightAnimation (Animations.MoveAnimation Animations.AnimationAngle.UpRight) tx
        let walkUp = moveUpAnimation (Animations.MoveAnimation Animations.AnimationAngle.Up) tx
        let walkUpLeft = moveUpRightAnimation (Animations.MoveAnimation Animations.AnimationAngle.UpLeft) tx        
        let walkLeft = moveRightAnimation (Animations.MoveAnimation Animations.AnimationAngle.Left) tx  
        let walkDownLeft = moveDownRightAnimation (Animations.MoveAnimation Animations.AnimationAngle.DownLeft) tx
        let walkDown = moveDownAnimation (Animations.MoveAnimation Animations.AnimationAngle.Down) tx
        let walkDownRight = moveDownRightAnimation (Animations.MoveAnimation Animations.AnimationAngle.DownRight) tx
        [| walkRight; walkUpRight; walkUp; walkUpLeft; walkLeft; walkDownLeft; walkDown; walkDownRight |] 
        |> Seq.map (fun a -> a.key, a)
        |> Map.ofSeq

    let animationKey (direction : Vector2)=
        let radians = MathF.Atan2(direction.Y, direction.X)
        let angle = toDegrees radians
        if angle >= 337.5f || angle < 22.5f then Animations.MoveAnimation Animations.AnimationAngle.Right
        elif angle >= 22.5f && angle < 67.5f then Animations.MoveAnimation Animations.AnimationAngle.DownRight
        elif angle >= 67.5f && angle < 112.5f then Animations.MoveAnimation Animations.AnimationAngle.Down
        elif angle >= 112.5f && angle < 157.5f then Animations.MoveAnimation Animations.AnimationAngle.DownLeft
        elif angle >= 157.5f && angle < 202.5f then Animations.MoveAnimation Animations.AnimationAngle.Left
        elif angle >= 202.5f && angle < 247.5f then Animations.MoveAnimation Animations.AnimationAngle.UpLeft
        elif angle >= 247.5f && angle < 292.5f then Animations.MoveAnimation Animations.AnimationAngle.Up
        elif angle >= 292.5f && angle < 337.5f then Animations.MoveAnimation Animations.AnimationAngle.UpRight
        else failwith "Invalid angle"

    let create options (map : TileMap.TileMap) tx (idx : PlayerIndex) active = 
        let n = playerIndexToArrayIndex idx
        let spawn = TileMap.fromTileCoords map map.spawnPoints.[n]
        let size = Vector2(float32 PLAYER_SIZE, float32 PLAYER_SIZE)
        let animations = loadAnimations tx
        {
            bounds = Collisions.createBoundingRectangle spawn size
            direction = normVector2 Vector2.One
            speed = PLAYER_SPEED
            offset = Point(0, 0)
            index = idx
            active = active
            color = color idx
            animations = animations
            currentAnimation = animations.[Animations.MoveAnimation Animations.AnimationAngle.Right]
            rayCaster = RayCasting.create options map spawn 0f
        }

    let draw sb pixel player  =
        if player.active then  
            let finish = player.bounds.center + player.direction * float32 PLAYER_SIZE     
            let rev = 
                match animationKey player.direction with
                | Animations.MoveAnimation Animations.AnimationAngle.Left -> true
                | Animations.MoveAnimation Animations.AnimationAngle.UpLeft -> true
                | Animations.MoveAnimation Animations.AnimationAngle.DownLeft -> true
                | _ -> false
            let destination = player.bounds.center - (player.bounds.size / 2f)
            Rectangle(destination.ToPoint(), player.bounds.size.ToPoint())
            |> Animations.draw sb player.currentAnimation Color.White rev
            drawLine sb pixel player.bounds.center finish 1 player.color

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
            then 
                let input = 
                    { 
                        movement = movement
                        rotate = rotate
                        act = act
                    }
                Some input                 
            else None

    let update (gametime : GameTime) (map, mapBounds) (player, input) =
        let deltatime = gametime.ElapsedGameTime.TotalMilliseconds
        let currentAngle = MathF.Atan2(player.direction.Y, player.direction.X) |> toDegrees
        let apply input = 
            let newAngle = currentAngle + input.rotate * PLAYER_SPEED_ROTATION * float32(deltatime) |> normAngle
            let radians = toRadians newAngle
            let c = (cos radians)
            let s = (sin radians) 
            let newDirection = Vector2(c, s)
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
            let velocity = normVector2 positionDelta * player.speed * float32(deltatime)
            let boundsWithVelocity = Collisions.updateVelocity player.bounds velocity
            let collisions = Collisions.predictCollisions boundsWithVelocity mapBounds
            let resolvedBounds = Collisions.resolve boundsWithVelocity collisions
            let newBounds = Collisions.move resolvedBounds
            let newRayCaster = RayCasting.update player.rayCaster map newBounds.center newAngle
            let newAnimationKey = animationKey newDirection
            let newAnimation = player.animations.[newAnimationKey]
            let animation = 
                if newAnimation.key = player.currentAnimation.key
                    then Animations.update gametime player.currentAnimation 
                    else newAnimation
            { 
                player with 
                    bounds = newBounds
                    direction = newDirection
                    currentAnimation = animation
                    rayCaster = newRayCaster 
            }
        match input with
        | Some input -> apply input
        | None -> 
            // No input means player is dead or inactive etc... not that there is no input from a player.
            let newRayCaster = RayCasting.update player.rayCaster map player.bounds.center currentAngle
            { player with rayCaster = newRayCaster }