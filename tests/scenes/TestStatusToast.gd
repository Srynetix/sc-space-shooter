extends Control

onready var _player: Player = $Player

func _ready() -> void:
    _player.connect("respawn", self, "_on_respawn")
    _player.get_status_toast().show_priority_message("Hello World!")
    _player.get_status_toast().show_message("Hello World 2!")
    yield(_player.get_status_toast(), "message_all_shown")

    while true:
        _player.get_status_toast().show_message("Short")
        yield(_player.get_status_toast(), "message_shown")

        _player.get_status_toast().show_message("Long long long long")
        yield(_player.get_status_toast(), "message_shown")

func _on_respawn() -> void:
    _player.respawn()
