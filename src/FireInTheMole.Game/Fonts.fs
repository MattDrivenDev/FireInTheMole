namespace FireInTheMole.Game

open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics

module Fonts = 

    let mutable title = Unchecked.defaultof<SpriteFont>
    let mutable menu = Unchecked.defaultof<SpriteFont>

    let loadFont (content : ContentManager) name = 
        content.Load<SpriteFont>(sprintf "Fonts/%s" name)
    
    let loadFonts (content : ContentManager) = 
        title <- loadFont content "title"
        menu <- loadFont content "text"