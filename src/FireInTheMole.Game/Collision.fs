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

    type RigidBody = 
        {
            bounds: Bounds
            velocity: Vector2
        }
        member this.position = 
            match this.bounds with
            | Rectangle r -> r.position
            | Circle c -> c.position

    type Contact = {
        rigidBody1: RigidBody
        rigidBody2: RigidBody
        normal: Vector2
        distance: float32
        impulse: float32
    }

    let createRectangle position size velocity = { 
        bounds = Rectangle { position = position; size = size }
        velocity = velocity
    }

    let createCircle position radius velocity = { 
        bounds = Circle { position = position; radius = radius }
        velocity = velocity
    }

    let createContact a b normal distance impulse = {
        rigidBody1 = a
        rigidBody2 = b
        normal = normal
        distance = distance
        impulse = Option.defaultValue 0f impulse
    }

    let updatePosition (rigidBody : RigidBody) = 
        match rigidBody.bounds with
        | Rectangle r -> { rigidBody with bounds = Rectangle { r with position = r.position + rigidBody.velocity } }
        | Circle c -> { rigidBody with bounds = Circle { c with position = c.position + rigidBody.velocity } }

    let update (rigidBody : RigidBody) (velocity : Vector2) = 
        { rigidBody with velocity = velocity }

    let rec collision (rigidBody1 : RigidBody) (rigidBody2 : RigidBody) =
        let collides = 
            match rigidBody1.bounds, rigidBody2.bounds with
            | Rectangle r1, Rectangle r2 -> 
                let min1 = r1.position
                let max1 = r1.position + r1.size
                let min2 = r2.position
                let max2 = r2.position + r2.size
                min1.X <= max2.X && max1.X >= min2.X && min1.Y <= max2.Y && max1.Y >= min2.Y
            | Circle c1, Circle c2 ->
                let distance = Vector2.Distance(c1.position, c2.position)
                distance <= c1.radius + c2.radius
            | Rectangle r, Circle c ->
                let min = r.position
                let max = r.position + r.size
                let closest = Vector2.Clamp(c.position, min, max)
                let distance = Vector2.Distance(c.position, closest)
                distance <= c.radius
            | Circle c, Rectangle r ->
                let min = r.position
                let max = r.position + r.size
                let closest = Vector2.Clamp(c.position, min, max)
                let distance = Vector2.Distance(c.position, closest)
                distance <= c.radius                
        if collides then
            let normal = Vector2.Normalize(rigidBody2.position - rigidBody1.position)
            let distance = Vector2.Distance(rigidBody1.position, rigidBody2.position)
            let relativeVelocity = rigidBody2.velocity - rigidBody1.velocity
            let impulse = Some(Vector2.Dot(relativeVelocity, normal))
            Some(createContact rigidBody1 rigidBody2 normal distance impulse)
        else
            None

    let broadphase (min : Vector2) (max : Vector2) (rigidBody : RigidBody) = 
        match rigidBody.bounds with
        | Rectangle r -> 
            let min' = r.position
            let max' = r.position + r.size
            min.X <= max'.X && max.X >= min'.X && min.Y <= max'.Y && max.Y >= min'.Y
        | Circle c ->
            let min' = c.position - Vector2.One * c.radius
            let max' = c.position + Vector2.One * c.radius
            min.X <= max'.X && max.X >= min'.X && min.Y <= max'.Y && max.Y >= min'.Y

    let collisions (rigidBodies : RigidBody seq) (rigidBody : RigidBody) = 
        let predictedPosition = rigidBody.position + rigidBody.velocity
        let mutable min, max = Vector2.Min(rigidBody.position, predictedPosition), Vector2.Max(rigidBody.position, predictedPosition)
        // Expand the bounds by the half-extents
        match rigidBody.bounds with
        | Rectangle r -> 
            min <- min - (r.size / 2f)
            max <- max + (r.size / 2f)
        | Circle c ->
            min <- min - Vector2.One * c.radius
            max <- max + Vector2.One * c.radius
        // Add a little bit of padding to the bounds
        min <- min - Vector2.One
        max <- max + Vector2.One
        // Check for collisions
        rigidBodies
        |> Seq.filter (broadphase min max)
        |> Seq.choose (collision rigidBody) 

    let resolve (rigidBody : RigidBody) (contacts : Contact seq) =
        // Adjust the rigidBody's velocity based on the contacts
        let velocity = 
            contacts
            |> Seq.fold (fun velocity contact -> velocity + (contact.normal * contact.impulse)) rigidBody.velocity
        // Return the updated rigidBody
        { rigidBody with velocity = velocity}

    /// For debug purposes it is quite nice to be able to draw the bounds of a rigid body
    let draw (sb : SpriteBatch) (tx : Texture2D) (rigidBody: RigidBody) (color : Color) = 
        match rigidBody.bounds with
        | Rectangle r -> 
            let rect = new Rectangle(int32 r.position.X, int32 r.position.Y, int32 r.size.X, int32 r.size.Y)
            drawHollowRectangle sb tx rect 2 color  
        | Circle c -> 
            drawHollowCircle sb c.position c.radius 2 color

    ()