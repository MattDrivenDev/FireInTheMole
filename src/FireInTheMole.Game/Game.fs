namespace FireInTheMole.Game
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics


type FireInTheMoleGame() as this = 
    inherit Game()

    let bg = Color.Black
    let graphics = new GraphicsDeviceManager(this)
    let mutable sb = Unchecked.defaultof<SpriteBatch>
    let mutable pixel = Unchecked.defaultof<Texture2D>
    let mutable circle = Unchecked.defaultof<Texture2D>
    let mutable player1 = Unchecked.defaultof<Player.Player>
    let mutable player2 = Unchecked.defaultof<Player.Player>
    let mutable player3 = Unchecked.defaultof<Player.Player>
    let mutable player4 = Unchecked.defaultof<Player.Player>
    let mutable players = Unchecked.defaultof<Player.Player[]>

    do
        this.Content.RootDirectory <- "Content"
        this.IsMouseVisible <- true
        graphics.GraphicsProfile <- GraphicsProfile.HiDef
        graphics.IsFullScreen <- false

    let loadTextures() = 
        pixel <- new Texture2D(this.GraphicsDevice, 1, 1, false, SurfaceFormat.Color)
        pixel.SetData([| Color.White |])

        circle <- new Texture2D(this.GraphicsDevice, Player.size, Player.size, false, SurfaceFormat.Color)
        Array.init Player.size (fun y -> Array.init Player.size (fun x -> 
            let dx = float32 (x - Player.size / 2)
            let dy = float32 (y - Player.size / 2)
            let d = dx * dx + dy * dy
            if d < float32(Player.size / 2) then Color.White else Color.Transparent))
        |> Array.concat
        |> circle.SetData

    let loadPlayers() =
        player1 <- Player.create circle PlayerIndex.One true (Vector2(100f, 100f))
        player2 <- Player.create circle PlayerIndex.Two false (Vector2(200f, 200f))
        player3 <- Player.create circle PlayerIndex.Three false (Vector2(300f, 300f))
        player4 <- Player.create circle PlayerIndex.Four false (Vector2(400f, 400f))
        players <- [| player1; player2; player3; player4 |]

    member this.Graphics = graphics

    override this.Initialize() =
        base.Initialize()
    
    override this.LoadContent() =
        sb <- new SpriteBatch(this.GraphicsDevice)        
        loadTextures()
        loadPlayers()
        base.LoadContent()

    override this.Update(gametime) = 
        players <- players 
            |> Seq.map (fun p -> p, Player.getInput p)
            |> Seq.map (Player.update gametime)
            |> Array.ofSeq
        base.Update(gametime)

    override this.Draw(gametime) =
        this.GraphicsDevice.Clear bg
        sb.Begin()
        Seq.iter (Player.draw sb) players
        sb.End()
        base.Draw(gametime)