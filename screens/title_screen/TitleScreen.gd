extends Control

##############
# Title screen

onready var instructions = $Instructions
onready var animation_player = $AnimationPlayer
onready var high_score = $VBoxContainer/HSValue

var instructions_loaded = false

###################
# Lifecycle methods

func _ready():
    if Utils.is_mobile_platform():
        self.instructions.text = "Touch to start"
    else:
        self.instructions.text = "Press X to start"
        
    var high_score_entry = GameState.get_high_score()
    self.high_score.text = "{name} {score}".format({"name": high_score_entry[0], "score": high_score_entry[1]})
    
    self.animation_player.play("title")
    yield(self.animation_player, "animation_finished")
    self.animation_player.play("instructions")
    self.instructions_loaded = true
    
func _process(delta):
    if !self.instructions_loaded:
        return
        
    if Input.is_action_just_pressed("player_shoot"):
        self._load_next()

func _input(event):
    if !self.instructions_loaded:
        return
        
    if event is InputEventScreenTouch:
        self._load_next()
        
func _notification(what):
    if (what == MainLoop.NOTIFICATION_WM_GO_BACK_REQUEST):
        self.get_tree().quit()
        
#################
# Private methods
        
func _load_next():
    self.set_process(false)
    self.set_process_input(false)
    GameState.load_screen(GameState.Screens.GAME)