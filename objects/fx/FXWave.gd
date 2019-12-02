extends Node2D

onready var animation_player = $AnimationPlayer

func _ready():
    self.animation_player.play("wave")
    yield(self.animation_player, "animation_finished")
    self.queue_free()
