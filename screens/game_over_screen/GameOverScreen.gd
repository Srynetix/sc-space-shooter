extends Control

##################
# Game Over screen

###################
# Lifecycle methods

func _ready():
    GameState.save_game_over()
    yield(self.get_tree().create_timer(2), "timeout")
    GameState.load_screen(GameState.Screens.SCORE)
