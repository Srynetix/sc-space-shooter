extends Enemy
class_name BossEnemy

const BOSS_BASE_HIT_POINTS := 50
const BOSS_BASE_FIRE_TIME := 0.25

onready var _weapon_swap: Timer = $WeaponSwap

var _velocity := Vector2.ZERO

func _ready() -> void:
    _fire_timer.wait_time = BOSS_BASE_FIRE_TIME
    _fire_timer.start()

    _weapon_swap.connect("timeout", self, "_on_weapon_swap_timeout")
    _weapon_swap.start()

    set_fire_time_factor(0.0)
    set_hit_points_factor(0.0)

func prepare(initial_pos: Vector2, initial_speed: float, initial_scale: float) -> void:
    .prepare(initial_pos, initial_speed, initial_scale)

    scale = Vector2(initial_scale * 2, initial_scale * 2)
    move_speed *= 1.5
    _velocity = Vector2(move_speed, down_speed)

func _calculate_base_hit_points() -> int:
    return BOSS_BASE_HIT_POINTS

func _calculate_base_fire_time() -> float:
    return BOSS_BASE_FIRE_TIME

func _move(delta: float) -> void:
    position += _velocity * delta * Vector2(cos(_acc), 1)

    _velocity.y *= 0.992
    if _velocity.y <= 0.01:
        _velocity.y = 0

func _on_weapon_swap_timeout() -> void:
    _bullet_system.switch_random_type()
