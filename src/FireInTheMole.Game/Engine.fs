﻿namespace FireInTheMole.Game

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
    let mutable camera = Unchecked.defaultof<OrthographicCamera>
    let mutable previousState = Unchecked.defaultof<GameState.GameState>
    let mutable gameState = Unchecked.defaultof<GameState.GameState>

    do
        this.Content.RootDirectory <- "Content"
        graphics.GraphicsProfile <- GraphicsProfile.HiDef
        graphics.IsFullScreen <- FULLSCREEN

    let initializeGraphics() =
        scale <- MathHelper.Clamp(scale, 1f, 2f)
        let renderWidth = SCREEN_WIDTH * int scale
        let renderHeight = SCREEN_HEIGHT * int scale
        graphics.PreferredBackBufferWidth <- renderWidth
        graphics.PreferredBackBufferHeight <- renderHeight
        graphics.ApplyChanges()
        rt <- new RenderTarget2D(
            this.GraphicsDevice,
            SCREEN_WIDTH,
            SCREEN_HEIGHT,
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
        circle <- new Texture2D(this.GraphicsDevice, PLAYER_SIZE, PLAYER_SIZE, false, SurfaceFormat.Color)
        Array.init PLAYER_SIZE (fun y -> Array.init PLAYER_SIZE (fun x -> 
            let dx = float32 (x - PLAYER_SIZE / 2)
            let dy = float32 (y - PLAYER_SIZE / 2)
            let d = dx * dx + dy * dy
            if d < float32(PLAYER_SIZE / 2) then Color.White else Color.Transparent))
        |> Array.concat
        |> circle.SetData
        // Yellow mole spritesheet
        yellow <- this.Content.Load<Texture2D>("mole/yellow")

    let loadPlayers (tilemap : TileMap.TileMap) =
        let maxLength = MathF.Max(float32 tilemap.width, float32 tilemap.height) |> int
        let options = RayCasting.createOptions 90f RAY_COUNT maxLength CORRECT_FISHEYE
        [| 
            Players.create options tilemap yellow PlayerIndex.One true 
            Players.create options tilemap yellow PlayerIndex.Two true 
            Players.create options tilemap yellow PlayerIndex.Three true 
            Players.create options tilemap yellow PlayerIndex.Four true 
        |]

    let loadTilemap() =
        TileMap.create this.Content "maps/snow/bigempty"

    let loadFonts() = Fonts.loadFonts this.Content

    let loadSounds() = Sounds.loadSounds this.Content

    let loadUI() = UI.loadTextures this.GraphicsDevice this.Content

    member this.Graphics = graphics

    override this.Initialize() =
        initializeGraphics()
        base.Initialize()
    
    override this.LoadContent() =
        sb <- new SpriteBatch(this.GraphicsDevice)    
        let ks = Keyboard.GetState();
        loadTextures()
        loadUI()
        loadFonts()
        loadSounds()
        let tileMap = loadTilemap()
        let players = loadPlayers tileMap
        GameState.GameScreen.load players tileMap 
        gameState <- GameState.Title
        base.LoadContent()

    override this.Update(gametime) =
        previousState <- gameState
        gameState <- GameState.update gametime gameState camera
        if gameState = GameState.Quit then this.Exit()         
        base.Update(gametime)

    override this.Draw(gameTime) = 
        // First, clear the screen to the background color
        this.GraphicsDevice.Clear(bg)
        // Setup a functions to draw with and without the camera
        let drawWithCamera drawf =
            // Set the render target used for resolution scaling
            this.GraphicsDevice.SetRenderTarget rt
            let transform = camera.GetViewMatrix()
            sb.Begin(transformMatrix=transform)
            drawf sb
            sb.End()      
            // Set the render target back to the screen
            this.GraphicsDevice.SetRenderTarget null    
            // Draw the render target to the screen with the correct scale
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
        let drawWithoutCamera drawf = 
            sb.Begin(SpriteSortMode.Deferred,
                     BlendState.AlphaBlend,
                     SamplerState.PointClamp,
                     DepthStencilState.None,
                     RasterizerState.CullCounterClockwise)
            drawf sb
            sb.End()

        // Draw the game state to the render target using the camera/no-camera functions
        let stateToDraw = if previousState = gameState then gameState else previousState
        GameState.draw drawWithCamera drawWithoutCamera gameTime pixel stateToDraw
        
        // Done.
        base.Draw(gameTime)