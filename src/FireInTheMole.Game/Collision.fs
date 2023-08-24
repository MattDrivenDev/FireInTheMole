namespace FireInTheMole.Game
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics


[<RequireQualifiedAccess>]
module Collision =

    type BoundingRectangle = {
        position: Vector2
        size: Vector2
    }

    type BoundingCircle = {
        position: Vector2
        radius: float32
    }

    type Bounds =
        | Rectangle of BoundingRectangle
        | Circle of BoundingCircle

    type RigidBody = {
        bounds: Bounds
        velocity: Vector2
    }

    /// For debug purposes it is quite nice to be able to draw the bounds of a rigid body
    let draw (sb : SpriteBatch) (tx : Texture2D) (rigidBody: RigidBody) (color : Color) = 
        match rigidBody.bounds with
        | Rectangle r -> 
            let rect = new Rectangle(int32 r.position.X, int32 r.position.Y, int32 r.size.X, int32 r.size.Y)
            sb.Draw(tx, rect, Nullable<Rectangle>(), color, 0f, Vector2.Zero, SpriteEffects.None, 0f)
        | Circle c -> 
            let rect = new Rectangle(int32 c.position.X, int32 c.position.Y, int32 c.radius, int32 c.radius)
            sb.Draw(tx, rect, Nullable<Rectangle>(), color, 0f, Vector2.Zero, SpriteEffects.None, 0f)

    let update (gametime: GameTime) (rigidBody: RigidBody) = 
        rigidBody

    ()