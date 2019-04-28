extends "res://objects/spawners/Spawner.gd"

###############
# Enemy spawner

# Sent when exploded
signal exploded
# Sent when fire
signal fire(bullet, pos, speed, type)

#################
# Private methods

func _connect_instance(inst):
    inst.connect("exploded", self, "_on_Element_exploded")
    inst.connect("fire", self, "_on_Element_fire")
    
#################
# Event callbacks

func _on_Element_exploded():
    emit_signal("exploded")
    
func _on_Element_fire(bullet, pos, speed, type):
    emit_signal("fire", bullet, pos, speed, type)