extends Area2D
class_name Powerup

enum PowerupType {
    WEAPON_UPGRADE,
    BOMB,
    LIFE
}

signal powerup(item)
export(PowerupType) var powerup_type := PowerupType.WEAPON_UPGRADE

onready var _visibility_notifier: VisibilityNotifier2D = $VisibilityNotifier2D
onready var _animation_player: AnimationPlayer = $AnimationPlayer
onready var _collision_shape: CollisionShape2D = $CollisionShape2D
onready var _sound: AudioStreamPlayer2D = $Sound

var _velocity := Vector2.ZERO

func _ready() -> void:
    _visibility_notifier.connect("screen_exited", self, "queue_free")
    connect("area_entered", self, "_on_area_entered")

func _process(delta: float) -> void:
    position += _velocity * delta

func prepare(initial_pos: Vector2, initial_speed: float, initial_scale: float) -> void:
    position = initial_pos
    scale = Vector2(initial_scale, initial_scale)
    _velocity = Vector2(0, initial_speed)

func _consume() -> void:
    _collision_shape.set_deferred("disabled", true)
    _sound.play()
    emit_signal("powerup", self)
    _animation_player.play("fade")

    yield(_sound, "finished")
    queue_free()

func _on_area_entered(area: Area2D) -> void:
    if area.is_in_group("player_body"):
        _consume()
