extends Node2D

const BossModel = preload("res://objects/enemies/BossEnemy.tscn")

onready var player = $Player

func _ready():
    var screen_size = Utils.GetGameSize()

    self.player.connect("fire", self, "_on_fire")
    self.player.connect("respawn", self, "_on_Player_respawn")

    var boss_inst = BossModel.instance()
    boss_inst.connect("fire", self, "_on_fire")
    boss_inst.Prepare(Vector2(screen_size.x / 2, -100), 100, 1.0)
    self.add_child(boss_inst)

func _on_fire(bullet, pos, speed, type, target, automatic):
    var inst = bullet.instance()
    inst.Prepare(pos, speed, type, target, automatic)
    self.add_child(inst)

func _on_Player_respawn():
    self.player.Respawn()
