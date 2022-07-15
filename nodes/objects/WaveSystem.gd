extends Control
class_name WaveSystem

signal timeout()

onready var _wave_timer: Timer = $WaveTimer

var _waves := []
var _current_wave := 0

func _ready() -> void:
    _waves = _load_wave_file()
    _wave_timer.connect("timeout", self, "_on_timer_timeout")

func get_current_wave() -> int:
    return _current_wave

func load_next_wave() -> Dictionary:
    _current_wave += 1

    var wave_info := _get_current_wave_info()
    _wave_timer.wait_time = float(wave_info["duration"])
    _wave_timer.start()

    return wave_info

func _load_wave_file() -> Array:
    return SxJson.read_json_file("res://data/waves.json")

func _get_current_wave_info() -> Dictionary:
    if len(_waves) < _current_wave:
        return _waves[len(_waves) - 1]
    else:
        return _waves[_current_wave - 1]

func _on_timer_timeout() -> void:
    emit_signal("timeout")
