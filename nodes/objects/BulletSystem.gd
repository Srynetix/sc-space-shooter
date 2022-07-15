extends Node2D
class_name BulletSystem

signal type_switch(prev_bullet_type, new_bullet_type)
signal bomb_available()
signal bomb_used()

export var fire_cooldown := 0.2
export var fire_speed := 500.0
export var bullet_model: PackedScene
export(Bullet.BulletType) var bullet_type := Bullet.BulletType.SIMPLE
export(Bullet.BulletTarget) var bullet_target := Bullet.BulletTarget.ENEMY
export var bullet_automatic := false
export var bomb_available := false

var target_container: Node

onready var _fire_timer: Timer = $FireTimer
onready var _sound: AudioStreamPlayer2D = $Sound

var _can_shoot := true
var _weapon_locked := false
var _previous_bullet_type: int = Bullet.BulletType.SIMPLE

func _ready() -> void:
    _fire_timer.wait_time = fire_cooldown
    _fire_timer.connect("timeout", self, "_on_fire_timer_timeout")

func enable_bomb() -> void:
    emit_signal("bomb_available")
    bomb_available = true

func reset_weapons() -> void:
    bomb_available = false

    if !_weapon_locked:
        _previous_bullet_type = Bullet.BulletType.SIMPLE
        switch_type(Bullet.BulletType.SIMPLE)

func upgrade_weapon() -> void:
    if _weapon_locked:
        return

    if bullet_type == Bullet.BulletType.SIMPLE:
        switch_type(Bullet.BulletType.DOUBLE)
    elif bullet_type == Bullet.BulletType.DOUBLE:
        switch_type(Bullet.BulletType.TRIPLE)
    elif bullet_type == Bullet.BulletType.TRIPLE:
        switch_type(Bullet.BulletType.LASER)

func switch_type(type: int) -> void:
    if bullet_type == type:
        return

    if type == Bullet.BulletType.LASER:
        _fire_timer.wait_time = fire_cooldown / 4.0
    else:
        _fire_timer.wait_time = fire_cooldown

    _previous_bullet_type = bullet_type
    bullet_type = type

    emit_signal("type_switch", _previous_bullet_type, bullet_type)

func switch_random_type() -> void:
    var weapon_id := SxMath.rand_range_i(0, 5)
    match weapon_id:
        0:
            switch_type(Bullet.BulletType.SIMPLE)
        1:
            switch_type(Bullet.BulletType.DOUBLE)
        2:
            switch_type(Bullet.BulletType.TRIPLE)
        3:
            switch_type(Bullet.BulletType.LASER)
        4:
            switch_type(Bullet.BulletType.SLOW_FAST)

func lock_weapon() -> void:
    _weapon_locked = true

func unlock_weapon() -> void:
    _weapon_locked = false

func can_upgrade_weapon() -> bool:
    return bullet_type != Bullet.BulletType.LASER

func fire(pos: Vector2) -> void:
    _fire(pos, bullet_type)

func fire_bomb(pos: Vector2) -> void:
    if bomb_available:
        bomb_available = false
        _can_shoot = true
        emit_signal("bomb_used")
        _fire(pos, Bullet.BulletType.BOMB)

func _fire(pos: Vector2, target_bullet_type: int) -> void:
    if _can_shoot:
        _can_shoot = false

        if target_bullet_type == Bullet.BulletType.DOUBLE:
            _spawn_bullet(Bullet.FireData.new(bullet_model, pos - Vector2(20, 0), fire_speed, target_bullet_type, bullet_target, bullet_automatic))
            _spawn_bullet(Bullet.FireData.new(bullet_model, pos + Vector2(20, 0), fire_speed, target_bullet_type, bullet_target, bullet_automatic))
        elif target_bullet_type == Bullet.BulletType.TRIPLE:
            _spawn_bullet(Bullet.FireData.new(bullet_model, pos - Vector2(20, 0), fire_speed, target_bullet_type, bullet_target, bullet_automatic))
            _spawn_bullet(Bullet.FireData.new(bullet_model, pos - Vector2(0, 40), fire_speed, target_bullet_type, bullet_target, bullet_automatic))
            _spawn_bullet(Bullet.FireData.new(bullet_model, pos + Vector2(20, 0), fire_speed, target_bullet_type, bullet_target, bullet_automatic))
        else:
            _spawn_bullet(Bullet.FireData.new(bullet_model, pos, fire_speed, target_bullet_type, bullet_target, bullet_automatic))

        _sound.play()
        _fire_timer.start()

func _spawn_bullet(data: Bullet.FireData) -> void:
    var instance: Bullet = data.bullet.instance()
    instance.prepare(data)

    if target_container != null:
        target_container.add_child(instance)
    else:
        get_parent().add_child(instance)

func _on_fire_timer_timeout() -> void:
    _can_shoot = true
