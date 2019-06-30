extends Node2D

##########
# FXCamera

onready var animation_player = $AnimationPlayer

################
# Public methods

func shake():
    self.animation_player.play("shake")
