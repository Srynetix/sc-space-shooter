extends Node2D
class_name Sparkles

onready var _particles: CPUParticles2D = $Particles2D
onready var _timer: Timer = $Timer

func _ready() -> void:
    _timer.connect("timeout", self, "queue_free")
