extends Control

onready var _timer: Timer = $Timer

func _ready() -> void:
    _timer.connect("timeout", self, "_on_timeout")

func _on_timeout() -> void:
    FXCamera.shake()
