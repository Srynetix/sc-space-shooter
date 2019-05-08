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
    self.fire_timer.wait_time = self.fire_cooldown
    self.fire_timer.connect("timeout", self, "_on_FireTimer_timeout")
    
    self.switch_type(self.bullet_type)

################
# Public methods

func reset_weapons():
    """Reset current bullet type."""
    self.switch_type(BulletType.Simple)

func upgrade_weapon():
    """Upgrade bullet type."""
    if self.bullet_type == BulletType.Simple:
        self.switch_type(BulletType.Double)
    elif self.bullet_type == BulletType.Double:
        self.switch_type(BulletType.Triple)
    elif self.bullet_type == BulletType.Triple:
        self.switch_type(BulletType.Laser)
        
func switch_type(type):
    """
    Switch type to another.
    
    :param type:    New type
    """
    if type == BulletType.Laser:
        self.fire_timer.wait_time = self.fire_cooldown / 4
    else:
        self.fire_timer.wait_time = self.fire_cooldown
        
    self.bullet_type = type
    
func switch_random_type():
    """
    Switch to random type.
    """
    var weapon_id = int(rand_range(0, 5))
    if weapon_id == 0:
        self.switch_type(BulletType.Simple)
    elif weapon_id == 1:
        self.switch_type(BulletType.Double)
    elif weapon_id == 2:
        self.switch_type(BulletType.Triple)
    elif weapon_id == 3:
        self.switch_type(BulletType.Laser)
    elif weapon_id == 4:
        self.switch_type(BulletType.SlowFast)

func fire(pos):
    """
    Fire from position.
    
    :param pos:     Position
    """
    if self.can_shoot:
        self.can_shoot = false
        
        if self.bullet_type == BulletType.Simple:
            self.emit_signal("fire", self.bullet_model, pos, self.fire_speed, self.bullet_type, self.bullet_target, self.bullet_automatic)
        elif self.bullet_type == BulletType.Double:
            self.emit_signal("fire", self.bullet_model, pos - Vector2(20, 0), self.fire_speed, self.bullet_type, self.bullet_target, self.bullet_automatic)
            self.emit_signal("fire", self.bullet_model, pos + Vector2(20, 0), self.fire_speed, self.bullet_type, self.bullet_target, self.bullet_automatic)
        elif self.bullet_type == BulletType.Triple:
            self.emit_signal("fire", self.bullet_model, pos - Vector2(20, 0), self.fire_speed, self.bullet_type, self.bullet_target, self.bullet_automatic)
            self.emit_signal("fire", self.bullet_model, pos - Vector2(0, 40), self.fire_speed, self.bullet_type, self.bullet_target, self.bullet_automatic)
            self.emit_signal("fire", self.bullet_model, pos + Vector2(20, 0), self.fire_speed, self.bullet_type, self.bullet_target, self.bullet_automatic)
        elif self.bullet_type == BulletType.Laser:
            self.emit_signal("fire", self.bullet_model, pos, self.fire_speed, self.bullet_type, self.bullet_target, self.bullet_automatic)
        elif bullet_type == BulletType.SlowFast:
            self.emit_signal("fire", self.bullet_model, pos, self.fire_speed, self.bullet_type, self.bullet_target, self.bullet_automatic)
            
        self.sound.play()
        self.fire_timer.start()
        
#################
# Event callbacks

func _on_FireTimer_timeout():
    self.can_shoot = true