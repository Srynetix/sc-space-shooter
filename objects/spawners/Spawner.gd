extends Node

#########
# Spawner

export (float) var frequency = 2
export (float) var speed = 100.0
export (float) var time_offset = 0
export (Vector2) var rand_scale = Vector2(0.5, 1.5)
export (PackedScene) var element
export (bool) var disabled = false

onready var timer = $Timer
onready var elements = $Elements

var parent_scene = null
var signals = {}

###################
# Lifecycle methods

func _ready():
    self.timer.wait_time = self.frequency
    self.timer.connect("timeout", self, "_on_Timer_timeout")

    if self.time_offset > 0:
        yield(get_tree().create_timer(self.time_offset), "timeout")

    self.timer.start()

################
# Public methods

func connect_target_scene(scene, signals):
    """Set parent scene."""
    self.parent_scene = scene
    self.signals = signals

func reset():
    """Reset spawner."""
    self.disabled = false
    self.timer.start()

func disable():
    """Disable spawner."""
    self.disabled = true

func set_frequency(freq):
    """
    Set spawner frequency.

    :param freq:    Frequency
    """
    self.frequency = freq
    self.timer.wait_time = freq

func spawn_at_position(pos):
    """
    Spawn at position.

    :param pos: Position
    """
    var inst = self.element.instance()
    inst.prepare(pos, self.speed, rand_range(self.rand_scale.x, self.rand_scale.y))

    for signal_name in self.signals:
        var fn_name = self.signals[signal_name]
        inst.connect(signal_name, self.parent_scene, fn_name)

    self.elements.add_child(inst)

func spawn():
    """
    Spawn element.
    """
    var game_size = Utils.get_game_size()
    var min_pos = game_size.x / 4
    var max_pos = game_size.x - game_size.x / 4
    var pos = Vector2(int(rand_range(min_pos, max_pos)), -50)
    self.spawn_at_position(pos)

#################
# Event callbacks

func _on_Timer_timeout():
    if self.disabled:
        return

    self.spawn()
