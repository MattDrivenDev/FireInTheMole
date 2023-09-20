namespace FireInTheMole.Game

open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content


module UI = 

    [<Literal>] 
    let STROBE_SPEED = 3.5f

    let mutable sliderTextures = Unchecked.defaultof<Texture2D * Texture2D>

    type SliderData = 
        {
            position: Vector2
            size: Vector2
            previousInput: KeyboardState
            minValue: float32
            maxValue: float32
            step: float32
            value: float32
            barTexture: Texture2D
            sliderTexture: Texture2D
            active: bool
        }

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
    
    let loadTextures (content : ContentManager) = 
        sliderTextures <- content.Load<Texture2D>("UI/sliderbar"), content.Load<Texture2D>("sliderbutton")

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

    let slider position size = 
        {
            position = position
            size = size
            previousInput = Keyboard.GetState()
            minValue = 0f
            maxValue = 1f
            step = 0.1f
            value = 0.5f
            barTexture = (fst sliderTextures)
            sliderTexture = (snd sliderTextures)
            active = false
        }

    let drawSlider (sb : SpriteBatch) (gt : GameTime) (slider : SliderData) = 
        let barPosition = slider.position
        sb.Draw(slider.barTexture, barPosition, Color.White)
        let sliderHalfSize = Vector2(float32 slider.sliderTexture.Width / 2f, float32 slider.sliderTexture.Height / 2f)
        let sliderx = (barPosition.X + slider.size.X * slider.value) - sliderHalfSize.X
        let slidery = barPosition.Y - sliderHalfSize.Y
        let sliderPosition = Vector2(sliderx, slidery)
        sb.Draw(slider.sliderTexture, sliderPosition, Color.White)

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
                if n = menu.selectedItem then strobeColor gt (fst menu.itemColors) (snd menu.itemColors) 
                else fst menu.itemColors
            sb.DrawString(menu.itemFont, item, itemPosition, color)
        sb.DrawString(menu.titleFont, menu.title, titlePosition, menu.titleColor)
        Seq.iteri drawItem menu.items