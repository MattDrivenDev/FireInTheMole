namespace FireInTheMole.Game

open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework


module Scenes = 
    
    type GameData = 
        {
            previousInput: KeyboardState
        }

    type PauseMenuData = 
        {
            title: string
            titleFont: SpriteFont
            previousInput: KeyboardState
            currentSelection: int
            items: string array
            itemFont: SpriteFont
        }

    type Scene = 
        | Splash
        | PauseMenu of PauseMenuData
        | Game of GameData
        | Quit

    let createPauseMenuScene() =
        let data = 
            { 
                title = "Fire In The Mole!"
                titleFont = Fonts.title
                previousInput = Keyboard.GetState()
                currentSelection = 0
                items = [| "Resume"; "Quit" |]
                itemFont = Fonts.menu
            }
        PauseMenu data

    let createGameScene() = 
        let data = 
            { 
                previousInput = Keyboard.GetState()
            }
        Game data

    let load scene = 
        match scene with
        | Splash -> ()
        | PauseMenu data -> ()
        | Game data -> ()
        | Quit -> ()

    let updatePauseMenu gameTime data =
        let currentKs = Keyboard.GetState()
        match (data.previousInput, currentKs) with
        | KeyPressed Keys.Escape -> Game { previousInput = currentKs }
        | KeyPressed Keys.Up -> 
            let currentSelection = data.currentSelection - 1
            let currentSelection = if currentSelection < 0 then data.items.Length - 1 else currentSelection
            PauseMenu { data with previousInput = currentKs; currentSelection = currentSelection }
        | KeyPressed Keys.Down -> 
            let currentSelection = data.currentSelection + 1
            let currentSelection = if currentSelection >= data.items.Length then 0 else currentSelection
            PauseMenu { data with previousInput = currentKs; currentSelection = currentSelection }
        | KeyPressed Keys.Enter ->
            match data.currentSelection with
            | 0 -> Game { previousInput = currentKs }
            | 1 -> Quit
            | _ -> failwith "Menu Item Error"
        | _ -> PauseMenu { data with previousInput = currentKs }

    let updateGame gameTime (data : GameData) =
        let currentKs = Keyboard.GetState()
        match (data.previousInput, currentKs) with
        | KeyPressed Keys.Escape -> createPauseMenuScene()
        | _ ->  Game { data with previousInput = currentKs }

    let update gameTime scene = 
        match scene with
        | Splash -> scene
        | PauseMenu data -> updatePauseMenu gameTime data
        | Game data -> updateGame gameTime data
        | Quit -> scene

    let drawPauseMenu (sb : SpriteBatch) pixel (data : PauseMenuData) = 
        let background = Rectangle(0, 0, sb.GraphicsDevice.Viewport.Width, sb.GraphicsDevice.Viewport.Height)
        let titleHeight = data.titleFont.MeasureString("title").Y
        let itemHeight = data.itemFont.MeasureString("item").Y
        drawRectangle sb pixel background Color.LightSlateGray
        sb.DrawString(data.titleFont, data.title, Vector2(100f, 100f), Color.DarkSlateBlue)
        let drawItem idx (item : string) =
            let color = if idx = data.currentSelection then Color.White else Color.DarkSlateBlue
            let position = Vector2(100f, 100f + titleHeight + (itemHeight * float32 idx))
            sb.DrawString(data.itemFont, item, position, color)
        Seq.iteri drawItem data.items

    /// I don't like the pixel but it is useful to get something on the screen
    let draw sb pixel scene = 
        match scene with
        | Splash -> ()
        | PauseMenu data -> drawPauseMenu sb pixel data
        | Game data -> ()
        | Quit -> ()