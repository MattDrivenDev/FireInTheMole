namespace FireInTheMole.Game

open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Content

module Sounds =
    
    let mutable private clicks = Unchecked.defaultof<SoundEffect array>

    let private loadSound (content : ContentManager) name = 
        content.Load<SoundEffect>(sprintf "%s/%s" CONTENT_SOUNDS_UI name)

    let loadSounds (content : ContentManager) =
        clicks <- 
            [| 
                loadSound content "click1" 
                loadSound content "click2"
                loadSound content "click3"
                loadSound content "click4"
                loadSound content "click5"
            |]

    let click n = 
        clicks.[n].Play() |> ignore

    let randomClick() = 
        let random = System.Random()
        let index = random.Next(0, clicks.Length)
        click index