extends Node2D
class_name Spawner

signal spawn(node)

onready var _timer: Timer = $Timer
onready var _elements: Node2D = $Elements

export var frequency := 2.0
export var speed := 100.0
export var time_offset := 0.0
export var rand_scale := Vector2(0.5, 1.5)
export var element: PackedScene
export var disabled := false
export var target_container_path: NodePath

var _parent_scene: Node
var _target_container: Node2D
var _signals: Dictionary

func _ready() -> void:
    if target_container_path:
        _target_container = get_node(target_container_path)
    else:
        _target_container = _elements

    _timer.wait_time = frequency
    _timer.connect("timeout", self, "_on_timer_timeout")

    if time_offset > 0:
        yield(get_tree().create_timer(time_offset), "timeout")

    _timer.start()

func connect_target_scene(scene: Node, signals_def: Dictionary) -> void:
    _parent_scene = scene
    _signals = signals_def

func reset() -> void:
    disabled = false
    _timer.start()

func reset_then_spawn() -> void:
    reset()
    spawn()

func disable() -> void:
    disabled = true

func set_frequency(freq: float) -> void:
    frequency = freq
    _timer.wait_time = freq

func spawn_at_position(pos: Vector2) -> Node:
    var instance := element.instance()
    _prepare_instance(instance, pos)

    for k in _signals:
        var fn_name: String = _signals[k]
        instance.connect(k, _parent_scene, fn_name)

    _target_container.add_child(instance)
    emit_signal("spawn", instance)
    return instance

func spawn_centered() -> Node:
    var game_size := GameState.get_game_size()
    var pos := Vector2(game_size.x / 2, -50.0)
    return spawn_at_position(pos)

func _prepare_instance(inst: Node, pos: Vector2) -> void:
    inst.call("prepare", pos, speed, rand_range(rand_scale.x, rand_scale.y))

func spawn() -> Node:
    var game_size := GameState.get_game_size()
    var min_pos := game_size.x / 4.0
    var max_pos := game_size.x - game_size.x / 4.0
    var pos := Vector2(rand_range(min_pos, max_pos), -50.0)
    return spawn_at_position(pos)

func _on_timer_timeout() -> void:
    if disabled:
        return

    call_deferred("spawn")

func get_elements() -> Array:
    return _target_container.get_children()
