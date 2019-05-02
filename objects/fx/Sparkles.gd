extends Node2D

onready var particles = $Particles2D
onready var timer = $Timer

###################
# Lifecycle methods

func _ready():
    timer.connect("timeout", self, "_on_Timer_timeout")
    particles.emitting = true
    
#################
# Event callbacks

func _on_Timer_timeout():
    queue_free()
