extends Area2D

#######
# Enemy

# Sent when exploded
signal exploded
# Sent when enemy fires
signal fire(bullet, pos, speed, type)

# Base hit points
const BASE_HIT_POINTS = 5

# Move speed
export (float) var move_speed = 150.0
# Y speed
export (float) var down_speed = 50.0
# Fire time
export (float) var fire_time = 1

onready var animation_player = $AnimationPlayer
onready var collision_shape = $CollisionShape2D
onready var muzzle = $Position2D
onready var fire_timer = $FireTimer
onready var bullet_system = $BulletSystem
onready var trail = $Trail

var hit_points = 0
var hit_count = 0
var exploded = false
var acc = 0
var screen_size = Vector2()
var firing = false

###################
# Lifecycle methods

func _ready():
    screen_size = get_viewport().size
    
    fire_timer.wait_time = fire_time
    fire_timer.connect("timeout", self, "_on_FireTimer_timeout")
    
    bullet_system.connect("fire", self, "_on_BulletSystem_fire")
    
    trail.process_material.scale = scale.x  

func _process(delta):
    _move(delta)
        
################
# Public methods

func prepare(pos, speed, scl):
    """
    Prepare enemy.
    
    :param pos:     Position
    :param speed:   Y speed
    :param scl:     Scale
    """
    position = pos
    down_speed = speed
    scale = Vector2(scl, scl)
    
    # Calculate hit points based on scale factor
    hit_points = int(ceil(scl * BASE_HIT_POINTS))

func hit():
    """Hit enemy."""
    if not exploded:
        animation_player.play("tint")
        hit_count += 1
        if hit_count == hit_points:
            exploded = true
            _explode()

#################
# Private methods

func _detect_player():
    var player = get_tree().get_nodes_in_group("player")
    if len(player) > 0:
        return player[0]
    else:
        return null
        
func _move(delta):
    acc += delta
    position += Vector2(move_speed, down_speed) * Vector2(delta, delta) * Vector2(cos(acc), 1)
    
    if firing:
        bullet_system.fire(muzzle.global_position)
    
    if position.y > screen_size.y:
        queue_free()
        
func _explode():
    # Stop fire
    firing = false
    fire_timer.stop()
    
    # Explode
    emit_signal("exploded")
    call_deferred("_disable_collisions")
    animation_player.play("explode")
    yield(animation_player, "animation_finished")
    queue_free()
    
func _disable_collisions():
    collision_shape.disabled = true    

#################
# Event callbacks

func _on_FireTimer_timeout():
    firing = not firing
    
func _on_BulletSystem_fire(bullet, pos, speed, type):
    emit_signal("fire", bullet, pos, speed, type)