namespace FireInTheMole.Game

open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input


module Scenes = 
    
    type GameData = 
        {
            previousInput: KeyboardState
        }

    type MenuData = 
        {
            title: string
            previousInput: KeyboardState
        }

    type Scene = 
        | Splash
        | Menu of MenuData
        | Game of GameData

    let createPauseMenuScene() =
        let data = 
            { 
                title = "Fire In The Mole!"
                previousInput = Keyboard.GetState()
            }
        Menu data

    let createGameScene() = 
        let data = 
            { 
                previousInput = Keyboard.GetState()
            }
        Game data

    let load scene = 
        match scene with
        | Splash -> ()
        | Menu data -> ()
        | Game data -> ()

    let updateMenu gameTime data =
        let currentKs = Keyboard.GetState()
        match (data.previousInput, currentKs) with
        | KeyPressed Keys.Escape ->  
            Game { previousInput = currentKs }
        | _ -> 
            Menu { data with previousInput = currentKs }

    let updateGame gameTime (data : GameData) =
        let currentKs = Keyboard.GetState()
        match (data.previousInput, currentKs) with
        | KeyPressed Keys.Escape -> 
            createPauseMenuScene()
        | _ -> 
            Game { data with previousInput = currentKs }

    let update gameTime scene = 
        match scene with
        | Splash -> scene
        | Menu data -> updateMenu gameTime data
        | Game data -> updateGame gameTime data

    let drawMenu (gd : GraphicsDevice) (data : MenuData) =         
        gd.Clear Color.Yellow

    let draw gd scene = 
        match scene with
        | Splash -> ()
        | Menu data -> drawMenu gd data
        | Game data -> ()