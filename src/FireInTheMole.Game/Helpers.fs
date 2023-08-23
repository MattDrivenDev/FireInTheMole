namespace FireInTheMole.Game
open Microsoft.Xna.Framework.Input


[<AutoOpen>]
module Helpers = 
       
    [<Measure>] type degrees
    [<Measure>] type radians

    let (|KeyDown|_|) key (ks : KeyboardState) = 
        if ks.IsKeyDown(key) then Some() else None