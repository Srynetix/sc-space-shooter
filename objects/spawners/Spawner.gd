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

###################
# Lifecycle methods

func _ready():
    timer.wait_time = frequency
    timer.connect("timeout", self, "_on_Timer_timeout")
    
    if time_offset > 0:
        yield(get_tree().create_timer(time_offset), "timeout")
        
    timer.start()
    
################
# Public methods

func reset():
    """Reset spawner."""
    disabled = false
    timer.start()
    
func disable():
    """Disable spawner."""
    disabled = true
    
func set_frequency(freq):
    """
    Set spawner frequency.
    
    :param freq:    Frequency
    """
    frequency = freq
    timer.wait_time = freq
    
#################
# Private methods

func _connect_instance(inst):
    pass
    
#################
# Event callbacks

func _on_Timer_timeout():
    if disabled:
        return
        
    var vp_width = get_viewport().size.x
    var pos = Vector2(randi() % int(vp_width), -50)
    
    var inst = element.instance()
    _connect_instance(inst)
    inst.prepare(pos, speed, rand_range(rand_scale.x, rand_scale.y))

    elements.add_child(inst)