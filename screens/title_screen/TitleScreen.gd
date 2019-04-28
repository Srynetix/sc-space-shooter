extends Control

##############
# Title screen

onready var instructions = $Instructions
onready var animation_player = $AnimationPlayer
onready var high_score = $VBoxContainer/HSValue

###################
# Lifecycle methods

func _ready():
    if Utils.is_mobile_platform():
        instructions.text = "Touch to start"
    else:
        instructions.text = "Press X to start"
        
    high_score.text = str(GameState.high_score)
    
    animation_player.play("title")
    yield(animation_player, "animation_finished")
    animation_player.play("instructions")
    
func _process(delta):
    if Input.is_action_just_pressed("player_shoot"):
        _load_next()

func _input(event):
    if event is InputEventScreenTouch:
        _load_next()
        
#################
# Private methods
        
func _load_next():
    set_process(false)
    set_process_input(false)
    GameState.load_screen(GameState.Screens.GAME)