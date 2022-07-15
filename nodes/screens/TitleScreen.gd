extends Control
class_name TitleScreen

onready var _high_score: Label = $Margin/All/Top/HSValue
onready var _buttons: VBoxContainer = $Margin/All/Buttons
onready var _start_game_button: Button = $Margin/All/Buttons/StartGame
onready var _options_button: Button = $Margin/All/Buttons/Options
onready var _tests_button: Button = $Margin/All/Buttons/Tests
onready var _options_buttons: VBoxContainer = $Margin/All/OptionsButtons
onready var _options_back_button: Button = $Margin/All/OptionsButtons/BackButton
onready var _how_to_play_button: Button = $Margin/All/OptionsButtons/HowToPlay
onready var _languages_button: OptionButton = $Margin/All/OptionsButtons/Languages
onready var _sound: AudioStreamPlayer = $Sound
onready var _version: Label = $Margin/All/Bottom/Version
onready var _title_label: Label = $Margin/All/Margin/Title
onready var _tween: Tween = $Tween

func _ready() -> void:
    _version.text = tr("TITLE_VERSION") + " " + GameState.get_version_number()

    var high_score_entry := GameState.get_high_score()
    _high_score.text = "%s %d" % [high_score_entry[0], high_score_entry[1]]

    var locale := GameState.get_language()
    if locale.begins_with("fr"):
        _languages_button.select(1)
    else:
        _languages_button.select(0)

    _title_label.modulate = Color.transparent
    _buttons.modulate = Color.transparent

    _tween.interpolate_property(_title_label, "modulate", null, Color.white, 2.0)
    _tween.start()
    yield(_tween, "tween_all_completed")

    _tween.interpolate_property(_buttons, "modulate", null, Color.white, 2.0, Tween.TRANS_LINEAR, Tween.EASE_IN_OUT)
    _tween.start()
    _connect_buttons()
    yield(_tween, "tween_all_completed")

func _disable_buttons() -> void:
    _start_game_button.mouse_filter = MOUSE_FILTER_IGNORE
    _tests_button.mouse_filter = MOUSE_FILTER_IGNORE
    _options_button.mouse_filter = MOUSE_FILTER_IGNORE
    _options_back_button.mouse_filter = MOUSE_FILTER_IGNORE
    _how_to_play_button.mouse_filter = MOUSE_FILTER_IGNORE

func _enable_buttons() -> void:
    _start_game_button.mouse_filter = MOUSE_FILTER_PASS
    _tests_button.mouse_filter = MOUSE_FILTER_PASS
    _options_button.mouse_filter = MOUSE_FILTER_PASS
    _options_back_button.mouse_filter = MOUSE_FILTER_PASS
    _how_to_play_button.mouse_filter = MOUSE_FILTER_PASS

func _connect_buttons() -> void:
    _start_game_button.connect("pressed", self, "_load_next")
    _tests_button.connect("pressed", self, "_load_tests")
    _options_button.connect("pressed", self, "_show_options")
    _options_back_button.connect("pressed", self, "_hide_options")
    _languages_button.connect("item_selected", self, "_change_language")
    _how_to_play_button.connect("pressed", self, "_load_tutorial")

func _notification(what: int) -> void:
    if what == NOTIFICATION_WM_GO_BACK_REQUEST:
        get_tree().quit()

func _show_options() -> void:
    _disable_buttons()

    _tween.interpolate_property(_buttons, "modulate", null, Color.transparent, 1.0, Tween.TRANS_LINEAR, Tween.EASE_IN_OUT)
    _tween.start()
    yield(_tween, "tween_all_completed")

    _buttons.hide()
    _options_buttons.show()
    _options_buttons.modulate = Color.transparent

    _tween.interpolate_property(_options_buttons, "modulate", null, Color.white, 1.0, Tween.TRANS_LINEAR, Tween.EASE_IN_OUT)
    _tween.start()
    yield(_tween, "tween_all_completed")

    _enable_buttons()

func _hide_options() -> void:
    _disable_buttons()

    _tween.interpolate_property(_options_buttons, "modulate", null, Color.transparent, 1.0, Tween.TRANS_LINEAR, Tween.EASE_IN_OUT)
    _tween.start()
    yield(_tween, "tween_all_completed")

    _options_buttons.hide()
    _buttons.show()
    _buttons.modulate = Color.transparent

    _tween.interpolate_property(_buttons, "modulate", null, Color.white, 1.0, Tween.TRANS_LINEAR, Tween.EASE_IN_OUT)
    _tween.start()
    yield(_tween, "tween_all_completed")

    _enable_buttons()

func _load_next() -> void:
    if !GameState.was_tutorial_shown():
        _load_screen(GameState.Screens.TUTORIAL)
    else:
        _load_screen(GameState.Screens.GAME)

func _load_screen(screen: int) -> void:
    _disable_buttons()
    _sound.play()
    set_process(false)
    set_process_input(false)
    GameState.load_screen(screen)

func _load_tutorial() -> void:
    _load_screen(GameState.Screens.TUTORIAL)

func _change_language(selected: int) -> void:
    if selected == 0:
        GameState.set_language("en")
    elif selected == 1:
        GameState.set_language("fr")

func _load_tests() -> void:
    _load_screen(GameState.Screens.TESTS)
