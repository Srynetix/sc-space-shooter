extends Area2D

#########
# Player

# Sent when player fires
signal fire(bullet, pos, speed, type)
# Sent when dead
signal dead

# Player state
enum State {
    IDLE,
    SPAWNING,
    DEAD
}

export (Vector2) var move_speed = Vector2(500, 500)
export (float) var damping = 0.9
export (float) var spawning_time = 3

onready var spawning_timer = $Timers/SpawningTimer
onready var muzzle = $Position2D
onready var animation_player = $AnimationPlayer
onready var collision_shape = $CollisionShape2D
onready var bullet_system = $BulletSystem

var initial_position = Vector2()
var velocity = Vector2()
var state = State.IDLE

###################
# Lifecycle methods

func _ready():
    connect("area_entered", self, "_on_area_entered")
    
    spawning_timer.wait_time = spawning_time
    spawning_timer.connect("timeout", self, "_on_SpawningTimer_timeout")
    bullet_system.connect("fire", self, "_on_BulletSystem_fire")
    
    initial_position = position

func _process(delta):
    if state == State.DEAD:
        return

    var movement = _handle_movement()
    _handle_fire()
    
    if movement == Vector2():
        # Damp velocity if idle
        velocity *= damping
    else:
        # Update velocity
        velocity = movement * move_speed
    
    # Update position
    position += velocity * delta
    _clamp_position()
    
################
# Public methods

func respawn():
    """Respawn player."""
    _set_state(State.SPAWNING)
    velocity = Vector2()
    position = initial_position    
    bullet_system.reset_weapons()
    
func hit():
    """Hit player."""
    _set_state(State.DEAD)

#################
# Private methods

func _clamp_position():
    var vp_width = get_viewport().size.x
    var vp_height = get_viewport().size.y
    
    position.x = clamp(position.x, 0, vp_width)
    position.y = clamp(position.y, 0, vp_height)

func _set_state(new_state):    
    if state != new_state:
        state = new_state
    
        match new_state:
            State.DEAD:
                collision_shape.set_deferred("disabled", true)
                animation_player.play("explode")
                yield(animation_player, "animation_finished")
                emit_signal("dead")
            State.IDLE:
                collision_shape.set_deferred("disabled", false)
                animation_player.play("idle")
            State.SPAWNING:
                spawning_timer.start()
                animation_player.play("spawning")                

func _handle_movement():
    var movement = Vector2()
    if Input.is_action_pressed("player_left"):
        movement.x -= 1
    if Input.is_action_pressed("player_right"):
        movement.x += 1
    if Input.is_action_pressed("player_up"):
        movement.y -= 1
    if Input.is_action_pressed("player_down"):
        movement.y += 1
        
    return movement

func _handle_fire():
    if Input.is_action_pressed("player_shoot"):
        bullet_system.fire(muzzle.global_position)
        
#################
# Event callbacks
    
func _on_SpawningTimer_timeout():
    _set_state(State.IDLE)
    
func _on_area_entered(area):
    if area.is_in_group("rocks"):
        area.explode()
        _set_state(State.DEAD)        
    elif area.is_in_group("enemies"):
        _set_state(State.DEAD)
    
func _on_BulletSystem_fire(bullet, pos, speed, type):
    emit_signal("fire", bullet, pos, speed, type)