namespace FireInTheMole.Game

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics


module Cameras = 

    type Camera = {
        position: Vector2
        rotation: float32
        zoom: float32
        viewport: Viewport
        viewportCenter: Vector2
    }

    let createCamera viewport position rotation zoom = {
        position = position
        rotation = rotation
        zoom = zoom
        viewport = viewport
        viewportCenter = Vector2(float32 viewport.Width / 2f, float32 viewport.Height / 2f)
    }

    let updateCamera camera position rotation zoom = 
        { camera with
            position = position
            rotation = rotation
            zoom = zoom }

    [<RequireQualifiedAccess>]
    module OrthographicCamera =
        
        let worldToScreen camera =
            Matrix.CreateTranslation(Vector3(-camera.position, 0f))
            * Matrix.CreateRotationZ(camera.rotation)
            * Matrix.CreateScale(Vector3(camera.zoom, camera.zoom, 1f))
            * Matrix.CreateTranslation(Vector3(camera.viewportCenter, 0f))

        let screenToWorld camera =
            worldToScreen camera |> Matrix.Invert

    [<RequireQualifiedAccess>]
    module Mode7Camera =
        
        type Frustum = {
            size: Vector2
            upperLeft: Vector2
            upperRight: Vector2
            lowerLeft: Vector2
            lowerRight: Vector2
        }

        let createFrustum size ul ur ll lr = {
            size = size
            upperLeft = ul
            upperRight = ur
            lowerLeft = ll
            lowerRight = lr
        }

        let affineTransform frustum = 
            Matrix(M11 = frustum.upperRight.X - frustum.upperLeft.X,
                   M12 = frustum.upperRight.Y - frustum.upperLeft.Y,
                   M21 = frustum.lowerLeft.X - frustum.upperLeft.X,
                   M22 = frustum.lowerLeft.Y - frustum.upperLeft.Y,
                   M33 = 1f,
                   M41 = frustum.upperLeft.X,
                   M42 = frustum.upperLeft.Y,
                   M44 = 1f)

        let nonAffineTransform frustum (affine : Matrix) =
            let mutable nonAffine = Matrix()
            let den = affine.M11 * affine.M22 - affine.M12 * affine.M21
            let a = (affine.M22 * frustum.lowerRight.X - affine.M21 * frustum.lowerRight.Y +
                     affine.M21 * affine.M42 - affine.M22 * affine.M41) / den
            let b = (affine.M11 * frustum.lowerRight.Y - affine.M12 * frustum.lowerRight.X +
                     affine.M12 * affine.M41 - affine.M11 * affine.M42) / den
            nonAffine.M11 <- a / (a + b - 1f)
            nonAffine.M22 <- b / (a + b - 1f)
            nonAffine.M33 <- 1f
            nonAffine.M14 <- nonAffine.M11 - 1f
            nonAffine.M24 <- nonAffine.M22 - 1f
            nonAffine.M44 <- 1f
            nonAffine

        let mode7Matrix frustum matrix =
            let scale = Matrix.CreateScale(1f / frustum.size.X, 1f / frustum.size.Y, 1f)
            let affine = affineTransform frustum
            let nonAffine = nonAffineTransform frustum affine
            matrix * scale * nonAffine * affine

        let worldToScreen frustum camera = 
            OrthographicCamera.worldToScreen camera
            |> mode7Matrix frustum

        let screenToWorld frustum camera =
            worldToScreen frustum camera
            |> Matrix.Invert