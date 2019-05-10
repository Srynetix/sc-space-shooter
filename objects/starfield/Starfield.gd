extends Control

###########
# Starfield

export (int) var velocity = 500

onready var particles = $Particles2D

func _ready():
    self.particles.process_material.initial_velocity = self.velocity
