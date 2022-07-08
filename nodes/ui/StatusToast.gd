extends Node2D
class_name StatusToast

enum Direction {
    UP = 0,
    DOWN
}

enum AnimationStep {
    NONE = 0,
    FADE_IN,
    WAIT,
    FADE_OUT
}

signal message_shown(msg)
signal message_all_shown()

export(Direction) var toast_direction := Direction.UP
export var message_offset := Vector2(0, -60.0)
export var message_visible_time := 1.0
export var message_anim_speed := 0.5

onready var _label: Label = $Label
onready var _timer: Timer = $Timer
onready var _tween: Tween = $Tween

var _message_queue := []
var _running := false
var _initial_label_position := Vector2.ZERO
var _anim_step: int = AnimationStep.NONE
var _initial_font: DynamicFont
var _overriden_font: DynamicFont

func _ready() -> void:
    _label.text = ""
    _timer.wait_time = message_visible_time
    _initial_label_position = _label.rect_position
    _timer.connect("timeout", self, "_on_timer_timeout")
    _tween.connect("tween_all_completed", self, "_on_tween_all_completed")
    _label.rect_min_size = Vector2(get_viewport_rect().size.x, 8)

    _initial_font = _label.get_font("font") as DynamicFont

func set_message_visible_time(time: float) -> void:
    _timer.wait_time = time

func stop() -> void:
    _tween.reset_all()
    _timer.stop()
    _anim_step = AnimationStep.NONE
    _label.text = ""

func show_message(message: String) -> void:
    show_message_with_color(message, Color.white)

func show_message_with_color(message: String, color: Color) -> void:
    _show_message_with_color(message, color, false)

func show_priority_message(message: String) -> void:
    show_priority_message_with_color(message, Color.white)

func show_priority_message_with_color(message: String, color: Color) -> void:
    _show_message_with_color(message, color, true)

func set_text_size(text_size: int) -> void:
    _overriden_font = DynamicFont.new()
    _overriden_font.font_data = _initial_font.font_data
    _overriden_font.size = text_size

    _label.add_font_override("font", _overriden_font)

func _show_message_with_color(message: String, color: Color, priority: bool) -> void:
    if _running:
        if priority:
            _message_queue.clear()
            stop()
        else:
            _message_queue.push_back([message, color])
            return

    _message_start(message, color)

func _message_fade_in(message: String, color: Color) -> void:
    var computed_message_offset := message_offset if toast_direction == Direction.UP else -message_offset
    _label.modulate = Color.transparent
    _label.text = message
    _label.rect_position = Vector2(_initial_label_position.x - _label.get_combined_minimum_size().x / 2.0, _initial_label_position.y)

    _anim_step = AnimationStep.FADE_IN

    _tween.interpolate_property(_label, "modulate", Color.transparent, color, message_anim_speed)
    _tween.interpolate_property(_label, "rect_position", _label.rect_position, _label.rect_position + computed_message_offset, message_anim_speed)
    _tween.start()

func _message_fade_out() -> void:
    _anim_step = AnimationStep.FADE_OUT

    _tween.interpolate_property(_label, "modulate", _label.modulate, Color.transparent, message_anim_speed)
    _tween.start()

func _message_wait() -> void:
    _anim_step = AnimationStep.WAIT
    _timer.start()

func _message_start(message: String, color: Color) -> void:
    _running = true
    _message_fade_in(message, color)

func _message_end() -> void:
    _running = false
    _anim_step = AnimationStep.NONE
    emit_signal("message_shown")

    if len(_message_queue) > 0:
        var message_args: Array = _message_queue.pop_front()
        var new_message: String = message_args[0]
        var new_color: Color = message_args[1]
        call_deferred("_show_message_with_color", new_message, new_color, false)
    else:
        emit_signal("message_all_shown")

func _on_tween_all_completed() -> void:
    if !_running:
        return

    if _anim_step == AnimationStep.FADE_IN:
        _message_wait()
    elif _anim_step == AnimationStep.FADE_OUT:
        _message_end()

func _on_timer_timeout() -> void:
    if !_running:
        return

    _message_fade_out()
