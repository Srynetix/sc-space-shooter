extends Node2D

onready var _animation_player: AnimationPlayer = $AnimationPlayer
onready var _camera: Camera2D = $Camera2D

func _ready() -> void:
    VisualServer.set_default_clear_color(Color.black)
    # get_viewport().connect("size_changed", self, "_resize_camera")
    _resize_camera()

func _resize_camera() -> void:
    var game_size := GameState.get_game_size()
    _camera.position = game_size / 2

func shake_loop() -> void:
    _shake(true)

func shake() -> void:
    _shake(false)

func _shake(loop: bool) -> void:
    if !loop && _animation_player.current_animation == "shake_loop":
        # Do not stop the loop with simple shake
        return

    if loop:
        _animation_player.play("shake_loop")
    else:
        _animation_player.play("shake")

func reset() -> void:
    _animation_player.stop()
    _camera.offset = Vector2.ZERO
