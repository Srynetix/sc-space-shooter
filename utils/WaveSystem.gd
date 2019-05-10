extends Node

# Wave system

signal timeout

onready var wave_timer = $WaveTimer

var waves = []
var current_wave = 0

###################
# Lifecycle methods

func _ready():
    # Load waves
    self.waves = self._load_wave_file()
    self.wave_timer.connect("timeout", self, "_on_WaveTimer_timeout")

################
# Public methods

func load_next_wave() -> Dictionary:
    self.current_wave += 1
    var wave_info = self._get_current_wave_info()

    self.wave_timer.wait_time = wave_info["duration"]
    self.wave_timer.start()

    return wave_info

#################
# Private methods

func _load_wave_file():
    # Load waves
    var wave_file = File.new()
    wave_file.open("res://data/waves.json", File.READ)
    var wave_text = wave_file.get_as_text()
    var wave_list = parse_json(wave_text)
    wave_file.close()

    return wave_list

func _get_current_wave_info() -> Dictionary:
    if len(self.waves) < self.current_wave:
        return self.waves[len(self.waves) - 1]
    return self.waves[self.current_wave - 1]

#################
# Event callbacks

func _on_WaveTimer_timeout():
    self.emit_signal("timeout")
