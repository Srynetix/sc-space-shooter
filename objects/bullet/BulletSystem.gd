extends Node

###############
# Bullet system

# Sent when fire
signal fire(bullet, pos, speed, type, target, automatic)

# Bullet type
const BulletType = preload("res://objects/bullet/BulletType.gd").BulletType
# Bullet target
const BulletTarget = preload("res://objects/bullet/BulletTarget.gd").BulletTarget

export (float) var fire_cooldown = 0.2
export (float) var fire_speed = 1000.0
export (PackedScene) var bullet_model = null
export (BulletType) var bullet_type = BulletType.Simple
export (BulletTarget) var bullet_target = BulletTarget.Enemy
export (bool) var bullet_automatic = false

onready var fire_timer = $FireTimer
onready var sound = $Sound

var can_shoot = true

###################
# Lifecycle methods

func _ready():
    fire_timer.wait_time = fire_cooldown
    fire_timer.connect("timeout", self, "_on_FireTimer_timeout")
    
    switch_type(bullet_type)

################
# Public methods

func reset_weapons():
    """Reset current bullet type."""
    switch_type(BulletType.Simple)

func upgrade_weapon():
    """Upgrade bullet type."""
    if bullet_type == BulletType.Simple:
        switch_type(BulletType.Double)
    elif bullet_type == BulletType.Double:
        switch_type(BulletType.Triple)
    elif bullet_type == BulletType.Triple:
        switch_type(BulletType.Laser)
        
func switch_type(type):
    """
    Switch type to another.
    
    :param type:    New type
    """
    if type == BulletType.Laser:
        fire_timer.wait_time = fire_cooldown / 4
    else:
        fire_timer.wait_time = fire_cooldown
        
    bullet_type = type
    
func switch_random_type():
    """
    Switch to random type.
    """
    var weapon_id = int(rand_range(0, 5))
    if weapon_id == 0:
        switch_type(BulletType.Simple)
    elif weapon_id == 1:
        switch_type(BulletType.Double)
    elif weapon_id == 2:
        switch_type(BulletType.Triple)
    elif weapon_id == 3:
        switch_type(BulletType.Laser)
    elif weapon_id == 4:
        switch_type(BulletType.SlowFast)

func fire(pos):
    """
    Fire from position.
    
    :param pos:     Position
    """
    if can_shoot:
        can_shoot = false
        
        if bullet_type == BulletType.Simple:
            emit_signal("fire", bullet_model, pos, fire_speed, bullet_type, bullet_target, bullet_automatic)
        elif bullet_type == BulletType.Double:
            emit_signal("fire", bullet_model, pos - Vector2(20, 0), fire_speed, bullet_type, bullet_target, bullet_automatic)
            emit_signal("fire", bullet_model, pos + Vector2(20, 0), fire_speed, bullet_type, bullet_target, bullet_automatic)
        elif bullet_type == BulletType.Triple:
            emit_signal("fire", bullet_model, pos - Vector2(20, 0), fire_speed, bullet_type, bullet_target, bullet_automatic)
            emit_signal("fire", bullet_model, pos - Vector2(0, 40), fire_speed, bullet_type, bullet_target, bullet_automatic)
            emit_signal("fire", bullet_model, pos + Vector2(20, 0), fire_speed, bullet_type, bullet_target, bullet_automatic)
        elif bullet_type == BulletType.Laser:
            emit_signal("fire", bullet_model, pos, fire_speed, bullet_type, bullet_target, bullet_automatic)
        elif bullet_type == BulletType.SlowFast:
            emit_signal("fire", bullet_model, pos, fire_speed, bullet_type, bullet_target, bullet_automatic)
            
        sound.play()
        fire_timer.start()
        
#################
# Event callbacks

func _on_FireTimer_timeout():
    can_shoot = true