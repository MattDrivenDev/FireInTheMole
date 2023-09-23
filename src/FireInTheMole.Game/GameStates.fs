namespace FireInTheMole.Game

open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework

module GameState = 

    type GameState = 
        | Splash
        | Title
        | Paused
        | Game
        | Quit

    module GameScreen = 

        let options : Projection.ProjectionOptions = 
            {
                tileWidth = MAP_TILE_SIZE
                screenDistance = PLAYER_PROJECTION_DISTANCE
                screenHeight = SCREEN_HEIGHT
                screenHalfHeight = SCREEN_HEIGHT_HALF
                screenWidth = SCREEN_WIDTH
                screenHalfWidth = SCREEN_WIDTH_HALF
                textureMappingWidth = SCREEN_WIDTH / RAY_COUNT
            }
    
        let mutable cachedKs = Keyboard.GetState()

        let mutable tileMap = Unchecked.defaultof<TileMap.TileMap>

        let mutable gamePlayers = Unchecked.defaultof<Players.Player array>

        let load players map = 
            tileMap <- map
            gamePlayers <- players

        let update gt =
            let previousKs = cachedKs
            let ks = Keyboard.GetState()
            cachedKs <- ks
            match previousKs, ks with
            | KeyPressed Keys.Escape ->             
                Sounds.randomClick()
                Paused
            | _ -> 
                tileMap <- TileMap.update gt tileMap
                let mapBounds = TileMap.getCollidableTiles tileMap
                gamePlayers <- gamePlayers 
                    |> Seq.map (fun p -> p, Players.getInput p)
                    |> Seq.map (Players.update gt (tileMap, mapBounds))
                    |> Array.ofSeq
                Game

        let draw sb =
            let player1 = gamePlayers.[0]
            Projection.project options player1
            |> Seq.iter (Projection.draw sb)


    module PauseMenu = 

        let POSITION_X, POSITION_Y = float32 SCREEN_WIDTH_HALF, float32 SCREEN_HEIGHT_HALF
        
        let items = [| RESUME_GAME; MUSIC_VOLUME; QUIT_GAME |]

        let mutable cachedKs = Keyboard.GetState()

        let mutable musicVolumeSlider = UI.slider (Vector2(200f, 20f))

        let mutable selectedItem = 0

        let inline selectItem n = 
            selectedItem <- MathHelper.Clamp(selectedItem + n, 0, items.Length - 1)
            if selectedItem <> 2 then items.[2] <- QUIT_GAME
            Sounds.click selectedItem
            Paused

        let inline resumeGame() = 
            Sounds.randomClick()
            selectedItem <- 0
            items.[2] <- QUIT_GAME
            Game

        let inline activateItem() =
            match items.[selectedItem] with
            | RESUME_GAME -> resumeGame() 
            | QUIT_GAME_CONFIRMATION -> Quit
            | QUIT_GAME -> 
                Sounds.randomClick()
                items.[selectedItem] <- QUIT_GAME_CONFIRMATION
                Paused
            | _ -> Paused

        let update (gt : GameTime) = 
            let previousKs = cachedKs
            let ks = Keyboard.GetState()
            cachedKs <- ks
            if selectedItem = 1 then musicVolumeSlider <- UI.updateSlider ks musicVolumeSlider            
            match previousKs, ks with
            | KeyPressed Keys.Up -> selectItem -1
            | KeyPressed Keys.Down -> selectItem 1
            | KeyPressed Keys.Enter -> activateItem()
            | KeyPressed Keys.Escape -> resumeGame() 
            | _ -> Paused

        let draw (gt : GameTime) (sb : SpriteBatch) =       
            let quitConfirmation = items.[selectedItem] = QUIT_GAME_CONFIRMATION
            // Measure the strings for sizes
            let titleSize = Fonts.title.MeasureString GAME_TITLE
            let resumeSize = Fonts.menu.MeasureString RESUME_GAME
            let musicVolumeSize = Fonts.menu.MeasureString MUSIC_VOLUME
            let quitSize = Fonts.menu.MeasureString (if quitConfirmation then QUIT_GAME_CONFIRMATION else QUIT_GAME)
            //let widestSize = 
            //    [| titleSize; resumeSize; musicVolumeSize; musicVolumeSlider.size; quitSize |]
            //    |> Array.maxBy (fun s -> s.X)
            let totalHeight = titleSize.Y + resumeSize.Y + musicVolumeSize.Y + musicVolumeSlider.size.Y + quitSize.Y
            let totalSize = Vector2(titleSize.X, totalHeight)
            // Calculate the positions so that the menu is centered
            let titlePosition = Vector2(POSITION_X, POSITION_Y) - totalSize / 2f
            let resumePosition = Vector2(POSITION_X, titlePosition.Y) + Vector2(- resumeSize.X / 2f, titleSize.Y)
            let musicVolumePosition = Vector2(POSITION_X, resumePosition.Y) + Vector2(- musicVolumeSize.X / 2f, resumeSize.Y)
            let musicVolumeSliderPosition = Vector2(POSITION_X, musicVolumePosition.Y) + Vector2(- musicVolumeSlider.size.X / 2f, musicVolumeSize.Y)
            let quitGamePosition = Vector2(POSITION_X, musicVolumeSliderPosition.Y) + Vector2(- quitSize.X / 2f, musicVolumeSlider.size.Y)
            // Calculate the colors
            let strobeColor = UI.strobeColor gt Color.Yellow Color.DarkSlateBlue
            let resumeColor = if items.[selectedItem] = RESUME_GAME then strobeColor else Color.DarkSlateBlue
            let musicVolumeColor = if items.[selectedItem] = MUSIC_VOLUME then strobeColor else Color.DarkSlateBlue
            let quitGameColor = 
                if items.[selectedItem] = QUIT_GAME || items.[selectedItem] = QUIT_GAME_CONFIRMATION 
                    then strobeColor 
                    else Color.DarkSlateBlue
            // Draw the strings and other UI elements
            sb.DrawString(Fonts.title, GAME_TITLE, titlePosition, Color.Yellow)
            sb.DrawString(Fonts.menu, RESUME_GAME, resumePosition, resumeColor)
            sb.DrawString(Fonts.menu, MUSIC_VOLUME, musicVolumePosition, musicVolumeColor)
            sb.DrawString(Fonts.menu, (if quitConfirmation then QUIT_GAME_CONFIRMATION else QUIT_GAME), quitGamePosition, quitGameColor)
            UI.drawSlider sb gt musicVolumeSlider musicVolumeSliderPosition


    module TitleMenu =

        let POSITION_X, POSITION_Y = float32 SCREEN_WIDTH_HALF, float32 SCREEN_HEIGHT_HALF
        
        let items = [| SINGLE_PLAYER; MULTIPLAYER; OPTIONS_MENU; VIEW_CREDITS; QUIT_GAME |]

        let mutable cachedKs = Keyboard.GetState()

        let mutable selectedItem = 0

        let inline selectItem n = 
            selectedItem <- MathHelper.Clamp(selectedItem + n, 0, items.Length - 1)
            if selectedItem <> 4 then items.[4] <- QUIT_GAME
            Sounds.click selectedItem
            Title

        let inline activateItem() =
            match items.[selectedItem] with
            | SINGLE_PLAYER -> Game
            | MULTIPLAYER -> Title
            | OPTIONS_MENU -> Title
            | VIEW_CREDITS -> Title
            | QUIT_GAME_CONFIRMATION -> Quit
            | QUIT_GAME -> 
                Sounds.randomClick()
                items.[selectedItem] <- QUIT_GAME_CONFIRMATION
                Title
            | _ -> Title

        let update (gt : GameTime) = 
            let previousKs = cachedKs
            let ks = Keyboard.GetState()
            cachedKs <- ks            
            match previousKs, ks with
            | KeyPressed Keys.Up -> selectItem -1
            | KeyPressed Keys.Down -> selectItem 1
            | KeyPressed Keys.Enter -> activateItem()
            | KeyPressed Keys.Escape ->                 
                selectedItem <- items.Length - 1
                Sounds.click selectedItem
                Title
            | _ -> Title

        let draw gt (sb : SpriteBatch) = 
            let quitConfirmation = items.[selectedItem] = QUIT_GAME_CONFIRMATION
            // Measure the strings for sizes
            let singleplayerSize = Fonts.menu.MeasureString SINGLE_PLAYER
            let multiplayerSize = Fonts.menu.MeasureString MULTIPLAYER
            let optionsSize = Fonts.menu.MeasureString OPTIONS_MENU
            let creditsSize = Fonts.menu.MeasureString VIEW_CREDITS
            let quitSize = Fonts.menu.MeasureString (if quitConfirmation then QUIT_GAME_CONFIRMATION else QUIT_GAME)
            let totalHeight = singleplayerSize.Y + multiplayerSize.Y + optionsSize.Y + creditsSize.Y + quitSize.Y
            // Calculate the positions so that the menu is centered
            let singleplayerPosition = Vector2(POSITION_X - singleplayerSize.X / 2f, POSITION_Y - totalHeight / 2f)
            let multiplayerPosition = Vector2(POSITION_X, singleplayerPosition.Y) + Vector2(- multiplayerSize.X / 2f, singleplayerSize.Y)
            let optionsPosition = Vector2(POSITION_X, multiplayerPosition.Y) + Vector2(- optionsSize.X / 2f, multiplayerSize.Y)
            let creditsPosition = Vector2(POSITION_X, optionsPosition.Y) + Vector2(- creditsSize.X / 2f, optionsSize.Y)
            let quitPosition = Vector2(POSITION_X, creditsPosition.Y) + Vector2(- quitSize.X / 2f, creditsSize.Y)
            // Calculate the colors
            let strobeColor = UI.strobeColor gt Color.Yellow Color.DarkSlateBlue
            let singleplayerColor = if items.[selectedItem] = SINGLE_PLAYER then strobeColor else Color.DarkSlateBlue
            let multiplayerColor = if items.[selectedItem] = MULTIPLAYER then strobeColor else Color.DarkSlateBlue
            let optionsColor = if items.[selectedItem] = OPTIONS_MENU then strobeColor else Color.DarkSlateBlue
            let creditsColor = if items.[selectedItem] = VIEW_CREDITS then strobeColor else Color.DarkSlateBlue
            let quitGameColor = 
                if items.[selectedItem] = QUIT_GAME || items.[selectedItem] = QUIT_GAME_CONFIRMATION 
                    then strobeColor 
                    else Color.DarkSlateBlue
            // Draw the strings and other UI elements
            sb.DrawString(Fonts.menu, SINGLE_PLAYER, singleplayerPosition, singleplayerColor)
            sb.DrawString(Fonts.menu, MULTIPLAYER, multiplayerPosition, multiplayerColor)
            sb.DrawString(Fonts.menu, OPTIONS_MENU, optionsPosition, optionsColor)
            sb.DrawString(Fonts.menu, VIEW_CREDITS, creditsPosition, creditsColor)
            sb.DrawString(Fonts.menu, (if quitConfirmation then QUIT_GAME_CONFIRMATION else QUIT_GAME), quitPosition, quitGameColor)


    let update gt gameState camera = 
        match gameState with
        | Splash -> gameState
        | Title -> TitleMenu.update gt
        | Paused  -> PauseMenu.update gt
        | Game  -> GameScreen.update gt
        | Quit -> Quit

    let draw drawWithCamera drawWithoutCamera gt pixel gameState = 
        match gameState with
        | Splash -> ()
        | Title -> drawWithoutCamera (TitleMenu.draw gt)
        | Game -> drawWithoutCamera GameScreen.draw
        | Paused -> drawWithoutCamera (PauseMenu.draw gt)
        | Quit -> ()