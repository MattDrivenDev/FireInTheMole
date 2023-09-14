﻿namespace FireInTheMole.Game

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open MonoGame.Extended
open MonoGame.Extended.ViewportAdapters


module Engine = 
    ()

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
    let mutable camera = Unchecked.defaultof<OrthographicCamera>
    let mutable gameState = Unchecked.defaultof<GameStates.GameState>

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
        camera.Zoom <- scale / 2f

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

    let loadPlayers (tilemap : TileMap.TileMap) =
        let maxLength = MathF.Max(float32 tilemap.width, float32 tilemap.height) |> int
        let options = RayCasting.createOptions 90f RayCasting.MaxRayCount maxLength true
        [| Players.create options tilemap yellow PlayerIndex.One true (Vector2(100f, 100f)) |]

    let loadTilemap() =
        TileMap.create this.Content "maps/grass/pillars"

    let loadFonts() = Fonts.loadFonts this.Content

    member this.Graphics = graphics

    override this.Initialize() =
        initializeGraphics()
        base.Initialize()
    
    override this.LoadContent() =
        sb <- new SpriteBatch(this.GraphicsDevice)    
        let ks = Keyboard.GetState();
        loadTextures()
        let tileMap = loadTilemap()
        let players = loadPlayers tileMap
        loadFonts()
        gameState <- GameStates.createGameState ks players tileMap
        base.LoadContent()

    override this.Update(gametime) =
        gameState <- GameStates.update gametime gameState camera
        if gameState = GameStates.Quit then this.Exit()        
        match Keyboard.GetState() with
        | KeyDown Keys.F11 -> graphics.IsFullScreen <- not graphics.IsFullScreen; initializeGraphics()
        | KeyDown Keys.Add -> scale <- scale + 1f; initializeGraphics()
        | KeyDown Keys.Subtract -> scale <- scale - 1f; initializeGraphics()
        | _ -> ()        
        base.Update(gametime)

    override this.Draw(gameTime) = 
        let drawViaRenderTarget drawf =
            this.GraphicsDevice.SetRenderTarget rt
            let transform = camera.GetViewMatrix()
            sb.Begin(transformMatrix=transform)
            drawf sb
            sb.End()
            this.GraphicsDevice.SetRenderTarget null
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
        let drawToScreen drawf = 
            sb.Begin()
            drawf sb
            sb.End()
        GameStates.draw drawViaRenderTarget drawToScreen pixel gameState
        base.Draw(gameTime)