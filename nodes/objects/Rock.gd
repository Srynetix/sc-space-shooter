extends Area2D
class_name Rock

const BASE_HIT_POINTS := 3
const X_VELOCITY_SPEED := 100.0

signal exploded(node)

onready var _trail: CPUParticles2D = $Sprite/Trail
onready var _animation_player: AnimationPlayer = $AnimationPlayer
onready var _collision_shape: CollisionShape2D = $CollisionShape2D
onready var _sprite: Sprite = $Sprite
onready var _explosion_sound: AudioStreamPlayer2D = $ExplosionSound

var _velocity := Vector2.ZERO
var _hit_points := 0
var _hit_count := 0
var _has_exploded := false

func _ready() -> void:
    connect("area_entered", self, "_on_area_entered")
    _trail.scale_amount = scale.x

func _process(delta: float) -> void:
    position += _velocity * delta
    rotation += delta / 2.0
    _trail.angle = 360 - rad2deg(rotation)
    _handle_position_wrap()

func prepare(initial_pos: Vector2, initial_speed: float, initial_scale: float) -> void:
    position = initial_pos
    scale = Vector2(initial_scale, initial_scale)
    _velocity = Vector2(rand_range(-X_VELOCITY_SPEED, X_VELOCITY_SPEED), initial_speed)
    _hit_points = int(ceil(initial_scale * BASE_HIT_POINTS))

func hit() -> void:
    if !_has_exploded:
        _animation_player.play("tint")
        _hit_count += 1
        if _hit_count == _hit_points:
            explode()

func explode() -> void:
    _has_exploded = true
    _explode()

func _explode() -> void:
    emit_signal("exploded", self)
    _collision_shape.set_deferred("disabled", true)
    _explosion_sound.play()
    _animation_player.play("explode")

    yield(_animation_player, "animation_finished")
    queue_free()

func _handle_position_wrap() -> void:
    var sprite_size := _sprite.texture.get_size() * scale
    var game_size := GameState.get_game_size()
    var x_screen_limits := Vector2(-sprite_size.x / 2, game_size.x + sprite_size.x / 2)

    if position.x > x_screen_limits.y:
        position = Vector2(x_screen_limits.x, position.y)
    elif position.x < x_screen_limits.x:
        position = Vector2(x_screen_limits.y, position.y)

    if position.y - _sprite.texture.get_size().y > game_size.y:
        queue_free()

func _on_area_entered(area: Area2D) -> void:
    if area.is_in_group("player") || area.is_in_group("wave"):
        explode()
    elif area.is_in_group("bullets"):
        hit()
