extends Node

############
# Game state

# Screens enumeration
enum Screens {
    BOOT,
    TITLE,
    GAME,
    GAMEOVER
}

# Current score
var score = 0
# High score
var high_score = 0
# Current lives
var lives = 3

# Current game save
var current_game_save = null

# Screen map
var screen_map = {
    Screens.BOOT: "res://screens/boot_screen/BootScreen.tscn",
    Screens.TITLE: "res://screens/title_screen/TitleScreen.tscn",
    Screens.GAME: "res://screens/game_screen/GameScreen.tscn",
    Screens.GAMEOVER: "res://screens/game_over_screen/GameOverScreen.tscn"
}

################
# Public methods

# Screen loading methods

func load_screen(screen):
    """
    Load screen.
    
    :param screen:  Screen enum value
    """
    _change_scene(screen_map[screen])

# State update methods

func reset_state_values():
    """Reset state values."""
    score = 0
    lives = 5

func add_score(value):
    """
    Add value to score.
    
    :param value:   Score to add
    """
    score += value

func remove_life():
    """Remove a life."""
    lives = max(lives - 1, 0)
    
func add_life():
    """Add a life."""
    lives = lives + 1

func update_hud(hud):
    """
    Update HUD.
    
    :param hud: HUD node
    """
    hud.update_score(score)
    hud.update_lives(lives)

############################
# User data handling methods

func load_empty_game_save():
    """Load default game save."""
    return {
        "high_score": 0
    }

func load_game_save():
    """Load game save."""
    var file_path = File.new()
    if not file_path.file_exists("user://save.dat"):
        return load_empty_game_save()

    var game_save = null
    file_path.open("user://save.dat", File.READ)
    while not file_path.eof_reached():
        game_save = parse_json(file_path.get_line())
        break
    file_path.close()

    if not game_save:
        game_save = load_empty_game_save()

    return game_save

func save_game_save(game_save):
    """
    Save game save.
    
    :param game_save:    Game save
    """
    var file_path = File.new()
    file_path.open("user://save.dat", File.WRITE)
    file_path.store_line(to_json(game_save))
    file_path.close()

func apply_game_save(game_save):
    """
    Apply game save to current state.
    
    :param game_save:    Game save
    """
    high_score = game_save["high_score"]
    current_game_save = game_save

func save_game_over():
    """Save after game over."""
    if score > high_score:
        high_score = score
        current_game_save["high_score"] = high_score
        save_game_save(current_game_save)

func save_game_success():
    """Save after game success."""
    if score > high_score:
        high_score = score
        current_game_save["high_score"] = high_score
    save_game_save(current_game_save)

#################
# Private methods

func _change_scene(path, transition_speed=1):
    Transition.fade_to_scene(path, transition_speed)

###################
# Lifecycle methods

func _ready():
    var game_save = load_game_save()
    apply_game_save(game_save)