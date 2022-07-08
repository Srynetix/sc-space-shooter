tool
extends Control
class_name Starfield

const RADIAL_ACCEL := 100.0

export var velocity := 100.0 setget _set_velocity
export var enable_radial_accel := false setget _set_enable_radial_accel

onready var _particles: CPUParticles2D = $Particles2D

var _should_update := true

func _ready() -> void:
    _particles.initial_velocity = velocity
    get_viewport().connect("size_changed", self, "_reset_state")
    _reset_state()

func _process(_delta: float) -> void:
    if _should_update:
        _should_update = false
        _reset_state()

func _reset_state() -> void:
    var game_size := get_viewport_rect().size
    _particles.position = game_size / 2 - Vector2(0, _particles.initial_velocity)
    _particles.emission_rect_extents = Vector2(game_size.x / 2, game_size.y / 2)
    _particles.initial_velocity = velocity

    if enable_radial_accel:
        _particles.radial_accel = RADIAL_ACCEL
    else:
        _particles.radial_accel = 0

func _set_velocity(value: float) -> void:
    velocity = value
    _should_update = true

func _set_enable_radial_accel(value: bool) -> void:
    enable_radial_accel = value
    _should_update = true
