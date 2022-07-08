tool
extends Label
class_name AnimatedText

enum Style {
    NORMAL = 1,
    SHAKE,
    WAVE
}

export var animated_text_value := ""
export(Style) var animated_style := Style.NORMAL
export var wave_amplitude := 2.0
export var wave_frequency := 10.0
export var wave_speed := 1.0
export var w_separation := 0.0
export var shake_amplitude := 0.5
export var shake_frequency := 75.0

var _wave_x := 0.0
var _wave_x_limit := 2.0 * PI
var _style_draw := Rect2()

func _process(delta: float) -> void:
    _wave_process(delta)
    update()

func _wave_process(delta: float) -> void:
    _wave_x += delta * wave_speed

    # Regulate
    while _wave_x > _wave_x_limit:
        _wave_x -= _wave_x_limit

func _notification(what: int) -> void:
    if what == NOTIFICATION_DRAW:
        var canvas_item_rid := get_canvas_item()
        var font := get_font("font")
        var style := get_stylebox("normal")
        var size := rect_size
        var text_value := tr(animated_text_value)
        var text_size := font.get_string_size(text_value)
        var char_count := len(text_value)

        # Style
        _style_draw.position = Vector2.ZERO
        _style_draw.size = size
        style.draw(canvas_item_rid, _style_draw)

        # Compute X offset
        var x_offset := 0.0
        if align in [Label.ALIGN_FILL, Label.ALIGN_LEFT]:
            x_offset = style.get_offset().x
        elif align == Label.ALIGN_CENTER:
            x_offset = int((size.x - text_size.x - (w_separation * char_count)) / 2.0)
        elif align == Label.ALIGN_RIGHT:
            x_offset = int(size.x - text_size.x - (w_separation * char_count) - style.get_margin(MARGIN_RIGHT))

        var y_offset := style.get_offset().y + font.get_ascent()

        var comp_pos := Vector2(x_offset, y_offset)
        var draw_pos := _calculate_draw_pos(0, comp_pos, comp_pos, 0)

        # Drawing
        for i in range(len(text_value)):
            var ch := str(text_value[i])
            var next_ch := text_value[i + 1] if (i + 1) < len(text_value) else ""
            var advance := draw_char(font, draw_pos, ch, next_ch, Color.white) + w_separation
            draw_pos = _calculate_draw_pos(i, comp_pos, draw_pos, advance)

func _calculate_draw_pos(char_idx: int, initial_pos: Vector2, previous_pos: Vector2, x_advance: float) -> Vector2:
    match animated_style:
        Style.WAVE:
            var nx := cos(char_idx + (_wave_x * wave_frequency / 2.0)) * wave_amplitude
            var ny := sin(char_idx + (_wave_x * wave_frequency)) * wave_amplitude
            return Vector2(previous_pos.x + x_advance + nx, initial_pos.y + ny)
        Style.SHAKE:
            var ny := sin(char_idx + (_wave_x * shake_frequency)) * shake_amplitude
            return Vector2(previous_pos.x + x_advance, initial_pos.y + ny)

    # Normal
    return Vector2(previous_pos.x + x_advance, previous_pos.y)
