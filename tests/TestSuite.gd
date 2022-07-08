extends Control

onready var runner: SxSceneRunner = $SxSceneRunner

func _ready() -> void:
    runner.connect("scene_loaded", self, "_on_scene_loaded")
    runner.connect("go_back", self, "_on_go_back")

func _on_scene_loaded(_name: String) -> void:
    FXCamera.reset()

func _on_go_back() -> void:
    GameState.load_screen(GameState.Screens.TITLE)
