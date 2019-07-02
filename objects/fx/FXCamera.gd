extends Node2D

##########
# FXCamera

onready var animation_player = $AnimationPlayer

###################
# Lifecycle methods

func _ready():
    VisualServer.set_default_clear_color(Color(0.0, 0.0, 0.0, 1.0))

################
# Public methods

func shake():
    self.animation_player.play("shake")
