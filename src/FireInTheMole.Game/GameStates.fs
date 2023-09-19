﻿namespace FireInTheMole.Game

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

    type MenuData = 
        {
            title: string
            titleFont: SpriteFont
            previousInput: KeyboardState
            currentSelection: int
            items: string array
            itemFont: SpriteFont
        }

    type GameState = 
        | Splash
        | Paused of GameStateData * MenuData
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
        let menu = 
            { 
                title = GAME_TITLE
                titleFont = Fonts.title
                previousInput = Keyboard.GetState()
                currentSelection = 0
                items = [| RESUME_GAME; QUIT_GAME |]
                itemFont = Fonts.menu
            }
        Paused(game, menu)

    let createGameState input players map = 
        let game = 
            { 
                previousInput = input
                players = players
                tileMap = map
            }
        Game game

    let updatePauseMenu (game : GameStateData) menu =
        let currentKs = Keyboard.GetState()
        let updatedGame = { game with previousInput = currentKs }
        match (menu.previousInput, currentKs) with
        | KeyPressed Keys.Escape -> Game updatedGame
        | KeyPressed Keys.Up -> 
            if menu.items.[1] = QUIT_GAME_CONFIRMATION then menu.items.[1] <- QUIT_GAME
            Sounds.click 2
            let currentSelection = menu.currentSelection - 1
            let currentSelection = if currentSelection < 0 then menu.items.Length - 1 else currentSelection
            Paused(updatedGame, { menu with previousInput = currentKs; currentSelection = currentSelection })
        | KeyPressed Keys.Down -> 
            if menu.items.[1] = QUIT_GAME_CONFIRMATION then menu.items.[1] <- QUIT_GAME
            Sounds.click 1
            let currentSelection = menu.currentSelection + 1
            let currentSelection = if currentSelection >= menu.items.Length then 0 else currentSelection
            Paused(updatedGame, { menu with previousInput = currentKs; currentSelection = currentSelection })
        | KeyPressed Keys.Enter ->
            match menu.currentSelection with
            | 0 -> Game updatedGame
            | 1 -> 
                Sounds.randomClick()
                if menu.items.[1] = QUIT_GAME then
                    menu.items.[1] <- QUIT_GAME_CONFIRMATION
                    Paused (updatedGame, { menu with previousInput = currentKs })
                else
                    Quit
            | _ -> failwith PAUSE_MENU_UPDATE_ERROR
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

    let drawPauseMenu pixel (menu : MenuData) (sb : SpriteBatch) = 
        let background = Rectangle(0, 0, sb.GraphicsDevice.Viewport.Width, sb.GraphicsDevice.Viewport.Height)
        let titleHeight = menu.titleFont.MeasureString("title").Y
        let itemHeight = menu.itemFont.MeasureString("item").Y
        drawRectangle sb pixel background Color.LightSlateGray
        sb.DrawString(menu.titleFont, menu.title, Vector2(100f, 100f), Color.DarkSlateBlue)
        let drawItem idx (item : string) =
            let color = if idx = menu.currentSelection then Color.White else Color.DarkSlateBlue
            let position = Vector2(100f, 100f + titleHeight + (itemHeight * float32 idx))
            sb.DrawString(menu.itemFont, item, position, color)
        Seq.iteri drawItem menu.items

    let drawGame pixel (game : GameStateData) (sb : SpriteBatch)  =
        //TileMap.draw sb game.tileMap 
        //Seq.iter (Players.draw sb pixel) game.players
        let player1 = game.players.[0]
        Projection.project options player1
        |> Seq.iter (Projection.draw sb)

    let draw drawWithCamera drawWithoutCamera pixel gameState = 
        match gameState with
        | Game game -> drawWithoutCamera(drawGame pixel game)
        | Paused (_, menu) -> drawWithoutCamera(drawPauseMenu pixel menu)
        | _ -> failwith GAMESTATES_DRAW_ERROR