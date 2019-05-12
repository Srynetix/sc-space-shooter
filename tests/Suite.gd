extends Control

############
# Test Suite

const TESTS_FOLDER = "res://tests"
const TEST_KEY_RESET = KEY_I
const TEST_KEY_PREV = KEY_O
const TEST_KEY_NEXT = KEY_P

onready var current = $Current
onready var test_name_label = $CanvasLayer/MarginContainer/Label

var known_tests = []
var current_test = 0

###################
# Lifecycle methods

func _init():
    self.known_tests = self._discover_tests()

func _ready():
    if len(self.known_tests) == 0:
        self.test_name_label.text = "[NO TESTS]"
        return

    self._load_current_test()

func _input(event):
    if event is InputEventKey:
        if event.pressed:
            if event.scancode == TEST_KEY_NEXT:
                self._load_next_test()
            elif event.scancode == TEST_KEY_PREV:
                self._load_prev_test()
            elif event.scancode == TEST_KEY_RESET:
                self._load_current_test()

#################
# Private methods

func _discover_tests():
    var tests = []
    var dir = Directory.new()
    var idx = 1

    dir.open(TESTS_FOLDER)
    dir.list_dir_begin()

    while true:
        var file = dir.get_next()
        if file == "":
            break

        if file.begins_with("Test") && file.ends_with(".tscn"):
            tests.append([idx, file.trim_suffix(".tscn"), load(TESTS_FOLDER + "/" + file)])
            idx += 1

    dir.list_dir_end()
    return tests

func _load_current_test():
    var entry = self.known_tests[self.current_test]
    var entry_idx = entry[0]
    var entry_test = entry[1]
    var entry_model = entry[2]

    # Clear previous
    for child in self.current.get_children():
        child.queue_free()

    var instance = entry_model.instance()
    self.test_name_label.text = str(entry_idx) + " - " + entry_test
    self.current.add_child(instance)

func _load_next_test():
    if self.current_test == len(self.known_tests) - 1:
        self.current_test = 0
    else:
        self.current_test += 1
    self._load_current_test()

func _load_prev_test():
    if self.current_test == 0:
        self.current_test = len(self.known_tests) - 1
    else:
        self.current_test -= 1
    self._load_current_test()
