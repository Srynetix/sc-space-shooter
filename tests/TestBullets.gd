extends Node2D

onready var player_bullets = $PlayerBullets
onready var enemy_bullets = $EnemyBullets
onready var laser_bullets = $LaserBullets

func _ready():
    player_bullets.connect("fire", self, "_on_bullets_fire")
    enemy_bullets.connect("fire", self, "_on_bullets_fire")
    laser_bullets.connect("fire", self, "_on_bullets_fire")
    
    player_bullets.fire(player_bullets.position)
    enemy_bullets.fire(enemy_bullets.position)
    laser_bullets.fire(laser_bullets.position)
    
func _on_bullets_fire(bullet, pos, speed, type, target, automatic):
    var inst = bullet.instance()
    inst.prepare(pos, speed, type, target, automatic)
    
    add_child(inst)
