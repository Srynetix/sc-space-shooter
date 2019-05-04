extends Area2D

########
# Bullet

const BulletType = preload("res://objects/bullet/BulletType.gd").BulletType
const BulletTarget = preload("res://objects/bullet/BulletTarget.gd").BulletTarget
const Sparkles = preload("res://objects/fx/Sparkles.tscn")

const EnemyBulletSprite = preload("res://assets/textures/laserRed06.png")

# Default target: Enemy
export (BulletTarget) var bullet_target = BulletTarget.Enemy
export (BulletType) var bullet_type = BulletType.Simple
export (bool) var bullet_automatic = false

onready var visibility_notifier = $VisibilityNotifier2D
onready var slow_timer = $SlowTimer
onready var trail = $Trail
onready var sprite = $Sprite

var velocity = Vector2()
var base_speed = 0

###################
# Lifecycle methods

func _ready():
    connect("area_entered", self, "_on_area_entered")
    visibility_notifier.connect("screen_exited", self, "_on_VisibilityNotifier2D_screen_exited")
    
    # Prepare for player target
    if bullet_target == BulletTarget.Player:
        sprite.texture = EnemyBulletSprite
        trail.texture = EnemyBulletSprite
        set_collision_layer_bit(1, false)
        set_collision_layer_bit(5, true)
        set_collision_mask_bit(2, false)
        set_collision_mask_bit(4, false)
        set_collision_mask_bit(0, true)
        
    if bullet_automatic:
        _handle_automatic_mode()
    
    if bullet_type == BulletType.Laser:
        sprite.scale *= 3
        trail.scale *= 3
    
    elif bullet_type == BulletType.SlowFast:
        velocity /= Vector2(6, 6)
        slow_timer.start()
        yield(slow_timer, "timeout")
        _handle_automatic_mode()        
        velocity *= Vector2(1.5, 1.5)

func _process(delta):
    position += velocity * delta

################
# Public methods

func prepare(pos, speed, type, target, automatic):
    """
    Prepare bullet.
    
    :param pos:     Position
    :param speed:   Y Speed
    :param type:    Bullet type
    :param target:  Bullet target
    """
    position = pos
    bullet_target = target
    bullet_automatic = automatic
    base_speed = speed
    
    if target == BulletTarget.Player:
        velocity = Vector2(0, speed)
    else:
        velocity = Vector2(0, -speed)
        
    bullet_type = type

#################
# Private methods

func _handle_automatic_mode():
    if bullet_target == BulletTarget.Player:
        var players = get_tree().get_nodes_in_group("player")
        if len(players) > 0:
            _rotate_to_target(players[0])
    elif bullet_target == BulletTarget.Enemy:
        var enemies = get_tree().get_nodes_in_group("enemies")
        if len(enemies) > 0:
            _rotate_to_target(enemies[0])

func _rotate_to_target(target):
    var direction = (target.position - position).normalized()
    var angle = Vector2(0, 1).angle_to(direction)
    rotation = angle
    trail.process_material.angle = 360 - rad2deg(angle)
    velocity = direction * base_speed
    
#################
# Event callbacks

func _on_VisibilityNotifier2D_screen_exited():
    queue_free()

func _on_area_entered(area):
    if area.is_in_group("rocks") or area.is_in_group("enemies") or area.is_in_group("player"):
        var sparkles_position = position
        var sparkles = Sparkles.instance()
        sparkles.position = sparkles_position
        sparkles.z_index = 10
        
        # Add child
        get_parent().add_child(sparkles)
        
        area.hit()
        
    queue_free()