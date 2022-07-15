extends Node2D
class_name TouchController

var touching := false
var double_touching := false
var last_touch_position := Vector2.ZERO
var touch_distance := Vector2.ZERO

func _input(event: InputEvent) -> void:
    if event is InputEventScreenTouch:
        var touch_event: InputEventScreenTouch = event

        # First finger
        if touch_event.index == 0:
            last_touch_position = touch_event.position
            touching = touch_event.pressed
            touch_distance = global_position - touch_event.position

        # Second finger
        elif touch_event.index == 1:
            double_touching = touch_event.pressed

    elif event is InputEventScreenDrag:
        var drag_event: InputEventScreenDrag = event
        if drag_event.index == 0:
            last_touch_position = drag_event.position

func get_computed_position() -> Vector2:
    return last_touch_position + touch_distance

func reset_state() -> void:
    touching = false
    double_touching = false
    last_touch_position = Vector2.ZERO
    touch_distance = Vector2.ZERO
