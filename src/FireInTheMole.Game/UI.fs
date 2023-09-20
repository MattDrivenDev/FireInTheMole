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
            speed: float32
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
            items: string array
            itemFont: SpriteFont
            itemColors: Color * Color
            selectedItem: int
        }
    and TextAlignment = 
        | Left
        | Center
        | Right
    
    let loadTextures gd (content : ContentManager) =         
        sliderTextures <- Helpers.createPixelTexture2D(gd), Helpers.createPixelTexture2D(gd)

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
            speed = 1f
            value = 0.5f
            barTexture = (fst sliderTextures)
            sliderTexture = (snd sliderTextures)
            colors = (Color.DarkSlateBlue, Color.Yellow)
            active = false
        }

    let toggleSliderActive (slider : SliderData) = 
        { slider with active = not slider.active }

    let updateSlider (gt : GameTime) (ks : KeyboardState) (slider : SliderData) = 
        let value = 
            if ks.IsKeyDown(Keys.Left) then slider.value - (slider.speed * float32 gt.ElapsedGameTime.TotalSeconds)
            elif ks.IsKeyDown(Keys.Right) then slider.value + (slider.speed * float32 gt.ElapsedGameTime.TotalSeconds)
            else slider.value
        let clamped = MathHelper.Clamp(value, slider.minValue, slider.maxValue)
        { slider with value = clamped; previousInput = ks }

    let drawSlider (sb : SpriteBatch) (gt : GameTime) (slider : SliderData) = 
        let barSize = Vector2(slider.size.X, slider.size.Y / 4f)
        let barHalfSize = barSize / 2f
        let barPosition = slider.position - Vector2(0f, barHalfSize.Y)
        let sliderSize = Vector2((slider.size.X / 100f) * 5f, slider.size.Y)
        let sliderHalfSize = sliderSize / 2f
        let sliderx = (slider.position.X + slider.size.X * slider.value) - sliderHalfSize.X
        let slidery = slider.position.Y - sliderHalfSize.Y
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