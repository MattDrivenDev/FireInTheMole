namespace FireInTheMole.Game

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
