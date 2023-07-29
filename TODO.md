# TODO
1. Track player wins too, as well as a match score.
2. Add a few more maps.
3. Add a sound effect when picking up a powerup.
4. Add some music... perhaps a different track per map?
5. Record a "Fire in the Mole!" wav with Andrew to be used for the title screen.

## Maybe
1. Scale positions down by the tile width (50) down to simplify everything and use same scale everywhere; and then scale up when rendering things.
2. Investigate both ViewPort and RenderTarget2D for the splitscreen stuff instead of current method of manually clipping and scaling.
3. Texture floor & ceiling - that needs some research because that needs perspective too (mode7-ish?).
4. Bug - fix the janky size & canwalk logic when moving at a shallow angle. This needs me to completely overhaul the tile/map system etc.
5. Refactoring - there's a bunch of things that need to be looked at and refactored. It's a mess of code right now.

## Done
- ~~Add a grave where a mole dies~~
- ~~Mole vs Mole Collision Detection~~
- ~~Figure out how to draw/animate the mole from different angles~~
- ~~Replace mole sprites with something "nicer" - each with different hard hat colour (yellow, red, blue, green).~~
- ~~and a map selection screen when starting a battle game.~~
- ~~Bug - not all dynamites per player are upgraded in size when picking up a powerup.~~
- ~~Replace explosion sprites with something nicer~~
- ~~Replace dynamite sprites with something nicer~~
- ~~Replace wall textures with something nicer~~
- ~~Bug - I *think* I might need to look at sprite scaling in split screen modes... some look wider than I think they should be.~~
- ~~Bug - dynamite instantly explodes when one is already exploding elsewhere (only with 1 dynamite)~~
- ~~Add a fuse sound effect.~~
- ~~Add an explosion sound effect.~~
- ~~Screen Management, menus, number of players, options, credits, etc.~~
- ~~Destroying walls need either a random chance to spawn pickups or a definite chance to drop a random pickup.~~
- ~~Collecting pickups need to buff or debuff the player character.~~
- ~~Give the players a "score" which is just a kill count for the now~~
- ~~Give the player some "size".~~
- ~~Explosions need to kill players.~~
- ~~Explosions need to remove breakable walls.~~
- ~~Figure out explosions more properly - either raycast those or follow the map grid.~~
- ~~Figure out the dynamite animation at the same time as sorting the player sprite out.~~
- ~~Fix the split screen stuff for sprites (visible bug)~~
- ~~Give the player a sprite of some kind.~~
- ~~Drop dynamite sprite into projection renderer.~~
- ~~The `PlayerCharacter` needs to be on the same scale as the map for the ray casting algorith.~~
- ~~Get the `RayCaster` finished.~~
- ~~Work on the `ProjectionRenderer` so we can get towards that pseudo-3D.~~
- ~~Horizontal split screen for 2-player.~~
- ~~4-way split screen for 3-player with one quarter blacked out~~
- ~~4-way split screen for 4-player.~~
- ~~Clean up the mess I made doing split screen projection and refactor things up some.~~
- ~~Put SOME texture on the walls so that they're not block colours.~~
- ~~Figure out texturing source rect in splitscreen modes.~~
