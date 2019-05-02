extends Node2D

onready var spawner = $EnemySpawner

func _ready():
    spawner.connect("fire", self, "_on_enemyspawner_fire")
    
func _on_enemyspawner_fire(bullet, pos, speed, type, target, automatic):
    var inst = bullet.instance()
    inst.prepare(pos, speed, type, target, automatic)
    
    add_child(inst)