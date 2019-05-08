extends Control

onready var scores = $MarginTop/MarginContainer/Scores

###################
# Lifecycle methods

func _ready():
    var high_scores = GameState.get_high_scores()
    var high_scores_str = ""
    
    for entry in high_scores:
        var name = entry[0]
        var high_score = entry[1]
        high_scores_str += "{name} {high_score}\n".format({"name": name, "high_score": high_score})
        
    self.scores.text = high_scores_str
    
    yield(self.get_tree().create_timer(3), "timeout")
    GameState.load_screen(GameState.Screens.TITLE)    