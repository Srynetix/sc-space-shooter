extends Control

###########
# Starfield

export (int) var velocity = 500

onready var particles = $Particles2D

func _ready():
    particles.process_material.initial_velocity = velocity