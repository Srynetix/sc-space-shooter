extends Node2D

onready var particles = $Particles2D
onready var timer = $Timer

###################
# Lifecycle methods

func _ready():
    self.timer.connect("timeout", self, "_on_Timer_timeout")
    self.particles.emitting = true
    
#################
# Event callbacks

func _on_Timer_timeout():
    self.queue_free()
