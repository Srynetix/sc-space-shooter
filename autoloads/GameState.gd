extends SxGameData

enum Screens {
    BOOT = 0,
    TITLE,
    TUTORIAL,
    GAME,
    GAMEOVER,
    SCORE,
    TESTS
}

const MAX_HIGH_SCORES := 5
const YOUR_NAME := "YOU"
const VERSION_KEY := "version"
const LANGUAGE_KEY := "language"
const TUTORIAL_SHOWN_KEY := "tutorial_shown"
const HIGH_SCORES_KEY := "high_scores"

const DEFAULT_HIGH_SCORES := [
    ["ZZZ", 30000],
    ["AAA", 10000],
    ["BBB", 7000]
]

const SCREEN_MAP := {
    Screens.BOOT: "res://nodes/screens/BootScreen.tscn",
    Screens.TITLE: "res://nodes/screens/TitleScreen.tscn",
    Screens.TUTORIAL: "res://nodes/screens/HowToPlayScreen.tscn",
    Screens.GAME: "res://nodes/screens/GameScreen.tscn",
    Screens.GAMEOVER: "res://nodes/screens/GameOverScreen.tscn",
    Screens.SCORE: "res://nodes/screens/ScoreScreen.tscn",
    Screens.TESTS: "res://tests/TestSuite.tscn",
}

signal lives_updated(lives)
signal score_updated(score)
signal high_score_updated(name, score)

var _score := 0
var _lives := 3
var _language := "en"
var _high_scores: Array
var _configuration: Dictionary
var _tutorial_shown: bool

func _ready():
    SxLog.get_logger("SxGameData").set_max_log_level(SxLog.LogLevel.DEBUG)

    randomize()
    _configuration = SxJson.read_json_file("res://data/config.json")
    load_from_disk()

    _high_scores = load_value(HIGH_SCORES_KEY, DEFAULT_HIGH_SCORES)
    _language = load_value(LANGUAGE_KEY, OS.get_locale())
    TranslationServer.set_locale(_language)
    _tutorial_shown = load_value(TUTORIAL_SHOWN_KEY, false)

func load_screen(screen_type: int) -> void:
    _change_scene(SCREEN_MAP[screen_type])

func add_score(value: int) -> void:
    _score += value
    emit_signal("score_updated", _score)

    var high_score := get_high_score()
    emit_signal("high_score_updated", high_score[0], high_score[1])

func remove_life() -> void:
    _lives = int(max(_lives - 1, 0))
    emit_signal("lives_updated", _lives)

func get_score() -> int:
    return _score

func add_life() -> void:
    _lives += 1
    emit_signal("lives_updated", _lives)

func get_lives() -> int:
    return _lives

func reset_game_state() -> void:
    _lives = 3
    _score = 0
    emit_signal("score_updated", _score)
    emit_signal("lives_updated", _lives)

    var high_score := get_high_score()
    emit_signal("high_score_updated", high_score[0], high_score[1])

func get_high_score() -> Array:
    var high_scores := get_high_scores()
    var first_score: Array = high_scores[0]

    if _score > first_score[1]:
        return [YOUR_NAME, _score]
    else:
        return first_score

func get_high_scores() -> Array:
    if _high_scores == null:
        return DEFAULT_HIGH_SCORES
    else:
        return _high_scores

func was_tutorial_shown() -> bool:
    return _tutorial_shown

func set_tutorial_shown() -> void:
    store_value(TUTORIAL_SHOWN_KEY, true)
    _tutorial_shown = true
    persist_to_disk()

func get_version_number() -> String:
    return _configuration[VERSION_KEY]

func get_game_size() -> Vector2:
    return get_viewport().get_visible_rect().size

func set_language(locale: String) -> void:
    TranslationServer.set_locale(locale)
    _language = locale
    store_value(LANGUAGE_KEY, locale)
    persist_to_disk()

func get_language() -> String:
    return _language

func save_game_over() -> void:
    if _has_high_score():
        var idx := _get_high_score_pos()
        _insert_high_score(idx)
        store_value(HIGH_SCORES_KEY, _high_scores)
        persist_to_disk()

func _has_high_score() -> bool:
    var idx := _get_high_score_pos()
    if idx != -1:
        return true

    return len(_high_scores) < MAX_HIGH_SCORES

func _get_high_score_pos() -> int:
    var idx := 0
    for entry in _high_scores:
        var high_score: int = entry[1]
        if _score > high_score:
            return idx
        idx += 1

    return -1

func _insert_high_score(idx: int) -> void:
    if idx == -1:
        if len(_high_scores) < MAX_HIGH_SCORES:
            _high_scores.append([YOUR_NAME, _score])
    else:
        _high_scores.insert(idx, [YOUR_NAME, _score])
        if len(_high_scores) > MAX_HIGH_SCORES:
            _high_scores.pop_back()

func _change_scene(path: String) -> void:
    GameSceneTransitioner.fade_to_scene_path(path)
