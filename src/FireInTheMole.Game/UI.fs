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
            size: Vector2
            previousInput: KeyboardState
            minValue: float32
            maxValue: float32
            step: float32
            value: float32
            barTexture: Texture2D
            sliderTexture: Texture2D
            colors: Color * Color
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
            items: MenuItem array
            itemFont: SpriteFont
            itemColors: Color * Color
            selectedItem: int
        }
    and TextAlignment = 
        | Left
        | Center
        | Right
    and MenuItem = 
        | Simple of string
        | Slider of string * SliderData

    let slider size = 
        {
            size = size
            previousInput = Keyboard.GetState()
            minValue = 0f
            maxValue = 1f
            step = 0.1f
            value = 0.5f
            barTexture = (fst sliderTextures)
            sliderTexture = (snd sliderTextures)
            colors = (Color.DarkSlateBlue, Color.Yellow)
            active = false
        }
    
    let loadTextures gd (content : ContentManager) =         
        sliderTextures <- Helpers.createPixelTexture2D(gd), Helpers.createPixelTexture2D(gd)

    let strobeColor (gt : GameTime) color1 color2 = 
        let t = float32 gt.TotalGameTime.TotalSeconds * STROBE_SPEED
        let tcossquared = (cos t) * (cos t)
        let amount = 0.2f + 0.6f * tcossquared
        Color.Lerp(color1, color2, amount)

    let pauseMenu() = 
        let volumeSlider = slider (Vector2(200f, 20f))
        { 
            position = Vector2(320f, 100f)
            alignment = TextAlignment.Center
            previousInput = Keyboard.GetState()
            background = None
            title = GAME_TITLE
            titleFont = Fonts.title
            titleColor = Color.Yellow
            items = 
                [| 
                    Simple RESUME_GAME
                    Slider(MUSIC_VOLUME, volumeSlider)
                    Simple QUIT_GAME 
                |]
            itemFont = Fonts.menu
            itemColors = (Color.DarkSlateBlue, Color.Yellow)
            selectedItem = 0
        }

    let toggleSliderActive (slider : SliderData) = 
        { slider with active = not slider.active }

    let updateSlider (ks : KeyboardState) (slider : SliderData) = 
        let value = 
            match slider.previousInput, ks with
            | KeyPressed Keys.Left -> slider.value - slider.step
            | KeyPressed Keys.Right -> slider.value + slider.step
            | _ -> slider.value
        let clamped = MathHelper.Clamp(value, slider.minValue, slider.maxValue)
        { slider with value = clamped; previousInput = ks }

    let drawSlider (sb : SpriteBatch) (gt : GameTime) (slider : SliderData) position = 
        let barSize = Vector2(slider.size.X, slider.size.Y / 4f)
        let barHalfSize = barSize / 2f
        let barPosition = position - Vector2(0f, barHalfSize.Y)
        let sliderSize = Vector2((slider.size.X / 100f) * 5f, slider.size.Y)
        let sliderHalfSize = sliderSize / 2f
        let sliderx = (position.X + slider.size.X * slider.value) - sliderHalfSize.X
        let slidery = position.Y - sliderHalfSize.Y
        let sliderPosition = Vector2(sliderx, slidery)
        sb.Draw(slider.barTexture, Rectangle(barPosition.ToPoint(), barSize.ToPoint()), fst slider.colors)
        sb.Draw(slider.sliderTexture, Rectangle(sliderPosition.ToPoint(), sliderSize.ToPoint()), snd slider.colors)

    let drawMenu (sb : SpriteBatch) gt (menu : MenuData) =
        let titleSize = menu.titleFont.MeasureString(menu.title)
        let titlePosition = 
            match menu.alignment with
            | Left -> menu.position
            | Center -> Vector2(menu.position.X - titleSize.X / 2f, menu.position.Y)
            | Right -> Vector2(menu.position.X - titleSize.X, menu.position.Y)
        let drawSimpleItem n (item : string) =
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
        let drawItem n (item : MenuItem) =
            match item with
            | Simple s -> drawSimpleItem n s
            | Slider(s, slider) -> 
                drawSimpleItem n s
                let pos = 
                    match menu.alignment with
                    | Left -> Vector2(titlePosition.X, titlePosition.Y + titleSize.Y + float32 n * slider.size.Y)
                    | Center -> Vector2(titlePosition.X + titleSize.X / 2f - slider.size.X / 2f, titlePosition.Y + titleSize.Y + float32 n * slider.size.Y)
                    | Right -> Vector2(titlePosition.X + titleSize.X - slider.size.X, titlePosition.Y + titleSize.Y + float32 n * slider.size.Y)
                drawSlider sb gt slider pos
        sb.DrawString(menu.titleFont, menu.title, titlePosition, menu.titleColor)
        Seq.iteri drawItem menu.items