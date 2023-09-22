namespace FireInTheMole.Game

open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open System


module GameState = 

    type GameState = 
        | Splash
        | Paused
        | Game
        | Quit

    module GameScreen = 

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
    
        let mutable cachedKs = Keyboard.GetState()

        let mutable tileMap = Unchecked.defaultof<TileMap.TileMap>

        let mutable players = Unchecked.defaultof<Players.Player array>

        let load loadPlayers loadTileMap = 
            tileMap <- loadTileMap()
            players <- loadPlayers()
            Game

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
                players <- players 
                    |> Seq.map (fun p -> p, Players.getInput p)
                    |> Seq.map (Players.update gt tileMap)
                    |> Array.ofSeq
                Game

        let draw sb =
            let player1 = players.[0]
            Projection.project options player1
            |> Seq.iter (Projection.draw sb)

    /// This another experiment to see if a different coding-style would be more
    /// suited for this project, since the immutable types being passed around
    /// is also now getting quite cumbersome to work with.
    module PauseMenu = 

        [<Literal>]
        let POSITION_X = 320f
        
        [<Literal>]
        let POSITION_Y = 180f
        
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

    let update gt gameState camera = 
        match gameState with
        | Splash -> gameState
        | Paused  -> PauseMenu.update gt
        | Game  -> GameScreen.update gt
        | Quit -> Quit

    let draw drawWithCamera drawWithoutCamera gt pixel gameState = 
        match gameState with
        | Splash -> ()
        | Game -> drawWithoutCamera GameScreen.draw
        | Paused -> drawWithoutCamera (PauseMenu.draw gt)
        | Quit -> ()