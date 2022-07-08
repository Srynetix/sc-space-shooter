extends Area2D
class_name Bullet

enum BulletType {
    SIMPLE = 0,
    DOUBLE,
    TRIPLE,
    LASER,
    SLOW_FAST,
    BOMB
}

enum BulletTarget {
    ENEMY = 0,
    PLAYER
}

class FireData:
    extends Resource

    var bullet: PackedScene
    var pos: Vector2
    var speed: float
    var bullet_type: int
    var bullet_target: int
    var automatic: bool

    func _init(bullet_: PackedScene, pos_: Vector2, speed_: float, bullet_type_: int, bullet_target_: int, automatic_: bool):
        self.bullet = bullet_
        self.pos = pos_
        self.speed = speed_
        self.bullet_type = bullet_type_
        self.bullet_target = bullet_target_
        self.automatic = automatic_

const ENEMY_BULLET_SPRITE: Texture = preload("res://assets/textures/laserRed06.png")
const SPARKLES_SCENE: PackedScene = preload("res://nodes/fx/Sparkles.tscn")
const FXWAVE_SCENE: PackedScene = preload("res://nodes/fx/FXWave.tscn")

export(BulletTarget) var bullet_target := BulletTarget.ENEMY
export(BulletType) var bullet_type := BulletType.SIMPLE
export var bullet_automatic := false

onready var _visibility_notifier: VisibilityNotifier2D = $VisibilityNotifier2D
onready var _slow_timer: Timer = $SlowTimer
onready var _bomb_timer: Timer = $BombTimer
onready var _trail: CPUParticles2D = $Trail
onready var _sprite: Sprite = $Sprite

var _velocity := Vector2.ZERO
var _base_speed := 0.0
var _enabled := true

func _ready() -> void:
    connect("area_entered", self, "_on_area_entered")
    _visibility_notifier.connect("screen_exited", self, "queue_free")
    _bomb_timer.connect("timeout", self, "_on_bomb_timer_timeout")

    if bullet_target == BulletTarget.PLAYER:
        _sprite.texture = ENEMY_BULLET_SPRITE
        _trail.texture = ENEMY_BULLET_SPRITE

        set_collision_layer_bit(1, false)
        set_collision_layer_bit(5, true)
        set_collision_mask_bit(2, false)
        set_collision_mask_bit(4, false)
        set_collision_mask_bit(0, true)

    if bullet_automatic:
        _handle_automatic_mode()

    if bullet_type == BulletType.LASER:
        _sprite.scale *= 3
        _trail.scale_amount *= 3
    elif bullet_type == BulletType.SLOW_FAST:
        _velocity /= 6.0
        _slow_timer.start()
        yield(_slow_timer, "timeout")
        _handle_automatic_mode()
        _velocity *= 1.5
    elif bullet_type == BulletType.BOMB:
        _bomb_timer.start()

func _process(delta: float) -> void:
    position += _velocity * delta

    if bullet_type == BulletType.BOMB:
        rotation_degrees += _velocity.y * delta

func prepare(data: FireData) -> void:
    position = data.pos
    bullet_type = data.bullet_type
    bullet_target = data.bullet_target
    bullet_automatic = data.automatic
    _base_speed = data.speed

    if bullet_target == BulletTarget.PLAYER:
        _velocity = Vector2(0, _base_speed)
    else:
        _velocity = Vector2(0, -_base_speed)

    if bullet_type == BulletType.BOMB:
        _base_speed /= 2.0

func _handle_automatic_mode() -> void:
    if bullet_target == BulletTarget.PLAYER:
        var players := get_tree().get_nodes_in_group("player")
        if len(players) > 0:
            _rotate_to_target(players[0])
    elif bullet_target == BulletTarget.ENEMY:
        var enemies := get_tree().get_nodes_in_group("enemies")
        if len(enemies) > 0:
            _rotate_to_target(enemies[0])

func _rotate_to_target(target: Node2D) -> void:
    var direction := (target.position - position).normalized()
    var angle := Vector2(0, 1).angle_to(direction)

    rotation = angle
    _trail.angle = 360 - rad2deg(angle)
    _velocity = direction * _base_speed

func _trigger_wave(pos: Vector2) -> void:
    var wave: FXWave = FXWAVE_SCENE.instance()
    get_parent().add_child(wave)

    wave.start_then_free(pos, 0.35)
    queue_free()

func _trigger_sparkles(pos: Vector2) -> void:
    var sparkles: Sparkles = SPARKLES_SCENE.instance()
    sparkles.position = pos
    sparkles.z_index = 10
    get_parent().add_child(sparkles)

func _on_area_entered(area: Area2D) -> void:
    if !_enabled:
        return

    if area.is_in_group("rocks") || area.is_in_group("enemies") || area.is_in_group("player"):
        _enabled = false
        _trigger_sparkles(position)

        if bullet_type == BulletType.BOMB:
            _bomb_timer.stop()
            call_deferred("_trigger_wave", position)
        else:
            queue_free()

func _on_bomb_timer_timeout() -> void:
    if !_enabled:
        return

    _trigger_sparkles(position)
    call_deferred("_trigger_wave", position)
