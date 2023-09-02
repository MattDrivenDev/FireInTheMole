namespace FireInTheMole.Game
open System
open System.Diagnostics
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics


[<AutoOpen>]
module Helpers = 

    let debug s =
        Debug.WriteLine s

    let (|KeyDown|_|) key (ks : KeyboardState) = 
        if ks.IsKeyDown(key) then Some() else None

    let normAngle angle = 
        let angle = angle % 360f
        if angle < 0f then angle + 360f else angle

    let normVector2 (vector : Vector2) = 
        if vector = Vector2.Zero then vector else Vector2.Normalize(vector)

    let toRadians angle = 
        MathHelper.ToRadians(angle)

    let toDegrees angle =
        MathHelper.ToDegrees(angle)

    let cos angle =
        MathF.Cos(angle)
        
    let sin angle =
        MathF.Sin(angle)

    let createPixelTexture2D (gd : GraphicsDevice) =
        let pixel = new Texture2D(gd, 1, 1, false, SurfaceFormat.Color)
        pixel.SetData([| Color.White |])
        pixel

    let createCircleTexture2D (gd : GraphicsDevice) (radius : float32) =
        let diameter = int32 (radius * 2f)
        let center = int32 radius
        let circle = new Texture2D(gd, diameter, diameter, false, SurfaceFormat.Color)
        let colors = Array.init (diameter * diameter) (fun i -> Color.Transparent)
        for x in 0 .. diameter - 1 do
            for y in 0 .. diameter - 1 do
                let distance = Vector2.Distance(new Vector2(float32 x, float32 y), new Vector2(float32 center, float32 center))
                if distance <= radius then
                    colors.[x + y * diameter] <- Color.White
        circle.SetData colors
        circle

    let createHollowCircleTexture2D (gd : GraphicsDevice) (radius : float32) thickness = 
        let diameter = int32 (radius * 2f)
        let center = int32 radius
        let circle = new Texture2D(gd, diameter, diameter, false, SurfaceFormat.Color)
        let colors = Array.init (diameter * diameter) (fun i -> Color.Transparent)
        for x in 0 .. diameter - 1 do
            for y in 0 .. diameter - 1 do
                let distance = Vector2.Distance(new Vector2(float32 x, float32 y), new Vector2(float32 center, float32 center))
                if distance <= radius && distance >= radius - float32 thickness then
                    colors.[x + y * diameter] <- Color.White
        circle.SetData colors
        circle

    let drawLine (sb : SpriteBatch) (tx : Texture2D) (start : Vector2) (finish : Vector2) thickness (color : Color) =
        let edge = finish - start
        let rotation = MathF.Atan2(edge.Y, edge.X)
        let scale = new Vector2(edge.Length(), float32 thickness)
        sb.Draw(tx, start, Nullable<Rectangle>(), color, rotation, Vector2.Zero, scale, SpriteEffects.None, 0f)

    let drawRectangle (sb : SpriteBatch) (tx : Texture2D) (rect : Rectangle) (color : Color) =
        sb.Draw(tx, rect, color)

    let drawHollowRectangle (sb : SpriteBatch) (tx : Texture2D) (rect : Rectangle) (thickness : int32) (color : Color) =
        let top = new Rectangle(rect.X, rect.Y, rect.Width, thickness)
        let bottom = new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness)
        let left = new Rectangle(rect.X, rect.Y, thickness, rect.Height)
        let right = new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height)
        drawRectangle sb tx top color
        drawRectangle sb tx bottom color
        drawRectangle sb tx left color
        drawRectangle sb tx right color

    let drawCircle (sb : SpriteBatch) (center : Vector2) (radius : float32) (color : Color) =
        let tx = createCircleTexture2D sb.GraphicsDevice radius
        sb.Draw(tx, center, Nullable<Rectangle>(), color, 0f, new Vector2(radius), 1f, SpriteEffects.None, 0f)

    let drawHollowCircle (sb : SpriteBatch) (center : Vector2) (radius : float32) (thickness : int32) (color : Color) =
        let tx = createHollowCircleTexture2D sb.GraphicsDevice radius 2
        sb.Draw(tx, center, Nullable<Rectangle>(), color, 0f, new Vector2(radius), 1f, SpriteEffects.None, 0f)