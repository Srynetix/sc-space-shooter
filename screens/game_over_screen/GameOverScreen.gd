extends Control

##################
# Game Over screen

###################
# Lifecycle methods

func _ready():
    yield(get_tree().create_timer(2), "timeout")
    GameState.load_screen(GameState.Screens.TITLE)