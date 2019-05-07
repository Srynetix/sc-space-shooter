extends Area2D

#######
# Rock

# Sent when exploded
signal exploded

# Hit points before explosion
const BASE_HIT_POINTS = 3
const X_VELOCITY_SPEED = 100

onready var trail = $Trail
onready var animation_player = $AnimationPlayer
onready var collision_shape = $CollisionShape2D
onready var sprite = $Sprite 
onready var explosion_sound = $ExplosionSound
onready var game_size = Utils.get_game_size()

var velocity = Vector2()
var hit_points = 0
var hit_count = 0
var exploded = false

var _x_screen_limits = null 

###################
# Lifecycle methods

func _ready():
    # Trail
    trail.process_material.scale = scale.x
    
    _cache_limits()

func _process(delta):
    position += velocity * delta
    rotation += delta / 2
    trail.process_material.angle = 360 - rad2deg(rotation)
    
    _handle_position_wrap()

################
# Public methods

func prepare(pos, speed, scl):
    """
    Prepare rock attributes.
    
    :param pos:     Position
    :param speed:   Y Speed
    :param scl:     Scale
    """
    position = pos
    scale = Vector2(scl, scl)
    velocity = Vector2(rand_range(-X_VELOCITY_SPEED, X_VELOCITY_SPEED), speed)
    
    # Calculate hit points based on scale factor
    hit_points = int(ceil(scl * BASE_HIT_POINTS))
    
func hit():
    """Hit rock."""
    if not exploded:
        animation_player.play("tint")
        hit_count += 1
        if hit_count == hit_points:
            exploded = true
            _explode()
            
func explode():
    """Explode rock."""
    exploded = true
    _explode()

#################
# Private methods

func _cache_limits():
    var sprite_size = sprite.texture.get_size() * scale
    _x_screen_limits = [-sprite_size.x / 2, game_size.x + sprite_size.x / 2]

func _handle_position_wrap():    
    if position.x > _x_screen_limits[1]:
        position.x = _x_screen_limits[0]
    elif position.x < _x_screen_limits[0]:
        position.x = _x_screen_limits[1]

func _disable_shape():
    collision_shape.disabled = true

func _explode():
    emit_signal("exploded")
    call_deferred("_disable_shape")
    explosion_sound.play()
    animation_player.play("explode")
    yield(animation_player, "animation_finished")
    queue_free()