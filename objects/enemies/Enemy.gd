extends Area2D

#######
# Enemy

# Sent when exploded
signal exploded
# Sent when enemy fires
signal fire(bullet, pos, speed, type, target, automatic)

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
onready var explosion_sound = $ExplosionSound

var hit_points = 0
var hit_count = 0
var exploded = false
var acc = 0
var screen_size = Vector2()
var firing = false

###################
# Lifecycle methods

func _ready():
    self.screen_size = Utils.get_game_size()
    
    self.fire_timer.wait_time = self.fire_time
    self.fire_timer.connect("timeout", self, "_on_FireTimer_timeout")
    
    self.bullet_system.connect("fire", self, "_on_BulletSystem_fire")
    
    self.trail.process_material.scale = self.scale.x

func _process(delta):
    self._move(delta)
        
################
# Public methods

func prepare(pos, speed, scl):
    """
    Prepare enemy.
    
    :param pos:     Position
    :param speed:   Y speed
    :param scl:     Scale
    """
    self.position = pos
    self.down_speed = speed
    self.scale = Vector2(scl, scl)
    
    # Calculate hit points based on scale factor
    self.hit_points = int(ceil(scl * BASE_HIT_POINTS))

func hit():
    """Hit enemy."""
    if not self.exploded:
        self.animation_player.play("tint")
        self.hit_count += 1
        if self.hit_count == self.hit_points:
            self.exploded = true
            self._explode()

#################
# Private methods

func _detect_player():
    var player = self.get_tree().get_nodes_in_group("player")
    if len(player) > 0:
        return player[0]
    else:
        return null
        
func _move(delta):
    self.acc += delta
    self.position += Vector2(self.move_speed, self.down_speed) * Vector2(delta, delta) * Vector2(cos(self.acc), 1)
    
    if self.firing:
        self.bullet_system.fire(self.muzzle.global_position)
    
    if self.position.y > self.screen_size.y:
        self.queue_free()
        
func _explode():
    # Stop fire
    self.firing = false
    self.fire_timer.stop()
    
    # Explode
    self.emit_signal("exploded")
    self.call_deferred("_disable_collisions")
    self.explosion_sound.play()
    self.animation_player.play("explode")
    yield(self.animation_player, "animation_finished")
    self.queue_free()
    
func _disable_collisions():
    self.collision_shape.disabled = true    

#################
# Event callbacks

func _on_FireTimer_timeout():
    self.firing = not self.firing
    
func _on_BulletSystem_fire(bullet, pos, speed, type, target, automatic):
    self.emit_signal("fire", bullet, pos, speed, type, target, automatic)