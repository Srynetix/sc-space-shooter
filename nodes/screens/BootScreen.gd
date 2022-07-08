extends Control
class_name BootScreen

func _ready() -> void:
    yield(get_tree().create_timer(1), "timeout")
    GameState.load_screen(GameState.Screens.TITLE)
