extends "res://objects/spawners/Spawner.gd"

#################
# Powerup spawner

# Sent on activation
signal powerup

#################
# Private methods

func _connect_instance(inst):
    inst.connect("powerup", self, "_on_Element_powerup")
    
#################
# Event callbacks

func _on_Element_powerup(powerup_type):
    emit_signal("powerup", powerup_type)