namespace FireInTheMole.Game

open System
open Microsoft.Xna.Framework

[<AutoOpen>]
module ThingsThatShouldProbablyBeConfigurable = 
    
    [<Literal>]
    let FULLSCREEN = false
    [<Literal>]
    let TARGET_FPS = 60
    [<Literal>]
    let SCREEN_WIDTH = 1600
    [<Literal>]
    let SCREEN_HEIGHT = 900
    [<Literal>]
    let CORRECT_FISHEYE = true
    /// Not-Literal: Calculated from SCREEN_WIDTH
    let SCREEN_WIDTH_HALF = SCREEN_WIDTH / 2
    /// Not-Literal: Calculated from SCREEN_HEIGHT
    let SCREEN_HEIGHT_HALF = SCREEN_HEIGHT / 2
    /// Non-Literal: Just an alias for SCREEN_WIDTH_HALF
    let RAY_COUNT = SCREEN_WIDTH_HALF

    [<Literal>]
    let MAP_TILE_SIZE = 256

    [<Literal>]
    let PLAYER_FOV = 75f
    [<Literal>]
    let PLAYER_SPEED = 1f
    [<Literal>]
    let PLAYER_SPEED_ROTATION = 0.01f 
    [<Literal>]
    let PLAYER_SIZE = 192
    [<Literal>]
    let PLAYER_DYNAMITE_TIME_COOLDOWN = 0.5f
    [<Literal>]
    let PLAYER_DYNAMITE_TIME_FUSE = 3f
    [<Literal>]
    let PLAYER_DYNAMITE_TIME_EXPLOSION = 1f
    /// Not-Literal: Calculated from PLAYER_FOV
    let PLAYER_FOV_HALF = PLAYER_FOV / 2f    
    /// Not-Literal: Calculated from SCREEN_WIDTH_HALF and PLAYER_FOV_HALF
    let PLAYER_PROJECTION_DISTANCE = int (float32 SCREEN_WIDTH_HALF / MathF.Tan(MathHelper.ToRadians(PLAYER_FOV_HALF)))

    [<Literal>]
    let DEGS_360 = 360f


[<AutoOpen>]
module MagicStrings = 

    [<Literal>]
    let GAME_TITLE = "FIRE IN THE MOLE"
    [<Literal>]
    let QUIT_GAME = "Quit Game"
    [<Literal>]
    let MUSIC_VOLUME = "Music Volume"
    [<Literal>]
    let QUIT_GAME_CONFIRMATION = "Quit Game? (Press Enter to Confirm)"
    [<Literal>]
    let RESUME_GAME = "Resume Game"
    [<Literal>]
    let PAUSE_MENU_UPDATE_ERROR = "Error in the pause menu update function"
    [<Literal>]
    let WALLS_LAYER_NAME = "Walls"
    [<Literal>]
    let DIRT_LAYER_NAME = "Dirt"
    [<Literal>]
    let SPAWN_LAYER_NAME = "Spawn"    
    [<Literal>]
    let TILEDMAP_LOADMAP_NOSPAWNLAYER_ERROR = "No spawn layer!"    
    [<Literal>]
    let TILEDMAP_MAPSPAWNOBJECTSLAYER_ERROR = "Error in mapSpawnObjectsLayer - spawn layer should have no more/less than 4 spawn points named 1 through 4"
    [<Literal>]
    let INVALID_PLAYER_INDEX = "Invalid Player Index"
    [<Literal>]
    let CONTENT_FONTS = "Fonts"
    [<Literal>]
    let FONT_TITLE = "title"
    [<Literal>]
    let FONT_TEXT = "text"
    [<Literal>]
    let CONTENT_SOUNDS_UI = "Sounds/UI"
