namespace FireInTheMole.Game

open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework


module UI = 

    [<Literal>] 
    let STROBE_SPEED = 3.5f

    type MenuData = 
        {
            position: Vector2
            alignment: TextAlignment
            previousInput: KeyboardState
            background: Texture2D option
            title: string
            titleFont: SpriteFont
            titleColor: Color
            items: string array
            itemFont: SpriteFont
            itemColors: Color * Color
            selectedItem: int
        }
    and TextAlignment = 
        | Left
        | Center
        | Right

    let strobeColor (gt : GameTime) color1 color2 = 
        let t = float32 gt.TotalGameTime.TotalSeconds * STROBE_SPEED
        let tcossquared = (cos t) * (cos t)
        let amount = 0.2f + 0.6f * tcossquared
        Color.Lerp(color1, color2, amount)

    let pauseMenu() = 
       { 
            position = Vector2(320f, 100f)
            alignment = TextAlignment.Center
            previousInput = Keyboard.GetState()
            background = None
            title = GAME_TITLE
            titleFont = Fonts.title
            titleColor = Color.Yellow
            items = [| RESUME_GAME; QUIT_GAME |]
            itemFont = Fonts.menu
            itemColors = (Color.DarkSlateBlue, Color.Yellow)
            selectedItem = 0
        }

    let drawMenu (sb : SpriteBatch) gt (menu : MenuData) =
        let titleSize = menu.titleFont.MeasureString(menu.title)
        let titlePosition = 
            match menu.alignment with
            | Left -> menu.position
            | Center -> Vector2(menu.position.X - titleSize.X / 2f, menu.position.Y)
            | Right -> Vector2(menu.position.X - titleSize.X, menu.position.Y)
        let drawItem n (item : string) =
            let itemSize = menu.itemFont.MeasureString(item)
            let itemPosition = 
                match menu.alignment with
                | Left -> Vector2(titlePosition.X, titlePosition.Y + titleSize.Y + float32 n * itemSize.Y)
                | Center -> Vector2(titlePosition.X + titleSize.X / 2f - itemSize.X / 2f, titlePosition.Y + titleSize.Y + float32 n * itemSize.Y)
                | Right -> Vector2(titlePosition.X + titleSize.X - itemSize.X, titlePosition.Y + titleSize.Y + float32 n * itemSize.Y)
            let color = 
                if n = menu.selectedItem then 
                    strobeColor gt (fst menu.itemColors) (snd menu.itemColors) 
                else 
                    fst menu.itemColors
            sb.DrawString(menu.itemFont, item, itemPosition, color)
        sb.DrawString(menu.titleFont, menu.title, titlePosition, menu.titleColor)
        Seq.iteri drawItem menu.items