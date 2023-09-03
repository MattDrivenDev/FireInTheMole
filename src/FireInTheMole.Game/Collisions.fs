namespace FireInTheMole.Game
open System
open Microsoft.Xna.Framework


[<RequireQualifiedAccess>]
module Collisions = 

    /// A bounding rectangle is a rectangle that is used to represent
    /// the bounds of an object. It is used for collision detection.
    type BoundingRectangle = 
        {
            center: Vector2
            size: Vector2
            velocity: Vector2
        }
        member this.halfSize = this.size / 2f
        member this.min = this.center - this.halfSize
        member this.max = this.center + this.halfSize
        member this.toRectangle() = Rectangle(this.min.ToPoint(), this.size.ToPoint())
        member this.predictedCenter = this.center + this.velocity
        member this.predictedMin = this.predictedCenter - this.halfSize
        member this.predictedMax = this.predictedCenter + this.halfSize

    /// A collision is a rectangle that represents the area of intersection
    /// between two bounding rectangles.
    type Collision = 
        {
            position: Vector2
            size: Vector2
        }
        member this.min = this.position
        member this.max = this.position + this.size
        member this.toRectangle() = Rectangle(this.min.ToPoint(), this.size.ToPoint())

    let createBoundingRectangle center size = 
        { 
            center = center
            size = size 
            velocity = Vector2.Zero 
        }

    let createCollision position size = 
        { 
            position = position
            size = size 
        }

    /// For debugging purposes this will draw a bounding rectangle to the screen.
    let drawBoundingRectangle sb tx color thickness (boundingRectangle : BoundingRectangle) =
        let centerPosition = boundingRectangle.center - Vector2(float32 thickness)
        let centerSize = Vector2(float32 thickness * 2f)
        let center = Rectangle(centerPosition.ToPoint(), centerSize.ToPoint())
        drawHollowRectangle sb tx (boundingRectangle.toRectangle()) thickness color
        drawRectangle sb tx center color

    /// Compares the predicted positions of two bounding rectangles and returns 
    /// the contact data if they collide.
    let predictCollisions (a : BoundingRectangle) (b : BoundingRectangle) = 
        let x = Math.Max(a.predictedMin.X, b.predictedMin.X)
        let y = Math.Max(a.predictedMin.Y, b.predictedMin.Y)
        let width = Math.Min(a.predictedMax.X, b.predictedMax.X) - x
        let height = Math.Min(a.predictedMax.Y, b.predictedMax.Y) - y
        if width > 0f && height > 0f
            then Some(createCollision (Vector2(x, y)) (Vector2(width, height)))
            else None

    /// A filter that determines if two bounding rectangles should be checked for collision.
    let broadphase (a : BoundingRectangle) (b : BoundingRectangle) = 
        true

    /// Updates the velocity of the bounding rectangle.
    let updateVelocity (boundingRectangle : BoundingRectangle) (velocity : Vector2) = 
        { 
            boundingRectangle with 
                velocity = velocity
        }

    /// Applies the velocity to the center of the bounding rectangle, 
    /// resetting the velocity to zero.
    let move boundingRectangle = 
        { 
            boundingRectangle with 
                center = boundingRectangle.center + boundingRectangle.velocity
                velocity = Vector2.Zero
        }

    let fullStop (boundingRectangle : BoundingRectangle) (collision : Collision) = 
        updateVelocity boundingRectangle Vector2.Zero

    /// Resolves the collision contacts by changing the velocity of the bounding rectangle.
    let resolve boundingRectangle collisions = 
        if Seq.isEmpty collisions
            then boundingRectangle
            else Seq.fold fullStop boundingRectangle collisions 