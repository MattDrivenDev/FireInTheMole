namespace FireInTheMole.Game

open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Content

module Sounds =
    
    let mutable private clickSfxs = Unchecked.defaultof<SoundEffect array>
    let mutable private selectSfx = Unchecked.defaultof<SoundEffect>

    let private loadSound (content : ContentManager) name = 
        content.Load<SoundEffect>(sprintf "%s/%s" CONTENT_SOUNDS_UI name)

    let loadSounds (content : ContentManager) =
        selectSfx <- loadSound content "coin1"
        clickSfxs <- 
            [| 
                loadSound content "click1" 
                loadSound content "click2"
                loadSound content "click3"
                loadSound content "click4"
                loadSound content "click5"
            |]

    let click n = 
        clickSfxs.[n].Play(0.3f, 1f, 0.5f) |> ignore

    let select() =
        selectSfx.Play(0.3f, 1f, 0.5f) |> ignore

    let randomClick() = 
        let random = System.Random()
        let index = random.Next(0, clickSfxs.Length)
        click index