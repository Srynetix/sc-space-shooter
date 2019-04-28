extends "res://objects/spawners/Spawner.gd"

##############
# Rock spawner

# Sent when exploded
signal exploded

#################
# Private methods

func _connect_instance(inst):
    inst.connect("exploded", self, "_on_Element_exploded")
    
#################
# Event callbacks

func _on_Element_exploded():
    emit_signal("exploded")