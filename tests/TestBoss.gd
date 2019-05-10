extends Node2D

const BulletType = preload("res://objects/bullet/BulletType.gd").BulletType
const BossModel = preload("res://objects/enemies/BossEnemy.tscn")

onready var player = $Player

func _ready():
    var screen_size = Utils.get_game_size()

    self.player.connect("fire", self, "_on_fire")
    self.player.connect("dead", self, "_on_Player_dead")

    var boss_inst = BossModel.instance()
    boss_inst.connect("fire", self, "_on_fire")
    boss_inst.prepare(Vector2(screen_size.x / 2, -100), 100, 1.0)
    self.add_child(boss_inst)

func _on_fire(bullet, pos, speed, type, target, automatic):
    var inst = bullet.instance()
    inst.prepare(pos, speed, type, target, automatic)
    self.add_child(inst)

func _on_Player_dead():
    self.player.respawn()
