extends Node

############
# Game state

# Screens enumeration
enum Screens {
    BOOT,
    TITLE,
    GAME,
    GAMEOVER,
    SCORE
}

const MAX_HIGH_SCORES = 5
const DEFAULT_HIGH_SCORES = [
    ["ZZZ", 30000],
    ["AAA", 10000],
    ["BBB", 7000]
]
const YOUR_NAME = "YOU"

# Current score
var score = 0
# High scores
var high_scores = []
# Current lives
var lives = 3

# Current game save
var current_game_save = null

# Current configuration
var configuration = {}

# Screen map
var screen_map = {
    Screens.BOOT: "res://screens/boot_screen/BootScreen.tscn",
    Screens.TITLE: "res://screens/title_screen/TitleScreen.tscn",
    Screens.GAME: "res://screens/game_screen/GameScreen.tscn",
    Screens.GAMEOVER: "res://screens/game_over_screen/GameOverScreen.tscn",
    Screens.SCORE: "res://screens/score_screen/ScoreScreen.tscn"
}

################
# Public methods

# Screen loading methods

func load_screen(screen):
    """
    Load screen.

    :param screen:  Screen enum value
    """
    self._change_scene(self.screen_map[screen])

# State update methods

func add_score(value):
    """
    Add value to score.

    :param value:   Score to add
    """
    self.score += value

func remove_life():
    """Remove a life."""
    self.lives = max(self.lives - 1, 0)

func add_life():
    """Add a life."""
    self.lives = self.lives + 1

func reset_game_state():
    """Reset game state."""
    self.lives = 3
    self.score = 0

func update_hud(hud):
    """
    Update HUD.

    :param hud: HUD node
    """
    var high_score = self.get_high_score()
    hud.update_score(self.score)
    hud.update_lives(self.lives)
    hud.update_high_score(high_score[0], high_score[1])

func get_high_score():
    """Get high score."""
    var _high_scores = self.get_high_scores()
    var _high_score = _high_scores[0]
    if self.score > _high_score[1]:
        return [YOUR_NAME, self.score]
    return _high_score

func get_high_scores():
    """Get high scores."""
    if self.high_scores == null:
        return DEFAULT_HIGH_SCORES
    return self.high_scores

func get_version_number():
    """Get version number."""
    return self.configuration["version"]

############################
# User data handling methods

func load_empty_game_save():
    """Load default game save."""
    return {
        "high_scores": DEFAULT_HIGH_SCORES
    }

func load_game_save():
    """Load game save."""
    var file_path = File.new()
    if not file_path.file_exists("user://save.dat"):
        return self.load_empty_game_save()

    var game_save = null
    file_path.open("user://save.dat", File.READ)
    while not file_path.eof_reached():
        game_save = parse_json(file_path.get_line())
        break
    file_path.close()

    if not game_save:
        game_save = self.load_empty_game_save()

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
    self.high_scores = game_save["high_scores"]
    self.current_game_save = game_save

func save_game_over():
    """Save after game over."""
    if self._has_high_score():
        var idx = self._get_high_score_pos()
        self._insert_high_score(idx)
        self.current_game_save["high_scores"] = self.high_scores
        self.save_game_save(self.current_game_save)

#################
# Private methods

func _change_scene(path, transition_speed=1):
    Transition.fade_to_scene(path, transition_speed)

func _get_high_score_pos():
    var idx = 0
    for entry in self.high_scores:
        var high_score = entry[1]
        if self.score > high_score:
            return idx
        idx += 1

    return -1

func _has_high_score():
    var pos = self._get_high_score_pos()
    if pos != -1:
        return true
    else:
        if len(self.high_scores) < MAX_HIGH_SCORES:
            return true
        else:
            return false

func _insert_high_score(idx):
    if idx == -1:
        if len(self.high_scores) < MAX_HIGH_SCORES:
            self.high_scores.push_back([YOUR_NAME, self.score])
    else:
        self.high_scores.insert(idx, [YOUR_NAME, self.score])
        if len(self.high_scores) > MAX_HIGH_SCORES:
            self.high_scores.pop_back()

func _load_config():
    var file_path = File.new()
    file_path.open("res://data/config.json", File.READ)
    self.configuration = parse_json(file_path.get_as_text())
    file_path.close()

###################
# Lifecycle methods

func _ready():
    randomize()

    # Load configuration
    self._load_config()

    var game_save = self.load_game_save()
    self.apply_game_save(game_save)
