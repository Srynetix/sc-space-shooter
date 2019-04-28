extends Control

#####
# HUD

onready var score_value = $MarginContainer/HBoxContainer/VBoxContainer/ScoreValue
onready var lives_value = $MarginContainer/HBoxContainer/VBoxContainer2/LivesValue
onready var notification = $MarginContainer/HBoxContainer2/Notification
onready var animation_player = $AnimationPlayer

################
# Public methods

func update_lives(lives):
    """
    Update lives.
    
    :param lives:   Lives
    """
    lives_value.text = str(lives)
    
func update_score(score):
    """
    Update score.
    
    :param score:   Score
    """
    score_value.text = str(score)
    
func show_message(msg):
    """
    Show message.
    
    :param msg:     Message
    """
    notification.text = msg
    animation_player.stop()
    animation_player.play("message")
