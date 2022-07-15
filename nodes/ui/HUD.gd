extends Control
class_name HUD

onready var _score_value: Label = $MarginContainer/HBoxContainer/VBoxContainer/ScoreValue
onready var _high_value: Label = $MarginContainer/HBoxContainer/VBoxContainer2/HighValue
onready var _lives_value: Label = $MarginContainer/HBoxContainer/VBoxContainer3/LivesValue
onready var _notification: Label = $MarginContainer/HBoxContainer2/Notification
onready var _animation_player: AnimationPlayer = $AnimationPlayer

func _ready():
    GameState.connect("lives_updated", self, "_update_lives")
    GameState.connect("score_updated", self, "_update_score")
    GameState.connect("high_score_updated", self, "_update_high_score")

func show_message(msg: String) -> void:
    _notification.text = msg
    _animation_player.stop()
    _animation_player.play("message")

func _update_lives(lives: int) -> void:
    _lives_value.text = str(lives)

func _update_score(score: int) -> void:
    _score_value.text = str(score)

func _update_high_score(name: String, score: int) -> void:
    _high_value.text = "%s %d" % [name, score]
