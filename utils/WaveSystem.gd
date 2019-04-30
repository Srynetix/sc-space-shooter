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
    waves = _load_wave_file()
    
    wave_timer.connect("timeout", self, "_on_WaveTimer_timeout")    

################
# Public methods

func load_next_wave() -> Dictionary:
    current_wave += 1
    var wave_info = _get_current_wave_info()
    
    wave_timer.wait_time = wave_info["duration"]
    wave_timer.start()
    
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
    if len(waves) < current_wave:
        return waves[len(waves) - 1]
    return waves[current_wave - 1]
    
#################
# Event callbacks

func _on_WaveTimer_timeout():
    emit_signal("timeout")