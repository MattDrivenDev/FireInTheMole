namespace FireInTheMole.Game

open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended
open System


module GameStates = 
   
    type GameStateData = 
        {
            previousInput: KeyboardState
            players: Players.Player array
            tileMap: TileMap.TileMap
        }

    type GameState = 
        | Splash
        | Paused of GameStateData * UI.MenuData
        | Game of GameStateData
        | Quit

    let options : Projection.ProjectionOptions = 
        {
            tileWidth = 256
            screenDistance = int (320f / MathF.Tan(MathHelper.ToRadians(45f)))
            screenHeight = 360
            screenHalfHeight = 180
            screenWidth = 640
            screenHalfWidth = 320
            textureMappingWidth = 640 / RayCasting.MaxRayCount
        }

    let createPauseMenuState game =
        Paused(game, UI.pauseMenu())

    let createGameState input players map = 
        let game = 
            { 
                previousInput = input
                players = players
                tileMap = map
            }
        Game game

    let updatePauseMenu (game : GameStateData) (menu : UI.MenuData) =
        let currentKs = Keyboard.GetState()
        let updatedGame = { game with previousInput = currentKs }        
        match menu.items.[menu.selectedItem] with
        | UI.Slider(s, slider) -> 
            let updatedSlider = UI.updateSlider currentKs slider
            menu.items.[menu.selectedItem] <- UI.Slider(s, updatedSlider)    
        | _ -> ()
        match (menu.previousInput, currentKs) with
        | KeyPressed Keys.Escape -> Game updatedGame
        | KeyPressed Keys.Up -> 
            if menu.items.[menu.selectedItem] = UI.Simple QUIT_GAME_CONFIRMATION then menu.items.[menu.selectedItem] <- UI.Simple QUIT_GAME
            Sounds.click 2
            let currentSelection = menu.selectedItem - 1
            let currentSelection = if currentSelection < 0 then menu.items.Length - 1 else currentSelection
            Paused(updatedGame, { menu with previousInput = currentKs; selectedItem = currentSelection })
        | KeyPressed Keys.Down -> 
            if menu.items.[menu.selectedItem] = UI.Simple QUIT_GAME_CONFIRMATION then menu.items.[menu.selectedItem] <- UI.Simple QUIT_GAME
            Sounds.click 1
            let currentSelection = menu.selectedItem + 1
            let currentSelection = if currentSelection >= menu.items.Length then 0 else currentSelection
            Paused(updatedGame, { menu with previousInput = currentKs; selectedItem = currentSelection })
        | KeyPressed Keys.Enter ->
            match menu.items.[menu.selectedItem] with
            | UI.Simple RESUME_GAME -> 
                Sounds.randomClick()
                Game updatedGame
            | UI.Simple QUIT_GAME -> 
                Sounds.randomClick()
                menu.items.[menu.selectedItem] <- UI.Simple QUIT_GAME_CONFIRMATION
                Paused (updatedGame, { menu with previousInput = currentKs })
            | UI.Simple QUIT_GAME_CONFIRMATION -> 
                Sounds.randomClick()
                Quit
            | _ -> Paused(updatedGame, { menu with previousInput = currentKs; })
        | _ -> Paused(updatedGame, { menu with previousInput = currentKs })

    let updateGame gameTime (game : GameStateData) (camera : OrthographicCamera) =
        let currentKs = Keyboard.GetState()
        let innerUpdate() =
            camera.LookAt(game.players.[0].position)
            let updatedGame = 
                { game with 
                    previousInput = currentKs 
                    tileMap = TileMap.update gameTime game.tileMap
                    players = 
                        game.players 
                        |> Seq.map (fun p -> p, Players.getInput p)
                        |> Seq.map (Players.update gameTime game.tileMap)
                        |> Array.ofSeq
                }
            Game updatedGame
        match (game.previousInput, currentKs) with
        | KeyPressed Keys.Escape ->             
            Sounds.randomClick()
            createPauseMenuState game
        | _ -> innerUpdate()

    let update gameTime gameState camera = 
        match gameState with
        | Splash -> gameState
        | Paused (game, menu) -> updatePauseMenu game menu
        | Game data -> updateGame gameTime data camera
        | Quit -> gameState

    let drawGame pixel (game : GameStateData) (sb : SpriteBatch)  =
        let player1 = game.players.[0]
        Projection.project options player1
        |> Seq.iter (Projection.draw sb)

    let draw drawWithCamera drawWithoutCamera gt pixel gameState = 
        // A function alias to swap the parameters
        let drawMenu' menu sb = UI.drawMenu sb gt menu
        match gameState with
        | Game game -> drawWithoutCamera(drawGame pixel game)
        | Paused (_, menu) -> drawWithoutCamera(drawMenu' menu)
        | _ -> failwith GAMESTATES_DRAW_ERROR