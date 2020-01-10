extends Control

onready var timer = $Timer
onready var camera = $FXCamera

func _ready():
    timer.connect("timeout", self, "_on_timeout")

func _on_timeout():
    camera.Shake()
