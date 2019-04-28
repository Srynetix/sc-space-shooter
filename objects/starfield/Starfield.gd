extends Control

###########
# Starfield

onready var particles = $Particles2D

################
# Public methods

func update_velocity(vel):
    """
    Update starfield velocity.
    
    :param vel:     Velocity
    """
    particles.process_material.initial_velocity = vel
