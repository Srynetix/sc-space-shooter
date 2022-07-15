extends Area2D
class_name Player

enum State {
    IDLE,
    SPAWNING,
    DEAD
}

const MESSAGE_SPEED := 0.05

signal dead()
signal respawn()

export(Bullet.BulletType) var initial_bullet_type := Bullet.BulletType.SIMPLE

onready var _spawn_timer: Timer = $Timers/SpawningTimer
onready var _muzzle: Position2D = $Position2D
onready var _sprite: Sprite = $Sprite
onready var _animation_player: AnimationPlayer = $AnimationPlayer
onready var _collision_shape: CollisionShape2D = $CollisionShape2D
onready var _bullet_system: BulletSystem = $BulletSystem
onready var _status_toast: StatusToast = $StatusToast
onready var _touch_controller: TouchController = $TouchController

var _initial_position := Vector2.ZERO
var _velocity := Vector2.ZERO
var _state: int = State.IDLE
var _move_speed := Vector2(500, 500)
var _damping := 0.9
var _spawn_time := 3.0

func _ready() -> void:
    connect("area_entered", self, "_on_area_entered")

    _spawn_timer.wait_time = _spawn_time
    _spawn_timer.connect("timeout", self, "_on_spawn_timer_timeout")
    _bullet_system.connect("bomb_available", self, "_on_bomb_available")
    _bullet_system.connect("bomb_used", self, "_on_bomb_used")
    _bullet_system.target_container = get_parent()

    var game_size := GameState.get_game_size()
    _initial_position = Vector2(game_size.x / 2.0, game_size.y - game_size.y / 8.0)
    position = _initial_position

    _bullet_system.switch_type(initial_bullet_type)
    _status_toast.show_priority_message(tr("PLAYER_SPAWN_MSG"))

func _process(delta: float) -> void:
    if _state == State.DEAD:
        return

    if _touch_controller.touching:
        _velocity = Vector2.ZERO
        position = _touch_controller.get_computed_position()
    else:
        var movement := _handle_movement()
        if movement.x == 0 && movement.y == 0:
            _velocity *= _damping
        else:
            _velocity = movement * _move_speed

    _handle_fire()

    position += _velocity * delta
    _clamp_position()

func respawn() -> void:
    _set_state(State.SPAWNING)
    position = _initial_position
    _bullet_system.reset_weapons()

func get_bullet_system() -> BulletSystem:
    return _bullet_system

func get_status_toast() -> StatusToast:
    return _status_toast

func hit() -> void:
    _set_state(State.DEAD)

func _clamp_position() -> void:
    var game_size := GameState.get_game_size()

    position = Vector2(
        clamp(position.x, 0, game_size.x),
        clamp(position.y, 0, game_size.y)
    )

func _set_state(new_state: int) -> void:
    if _state == new_state:
        return

    _state = new_state
    match new_state:
        State.DEAD:
            _set_state_dead()
        State.IDLE:
            _set_state_idle()
        State.SPAWNING:
            _set_state_spawning()

func _set_state_dead() -> void:
    _velocity = Vector2.ZERO
    _touch_controller.reset_state()
    _touch_controller.set_process(false)
    _touch_controller.set_process_input(false)

    _status_toast.stop()
    _collision_shape.set_deferred("disabled", true)

    emit_signal("dead")
    call_deferred("_play_explosion")

func _set_state_idle() -> void:
    _collision_shape.set_deferred("disabled", false)
    _animation_player.play("idle")
    _status_toast.show_priority_message(tr("PLAYER_SPAWN_MSG"))

func _set_state_spawning() -> void:
    _touch_controller.set_process(true)
    _touch_controller.set_process_input(true)
    _spawn_timer.start()
    _animation_player.play("spawning")

func can_upgrade_weapon() -> bool:
    return _bullet_system.can_upgrade_weapon()

func upgrade_weapon():
    _bullet_system.upgrade_weapon()
    _status_toast.show_message(tr("PLAYER_WEAPON_UPGRADE_MSG"))

func _play_explosion() -> void:
    _animation_player.play("explode")
    yield(_animation_player, "animation_finished")

    emit_signal("respawn")

func _handle_movement() -> Vector2:
    var movement := Vector2()

    if Input.is_action_pressed("player_left"):
        movement.x -= 1
    if Input.is_action_pressed("player_right"):
        movement.x += 1
    if Input.is_action_pressed("player_up"):
        movement.y -= 1
    if Input.is_action_pressed("player_down"):
        movement.y += 1

    return movement

func _handle_fire() -> void:
    if Input.is_action_just_pressed("player_bomb") || _touch_controller.double_touching:
        _bullet_system.fire_bomb(_muzzle.global_position)
    elif Input.is_action_pressed("player_shoot") || _touch_controller.touching:
        _bullet_system.fire(_muzzle.global_position)

func _on_spawn_timer_timeout() -> void:
    _set_state(State.IDLE)

func _on_area_entered(area: Area2D) -> void:
    if area.is_in_group("rocks") || area.is_in_group("enemies") || area.is_in_group("bullets"):
        _set_state(State.DEAD)

func _on_bomb_available() -> void:
    _sprite.modulate = Color.green
    _status_toast.show_message(tr("PLAYER_BOMB_PICKED_MSG"))

func _on_bomb_used() -> void:
    _sprite.modulate = Color.white
    _status_toast.show_message(tr("PLAYER_BOMB_FIRED_MSG"))
