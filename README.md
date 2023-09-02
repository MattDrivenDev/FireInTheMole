# Fire In The Mole!
A game about moles mining with dynamite and blowing each other up (a Bomberstein3D).

## TODO
 - [x] Change the animation based on the movement and direction of the player.
 - [x] Load a Tiled map.
 - [ ] Load player spawn points from the Tile Map.
 - [ ] When rendering top-down, use an orthographic camera that follows the player.
 - [ ] Bug: prototype janky collision detection... allow player to move flush up against the wall.
 - [x] Render mole instead of circle for player.

## But why F#?
So... I wrote the prototype in C#. Then, after some playtesting and realising that the game might have some legs, I knew I had to do some fairly heavy refactoring. But, the harder I refactored, the more it became a re-write. And then the harder I re-wrote, the more of a hot spaghetti mess it became again.

I am 100% finding it hard to write game code. It is quite far removed from the kind of code that I write day-to-day in boring business software land. Dependency injection, strategy patterns, and event streaming have me all in a muddle - and my game code just ends up being a hot mess that I struggle to work with without introducing 3 bugs for every 1 feature.

I've had quite a lot of experience in F#, and I'm fairly confident with it. One of the nice things about it is that it forces you to be more strict... and in this instance, the strictness I think comes from the lack of circular dependencies (which is enforced at compile-time with the type system).

So, consider this to be a kind of prototype++.

### Tools
 - MapViewer: used to quickly test tile map rendering, tile destruction, and an orthographic camera.
 - Floored: to figure out how to do the floor texturing (Mode7 camera)... hopefully this will work!
 - Collider: to figure out how to do the collision detection.