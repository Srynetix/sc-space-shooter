extends Control

############
# BootScreen

###################
# Lifecycle methods

func _ready():
    yield(get_tree().create_timer(1), "timeout")
    GameState.load_screen(GameState.Screens.TITLE)
