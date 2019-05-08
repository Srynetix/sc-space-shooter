extends Control

#####
# HUD

onready var score_value = $MarginContainer/HBoxContainer/VBoxContainer/ScoreValue
onready var high_value = $MarginContainer/HBoxContainer/VBoxContainer2/HighValue
onready var lives_value = $MarginContainer/HBoxContainer/VBoxContainer3/LivesValue
onready var notification = $MarginContainer/HBoxContainer2/Notification
onready var animation_player = $AnimationPlayer

################
# Public methods

func update_lives(lives):
    """
    Update lives.
    
    :param lives:   Lives
    """
    self.lives_value.text = str(lives)
    
func update_score(score):
    """
    Update score.
    
    :param score:   Score
    """
    self.score_value.text = str(score)
    
func show_message(msg):
    """
    Show message.
    
    :param msg:     Message
    """
    self.notification.text = msg
    self.animation_player.stop()
    self.animation_player.play("message")

func update_high_score(name, score):
    """
    Update high score.
    
    :param name:    Name
    :param score:   Score
    """
    self.high_value.text = "{name} {score}".format({"name": name, "score": score})