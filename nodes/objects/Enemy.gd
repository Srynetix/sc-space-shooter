extends Area2D
class_name Enemy

signal exploded(node)

const BASE_HIT_POINTS := 5
const BASE_FIRE_TIME := 0.25
const BOMB_HIT_TIME := 0.1
const BOMB_HIT_COUNT := 2

export var move_speed := 150.0
export var down_speed := 50.0
export var fire_time := BASE_FIRE_TIME
export var hit_points := BASE_HIT_POINTS

onready var _animation_player: AnimationPlayer = $AnimationPlayer
onready var _collision_shape: CollisionShape2D = $CollisionShape2D
onready var _muzzle: Position2D = $Position2D
onready var _fire_timer: Timer = $FireTimer
onready var _bullet_system: BulletSystem = $BulletSystem
onready var _status_toast: StatusToast = $StatusToast
onready var _explosion_sound: AudioStreamPlayer2D = $ExplosionSound
onready var _sprite: Sprite = $Sprite
onready var _progress_bar: AnimatedProgressBar = $AnimatedProgressBar
onready var _bomb_hit_timer: Timer = $BombHitTimer

var _hit_count := 0
var _has_exploded := false
var _acc := 0.0
var _is_firing := false
var _showing_message := false
var _currently_in_bomb := false

func _ready() -> void:
    connect("area_entered", self, "_on_area_entered")
    connect("area_exited", self, "_on_area_exited")

    _fire_timer.wait_time = fire_time
    _fire_timer.connect("timeout", self, "_on_fire_timer_timeout")
    _bomb_hit_timer.wait_time = BOMB_HIT_TIME
    _bomb_hit_timer.connect("timeout", self, "_on_bomb_hit_timer_timeout")
    _status_toast.connect("message_all_shown", self, "_on_message_all_shown")
    _bullet_system.target_container = get_parent()

    set_fire_time_factor(0.0)
    set_hit_points_factor(0.0)
    _scan_player()

func _process(delta: float) -> void:
    _acc += delta

    _move(delta)
    _handle_state()

    if !_showing_message:
        _showing_message = true
        _status_toast.show_message_with_color(tr("ENEMY_FIRE_MSG"), Color.red)

func prepare(initial_pos: Vector2, initial_speed: float, initial_scale: float) -> void:
    position = initial_pos
    down_speed = initial_speed
    scale = Vector2(initial_scale, initial_scale)

func _calculate_base_hit_points() -> int:
    return BASE_HIT_POINTS

func _calculate_base_fire_time() -> float:
    return BASE_FIRE_TIME

func set_fire_time_factor(factor: float) -> void:
    fire_time = int(ceil(_calculate_base_fire_time() + factor))
    _fire_timer.wait_time = fire_time
    _fire_timer.start()

func set_hit_points_factor(factor: float) -> void:
    hit_points = int(ceil(_calculate_base_hit_points() + factor))
    _progress_bar.max_value = hit_points
    _progress_bar.value = hit_points

func hit(count: int = 1) -> void:
    if !_has_exploded:
        _animation_player.play("tint")
        _hit_count += count
        _update_progress_bar()
        if _hit_count >= hit_points:
            explode()

func _update_progress_bar() -> void:
    _progress_bar.value = hit_points - _hit_count
    _progress_bar.fade_in()

func explode() -> void:
    _has_exploded = true
    _is_firing = false
    _fire_timer.stop()
    emit_signal("exploded", self)
    _disable_collisions()
    _status_toast.stop()
    _explosion_sound.play()
    _animation_player.play("explode")

    yield(_animation_player, "animation_finished")
    queue_free()

func _disable_collisions() -> void:
    _collision_shape.set_deferred("disabled", true)

func _move(delta: float) -> void:
    position += Vector2(move_speed, down_speed) * delta * Vector2(cos(_acc), 1)

func _handle_state() -> void:
    var game_size := GameState.get_game_size()

    if _is_firing:
        _bullet_system.fire(_muzzle.global_position)

    var sprite_size := _sprite.texture.get_size()
    if position.y - sprite_size.y > game_size.y:
        queue_free()

func _scan_player() -> void:
    var players := get_tree().get_nodes_in_group("player")
    for node in players:
        var player: Player = node
        player.connect("dead", self, "_taunt_player")

func _taunt_player() -> void:
    _status_toast.show_priority_message_with_color(tr("ENEMY_TAUNT_MSG"), Color.red)

func _on_area_entered(area: Area2D) -> void:
    if area.is_in_group("wave"):
        _currently_in_bomb = true
        hit(BOMB_HIT_COUNT)
        _bomb_hit_timer.start()
    elif area.is_in_group("bullets"):
        hit()

func _on_area_exited(area: Area2D) -> void:
    if area.is_in_group("wave"):
        _currently_in_bomb = false

func _on_fire_timer_timeout() -> void:
    _is_firing = !_is_firing

func _on_bomb_hit_timer_timeout() -> void:
    if _currently_in_bomb:
        hit(BOMB_HIT_COUNT)
        _bomb_hit_timer.start()

func _on_message_all_shown() -> void:
    _showing_message = false
