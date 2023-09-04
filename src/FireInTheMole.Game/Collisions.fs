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
            b : BoundingRectangle
            position: Vector2
            size: Vector2
            normal: Vector2
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

    let createCollision a b position size normal = 
        { 
            b = b
            position = position
            size = size 
            normal = normal
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
    let predictCollision (a : BoundingRectangle) (b : BoundingRectangle) = 
        let bLeft, bTop, bRight, bBottom = 
            b.predictedMin.X, b.predictedMin.Y, b.predictedMax.X, b.predictedMax.Y
        let aLeft, aTop, aRight, aBottom =
            a.predictedMin.X, a.predictedMin.Y, a.predictedMax.X, a.predictedMax.Y
        let x = Math.Max(aLeft, bLeft)
        let y = Math.Max(aTop, bTop)
        let width = Math.Min(aRight, bRight) - x
        let height = Math.Min(aBottom, bBottom) - y
        if width > 0f && height > 0f then
            if width >= height then
                // Top/Bottom Collision
                if aTop < bTop then Some(createCollision a b (Vector2(x, y)) (Vector2(width, height)) (Vector2.UnitY))
                else Some(createCollision a b (Vector2(x, y)) (Vector2(width, height)) (-Vector2.UnitY))
            else 
                // Side Collision
                if aLeft < bLeft then Some(createCollision a b (Vector2(x, y)) (Vector2(width, height)) (Vector2.UnitX))
                else Some(createCollision a b (Vector2(x, y)) (Vector2(width, height)) (-Vector2.UnitX))
        else None

    /// A filter that determines if two bounding rectangles should be checked for collision.
    let broadphase (a : BoundingRectangle) (b : BoundingRectangle) = 
        true // TODO: Implement broadphase

    /// Predicts collisions between a bounding rectangle and a sequence of bounding rectangles.
    let predictCollisions a bs = 
        bs
        |> Seq.filter (broadphase a)
        |> Seq.choose (predictCollision a)

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

    /// Stops the bounding rectangle from moving against the collision at all.
    let fullStop (boundingRectangle : BoundingRectangle) (collision : Collision) = 
        // TODO: Implement fullStop properly to allow the bounding
        // rectangle to touch the other.
        updateVelocity boundingRectangle Vector2.Zero
        
    /// Resolves the collision by allowing the bounding rectangle to move
    /// against the normal of the collision.
    let slide (boundingRectangle : BoundingRectangle) (collision : Collision) = 
        let diffDistance = 
            match collision.normal with
            | n when n = Vector2.UnitX -> Vector2(collision.size.X, 0f)
            | n when n = -Vector2.UnitX -> Vector2(-collision.size.X, 0f)
            | n when n = Vector2.UnitY -> Vector2(0f, collision.size.Y)
            | n when n = -Vector2.UnitY -> Vector2(0f, -collision.size.Y)
            | _ -> Vector2.Zero     
        let velocity = boundingRectangle.velocity - diffDistance
        updateVelocity boundingRectangle velocity

    /// Resolves the collision contacts by changing the velocity of the bounding rectangle.
    let resolve boundingRectangle collisions = 
        if Seq.isEmpty collisions then boundingRectangle
        else Seq.fold slide boundingRectangle collisions