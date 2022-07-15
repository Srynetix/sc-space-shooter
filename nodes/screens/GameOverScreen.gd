extends Control
class_name GameOverScreen

func _ready() -> void:
    GameState.save_game_over()
    yield(get_tree().create_timer(2.0), "timeout")

    GameState.load_screen(GameState.Screens.SCORE)
