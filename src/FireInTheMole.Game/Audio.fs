namespace FireInTheMole.Game

open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Content

module Sounds =
    
    let mutable private clicks = Unchecked.defaultof<SoundEffect array>

    let private loadSound (content : ContentManager) name = 
        content.Load<SoundEffect>(sprintf "Sounds/%s" name)

    let loadSounds (content : ContentManager) =
        clicks <- 
            [| 
                loadSound content "UI/click1" 
                loadSound content "UI/click2"
                loadSound content "UI/click3"
                loadSound content "UI/click4"
                loadSound content "UI/click5"
            |]

    let click n = 
        clicks.[n].Play() |> ignore

    let randomClick() = 
        let random = System.Random()
        let index = random.Next(0, clicks.Length)
        click index

module Music = 
    ()