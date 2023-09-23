namespace FireInTheMole.Game

open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics

module Fonts = 

    let mutable title = Unchecked.defaultof<SpriteFont>
    let mutable menu = Unchecked.defaultof<SpriteFont>

    let private loadFont (content : ContentManager) name = 
        content.Load<SpriteFont>(sprintf "%s/%s" CONTENT_FONTS name)
    
    let loadFonts (content : ContentManager) = 
        title <- loadFont content FONT_TITLE
        menu <- loadFont content FONT_TEXT