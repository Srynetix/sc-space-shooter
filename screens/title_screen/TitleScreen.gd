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
        
    var high_score_entry = GameState.get_high_score()
    high_score.text = "{name} {score}".format({"name": high_score_entry[0], "score": high_score_entry[1]})
    
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