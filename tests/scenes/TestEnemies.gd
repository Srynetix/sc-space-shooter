extends Node2D

onready var spawner = $EnemySpawner

func _ready():
    self.spawner.ConnectTargetScene(self, {"fire": "_on_enemyspawner_fire"})

func _on_enemyspawner_fire(bullet, pos, speed, type, target, automatic):
    var inst = bullet.instance()
    inst.Prepare(pos, speed, type, target, automatic)
    self.add_child(inst)
