extends Control

const FXWAVE_SCENE := preload("res://nodes/fx/FXWave.tscn")

func _ready() -> void:
    yield(get_tree().create_timer(1.0), "timeout")

    while true:
        var wave: FXWave = FXWAVE_SCENE.instance()
        add_child(wave)
        wave.start(GameState.get_game_size() / 2, 0.35)
        yield(get_tree().create_timer(0.2), "timeout")

        var wave2: FXWave = FXWAVE_SCENE.instance()
        add_child(wave2)
        wave2.start(GameState.get_game_size() / 4, 0.35)
        yield(get_tree().create_timer(0.2), "timeout")

        var wave3: FXWave = FXWAVE_SCENE.instance()
        add_child(wave3)
        wave3.start(GameState.get_game_size() / 2 + GameState.get_game_size() / 4, 0.35)

        yield(wave, "finished")
        yield(wave2, "finished")
        yield(wave3, "finished")
        yield(get_tree().create_timer(1.0), "timeout")
