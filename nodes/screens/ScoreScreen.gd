extends Control
class_name ScoreScreen

onready var _scores: Label = $MarginTop/MarginContainer/Scores

func _ready() -> void:
    var high_scores := GameState.get_high_scores()
    var high_scores_str := ""

    for entry in high_scores:
        var name: String = entry[0]
        var score: int = entry[1]
        high_scores_str += "%s %d\n" % [name, score]

    _scores.text = high_scores_str
    yield(get_tree().create_timer(3.0), "timeout")

    GameState.load_screen(GameState.Screens.TITLE)
