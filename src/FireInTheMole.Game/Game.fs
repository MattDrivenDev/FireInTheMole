namespace FireInTheMole.Game
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open MonoGame.Extended
open MonoGame.Extended.ViewportAdapters


type FireInTheMoleGame() as this = 
    inherit Game()

    let bg = Color.Black
    let graphics = new GraphicsDeviceManager(this)
    let mutable rt = Unchecked.defaultof<RenderTarget2D>
    let mutable scale = 1f
    let mutable sb = Unchecked.defaultof<SpriteBatch>
    let mutable pixel = Unchecked.defaultof<Texture2D>
    let mutable circle = Unchecked.defaultof<Texture2D>
    let mutable yellow = Unchecked.defaultof<Texture2D>
    let mutable players = Unchecked.defaultof<Players.Player[]>
    let mutable tilemap = Unchecked.defaultof<TileMap.TileMap>
    let mutable camera = Unchecked.defaultof<OrthographicCamera>

    do
        this.Content.RootDirectory <- "Content"
        this.IsMouseVisible <- true
        graphics.GraphicsProfile <- GraphicsProfile.HiDef
        graphics.IsFullScreen <- false

    let initializeGraphics() =
        scale <- MathHelper.Clamp(scale, 1f, 4f)
        let renderWidth = 640 * int scale
        let renderHeight = 360 * int scale
        graphics.PreferredBackBufferWidth <- renderWidth
        graphics.PreferredBackBufferHeight <- renderHeight
        graphics.ApplyChanges()
        rt <- new RenderTarget2D(
            this.GraphicsDevice,
            640,
            360,
            false,
            SurfaceFormat.Color,
            DepthFormat.Depth24Stencil8);
        let adapter = new BoxingViewportAdapter(this.Window, this.GraphicsDevice, renderWidth, renderHeight)
        camera <- new OrthographicCamera(adapter)
        camera.Zoom <- scale

    let loadTextures() = 
        // A single pixel is useful for drawing lots of primitives
        pixel <- createPixelTexture2D this.GraphicsDevice
        // A circle could be drawn with pixels, but that nukes the framerate so
        // we'll create the texture once and reuse it
        circle <- new Texture2D(this.GraphicsDevice, Players.size, Players.size, false, SurfaceFormat.Color)
        Array.init Players.size (fun y -> Array.init Players.size (fun x -> 
            let dx = float32 (x - Players.size / 2)
            let dy = float32 (y - Players.size / 2)
            let d = dx * dx + dy * dy
            if d < float32(Players.size / 2) then Color.White else Color.Transparent))
        |> Array.concat
        |> circle.SetData
        // Yellow mole spritesheet
        yellow <- this.Content.Load<Texture2D>("mole/yellow")

    let loadPlayers() =
        players <- [| Players.create yellow PlayerIndex.One true (Vector2(100f, 100f)) |]

    let loadTilemap() =
        tilemap <- TileMap.create this.Content "maps/grass/pillars"

    member this.Graphics = graphics

    override this.Initialize() =
        initializeGraphics()
        base.Initialize()
    
    override this.LoadContent() =
        sb <- new SpriteBatch(this.GraphicsDevice)        
        loadTextures()
        loadPlayers()
        loadTilemap()
        base.LoadContent()

    override this.Update(gametime) = 
        tilemap <- TileMap.update gametime tilemap
        players <- players 
            |> Seq.map (fun p -> p, Players.getInput p)
            |> Seq.map (Players.update gametime)
            |> Array.ofSeq
        camera.LookAt(players.[0].position)
        match Keyboard.GetState() with
        | KeyDown Keys.F11 -> graphics.IsFullScreen <- not graphics.IsFullScreen; initializeGraphics()
        | KeyDown Keys.Add -> scale <- scale + 1f; initializeGraphics()
        | KeyDown Keys.Subtract -> scale <- scale - 1f; initializeGraphics()
        | KeyDown Keys.Escape -> this.Exit()
        | _ -> ()
        base.Update(gametime)

    override this.Draw(gametime) =        
        // First, we are drawing everything to the render target 
        this.GraphicsDevice.SetRenderTarget rt
        this.GraphicsDevice.Clear bg
        let transform = camera.GetViewMatrix()
        sb.Begin(transformMatrix=transform)
        TileMap.draw sb tilemap 
        Seq.iter (Players.draw sb pixel) players
        sb.End()
        this.GraphicsDevice.SetRenderTarget null
        // Then we draw the render target to the screen
        let position = Vector2(float32 this.GraphicsDevice.Viewport.Width, float32 this.GraphicsDevice.Viewport.Height) / 2f
        let origin = Vector2(float32 rt.Width, float32 rt.Height) / 2f
        let rect = Nullable<Rectangle>()
        sb.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise)
        sb.Draw(rt, position, rect, Color.White, 0f, origin, scale, SpriteEffects.None, 1f)
        sb.End()
        base.Draw(gametime)