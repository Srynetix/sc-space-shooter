extends CanvasLayer
class_name Pause

onready var _panel: PopupPanel = $Panel
onready var _button: Button = $Panel/Margin/VBox/Button

func _ready() -> void:
    _button.connect("pressed", self, "_resume")

func _notification(what: int) -> void:
    if what == NOTIFICATION_WM_FOCUS_OUT:
        _pause()

func _pause() -> void:
    _panel.call_deferred("popup_centered")
    get_tree().paused = true

func _resume() -> void:
    _panel.hide()
    get_tree().paused = false
