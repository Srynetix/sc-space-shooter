extends Control

"""
Scene runner.
"""

signal scene_loaded(name)
signal go_back

export(String, DIR) var scene_folder: String

const SCENE_KEY_RESET = KEY_I
const SCENE_KEY_PREV = KEY_O
const SCENE_KEY_NEXT = KEY_P

onready var current = $Current
onready var scene_name = $CanvasLayer/Margin/VBox/Text/SceneName
onready var back_button = $CanvasLayer/Margin/BackButton
onready var previous_btn = $CanvasLayer/Margin/VBox/Margin/Buttons/Previous
onready var reset_btn = $CanvasLayer/Margin/VBox/Margin/Buttons/Reset
onready var next_btn = $CanvasLayer/Margin/VBox/Margin/Buttons/Next

var known_scenes = []
var current_scene = 0

###################
# Lifecycle methods

func _ready():
    self.known_scenes = self._discover_scenes()
    self._load_first_scene()

    previous_btn.connect("pressed", self, "_load_prev_scene")
    reset_btn.connect("pressed", self, "_load_current_scene")
    next_btn.connect("pressed", self, "_load_next_scene")
    back_button.connect("pressed", self, "_go_back")

func _input(event):
    if event is InputEventKey:
        if event.pressed:
            if event.scancode == SCENE_KEY_NEXT:
                self._load_next_scene()
            elif event.scancode == SCENE_KEY_PREV:
                self._load_prev_scene()
            elif event.scancode == SCENE_KEY_RESET:
                self._load_current_scene()
            elif event.scancode == KEY_ESCAPE:
                self._go_back()

func _notification(what):
    if what == NOTIFICATION_WM_GO_BACK_REQUEST:
        self._go_back()

#################
# Private methods

func _load_first_scene():
    if len(self.known_scenes) == 0:
        self.scene_name.text = tr("SCENE_RUNNER_NO_SCENE")
        return

    self._load_current_scene()

func _discover_scenes():
    var scenes = []
    var dir = Directory.new()
    var idx = 1

    # Stop on empty scene folder
    if scene_folder == "":
        return scenes

    dir.open(scene_folder)
    dir.list_dir_begin()

    while true:
        var file = dir.get_next()
        if file == "":
            break

        if file.ends_with(".tscn"):
            scenes.append([idx, file.trim_suffix(".tscn"), load(scene_folder + "/" + file)])
            idx += 1

    dir.list_dir_end()
    return scenes

func _load_current_scene():
    var entry = self.known_scenes[self.current_scene]
    var entry_idx = entry[0]
    var entry_name = entry[1]
    var entry_model = entry[2]

    # Clear previous
    for child in self.current.get_children():
        child.queue_free()

    # Load instance
    var instance = entry_model.instance()
    self.scene_name.text = str(entry_idx) + " - " + entry_name + "\n" + str(entry_idx) + "/" + str(len(self.known_scenes))
    self.current.add_child(instance)
    emit_signal("scene_loaded", entry_name)

func _load_next_scene():
    if self.current_scene == len(self.known_scenes) - 1:
        self.current_scene = 0
    else:
        self.current_scene += 1
    self._load_current_scene()

func _load_prev_scene():
    if self.current_scene == 0:
        self.current_scene = len(self.known_scenes) - 1
    else:
        self.current_scene -= 1
    self._load_current_scene()

func _go_back():
    back_button.mouse_filter = MOUSE_FILTER_IGNORE
    emit_signal("go_back")
