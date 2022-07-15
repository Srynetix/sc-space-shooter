extends Area2D
class_name FXWave

signal finished()

const SPARKLES_SCENE: PackedScene = preload("res://nodes/fx/Sparkles.tscn")

onready var _tween: Tween = $Tween
onready var _sprite: Sprite = $Sprite
onready var _collision_shape: CollisionShape2D = $CollisionShape2D
onready var _particles: CPUParticles2D = $Particles2D
onready var _sound: AudioStreamPlayer = $AudioStreamPlayer

onready var _shape: Shape2D = _collision_shape.shape
onready var _shader_material: ShaderMaterial = _sprite.material

func _ready() -> void:
    connect("area_entered", self, "_on_area_entered")

    var game_size := GameState.get_game_size()
    var max_v := game_size.x
    _sprite.scale = Vector2(max_v, max_v) * 1.75 / _sprite.texture.get_size()

func start(target: Vector2, force: float = 1.0) -> void:
    var game_size := GameState.get_game_size()
    var max_v := game_size.x * force

    _sprite.position = target
    _particles.position = target
    _collision_shape.position = target

    var final_particles_velocity := max_v * 20 * force
    _tween.stop_all()
    _tween.interpolate_property(_shader_material, "shader_param/radius", 0, force, 3.0, Tween.TRANS_EXPO, Tween.EASE_OUT)
    _tween.interpolate_property(_shader_material, "shader_param/waveColor", Color.transparent, Color8(0, 255, 255, 128), 2.0, Tween.TRANS_EXPO, Tween.EASE_OUT)
    _tween.interpolate_property(_shape, "radius", max_v * 1.75, 3.0, Tween.TRANS_EXPO, Tween.EASE_OUT)
    _tween.interpolate_property(_particles, "initial_velocity", 10, final_particles_velocity, 3.0, Tween.TRANS_EXPO, Tween.EASE_OUT)
    _tween.start()
    _sound.play()
    FXCamera.shake_loop()
    yield(_tween, "tween_all_completed")

    FXCamera.reset()
    _sound.stop()
    _particles.emitting = false

    _tween.interpolate_property(_shader_material, "shader_param/radius", force, 0, 1.0, Tween.TRANS_ELASTIC, Tween.EASE_OUT)
    _tween.interpolate_property(_shape, "radius", max_v * 1.75, 0, 1.0, Tween.TRANS_ELASTIC, Tween.EASE_OUT)
    _tween.interpolate_property(_shader_material, "shader_param/waveColor", Color8(0, 255, 255, 128), Color.transparent, 1.0, Tween.TRANS_EXPO, Tween.EASE_IN)
    _tween.interpolate_property(_particles, "initial_velocity", _particles.initial_velocity, 0, 0.5, Tween.TRANS_EXPO, Tween.EASE_IN_OUT)
    _tween.interpolate_property(_particles, "orbit_velocity", _particles.orbit_velocity, 0, 0.5, Tween.TRANS_EXPO, Tween.EASE_IN_OUT)
    _tween.interpolate_property(_particles, "amount", 64, 1, 0.5, Tween.TRANS_LINEAR, Tween.EASE_IN_OUT)
    _tween.start()

    yield(_tween, "tween_all_completed")
    emit_signal("finished")

func start_then_free(target: Vector2, force: float = 1.0) -> void:
    start(target, force)
    yield(self, "finished")

    queue_free()

func _on_area_entered(area: Area2D) -> void:
    if area.is_in_group("rocks") || area.is_in_group("enemies"):
        var sparkles: Sparkles = SPARKLES_SCENE.instance()
        sparkles.position = position
        sparkles.z_index = 10

        get_parent().add_child(sparkles)
