namespace FireInTheMole.Game
open Microsoft.Xna.Framework.Input


[<AutoOpen>]
module Helpers = 
       
    [<Measure>] type degrees
    [<Measure>] type radians

    let (|KeyDown|_|) key (ks : KeyboardState) = 
        if ks.IsKeyDown(key) then Some() else None

    let normAngle (angle : float32<degrees>) = 
        let angle = angle % 360f<degrees>
        if angle < 0f<degrees> then angle + 360f<degrees> else angle