extends Control

############
# BootScreen

###################
# Lifecycle methods

func _ready():
    GameState.load_screen(GameState.Screens.TITLE)