extends Node2D

onready var player_bullets = $PlayerBullets
onready var enemy_bullets = $EnemyBullets
onready var laser_bullets = $LaserBullets

func _ready():
    self.player_bullets.connect("fire", self, "_on_bullets_fire")
    self.enemy_bullets.connect("fire", self, "_on_bullets_fire")
    self.laser_bullets.connect("fire", self, "_on_bullets_fire")
    
    self.player_bullets.fire(self.player_bullets.position)
    self.enemy_bullets.fire(self.enemy_bullets.position)
    self.laser_bullets.fire(self.laser_bullets.position)
    
func _on_bullets_fire(bullet, pos, speed, type, target, automatic):
    var inst = bullet.instance()
    inst.prepare(pos, speed, type, target, automatic)
    self.add_child(inst)
