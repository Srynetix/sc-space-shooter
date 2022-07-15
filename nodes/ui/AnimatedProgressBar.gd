extends ProgressBar
class_name AnimatedProgressBar

const FADE_SPEED := 1.0

onready var _timer: Timer = $Timer
onready var _tween: Tween = $Tween

var _running := false

func _ready() -> void:
    _timer.connect("timeout", self, "_on_timer_timeout")
    modulate = Color.transparent

func fade_in() -> void:
    if !_running:
        _running = true
        _fade_in()
    else:
        _timer.stop()
        _timer.start()

func _fade_in() -> void:
    _tween.stop_all()
    _tween.interpolate_property(self, "modulate", Color.transparent, Color.white, FADE_SPEED)
    _tween.start()

    yield(_tween, "tween_all_completed")
    _tween.start()

func _fade_out() -> void:
    _tween.stop_all()
    _tween.interpolate_property(self, "modulate", modulate, Color.transparent, FADE_SPEED)
    _tween.start()

func _on_timer_timeout() -> void:
    if _running:
        _running = false
        call_deferred("_fade_out")
