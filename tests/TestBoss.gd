extends Node2D

const BulletType = preload("res://objects/bullet/BulletType.gd").BulletType
const BossModel = preload("res://objects/enemies/BossEnemy.tscn")

onready var player = $Player

func _ready():
    var screen_size = get_viewport().size
    
    player.connect("fire", self, "_on_fire")
    player.connect("dead", self, "_on_Player_dead")
    
    var boss_inst = BossModel.instance()
    boss_inst.connect("fire", self, "_on_fire")
    boss_inst.prepare(Vector2(screen_size.x / 2, -100), 100, 1.0)    
    add_child(boss_inst)
    
func _on_fire(bullet, pos, speed, type, target, automatic):
    var inst = bullet.instance()
    inst.prepare(pos, speed, type, target, automatic)
    
    add_child(inst)
    
func _on_Player_dead():
    player.respawn()