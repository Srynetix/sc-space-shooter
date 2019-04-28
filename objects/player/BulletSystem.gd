extends Node

###############
# Bullet system

# Sent when fire
signal fire(bullet, pos, speed, type)

# Bullet types
enum BulletType {
    # Simple bullet
    Simple,
    # Double bullets
    Double,
    # Triple bullets
    Triple,
    # Laser bullets
    Laser,
    
    # Slow/Fast bullets
    SlowFast,
    # Automatic
    Automatic,
}

export (PackedScene) var bullet_model
export (PackedScene) var laser_model
export (float) var fire_cooldown = 0.2
export (float) var fire_speed = 1000.0
export (BulletType) var bullet_type = BulletType.Simple

onready var fire_timer = $FireTimer

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

func fire(pos):
    """
    Fire from position.
    
    :param pos:     Position
    """
    if can_shoot:
        can_shoot = false
        
        if bullet_type == BulletType.Simple:
            emit_signal("fire", bullet_model, pos, fire_speed, bullet_type)
        elif bullet_type == BulletType.Double:
            emit_signal("fire", bullet_model, pos - Vector2(20, 0), fire_speed, bullet_type)
            emit_signal("fire", bullet_model, pos + Vector2(20, 0), fire_speed, bullet_type)
        elif bullet_type == BulletType.Triple:
            emit_signal("fire", bullet_model, pos - Vector2(20, 0), fire_speed, bullet_type)
            emit_signal("fire", bullet_model, pos - Vector2(0, 40), fire_speed, bullet_type)
            emit_signal("fire", bullet_model, pos + Vector2(20, 0), fire_speed, bullet_type)
        elif bullet_type == BulletType.Laser:
            emit_signal("fire", laser_model, pos, fire_speed, bullet_type)
        elif bullet_type == BulletType.SlowFast:
            emit_signal("fire", bullet_model, pos, fire_speed, bullet_type)
        elif bullet_type == BulletType.Automatic:
            emit_signal("fire", bullet_model, pos, fire_speed, bullet_type)
            
        fire_timer.start()
        
#################
# Event callbacks

func _on_FireTimer_timeout():
    can_shoot = true