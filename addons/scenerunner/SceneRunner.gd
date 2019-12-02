extends Control

"""
Scene runner.
"""

export(String, DIR) var scene_folder: String

const SCENE_KEY_RESET = KEY_I
const SCENE_KEY_PREV = KEY_O
const SCENE_KEY_NEXT = KEY_P

onready var current = $Current
onready var scene_name = $CanvasLayer/MarginContainer/HBoxContainer/SceneName

var known_scenes = []
var current_scene = 0

###################
# Lifecycle methods
    
func _ready():
    self.known_scenes = self._discover_scenes()
    self._load_first_scene()
    
func _input(event):
    if event is InputEventKey:
        if event.pressed:
            if event.scancode == SCENE_KEY_NEXT:
                self._load_next_scene()
            elif event.scancode == SCENE_KEY_PREV:
                self._load_prev_scene()
            elif event.scancode == SCENE_KEY_RESET:
                self._load_current_scene()

#################
# Private methods

func _load_first_scene():
    if len(self.known_scenes) == 0:
        self.scene_name.text = "[NO SCENE FOUND]"
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
