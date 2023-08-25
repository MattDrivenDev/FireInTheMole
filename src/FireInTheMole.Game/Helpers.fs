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

    let drawLine (sb : SpriteBatch) (tx : Texture2D) (start : Vector2) (finish : Vector2) thickness (color : Color) =
        let edge = finish - start
        let rotation = MathF.Atan2(edge.Y, edge.X)
        let scale = new Vector2(edge.Length(), float32 thickness)
        sb.Draw(tx, start, Nullable<Rectangle>(), color, rotation, Vector2.Zero, scale, SpriteEffects.None, 0f)

    let drawRectangle (sb : SpriteBatch) (tx : Texture2D) (rect : Rectangle) (color : Color) =
        sb.Draw(tx, rect, color)