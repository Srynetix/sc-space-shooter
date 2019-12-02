extends Node2D

onready var spawner = $EnemySpawner

func _ready():
    self.spawner.connect_target_scene(self, {"fire": "_on_enemyspawner_fire"})

func _on_enemyspawner_fire(bullet, pos, speed, type, target, automatic):
    var inst = bullet.instance()
    inst.prepare(pos, speed, type, target, automatic)
    self.add_child(inst)
