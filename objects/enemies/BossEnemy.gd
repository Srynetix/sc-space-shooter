extends "res://objects/enemies/Enemy.gd"

############
# Boss Enemy

# Base hit points
const BOSS_BASE_HIT_POINTS = 50

var velocity = Vector2()

################
# Public methods

func prepare(pos, speed, scl):
    """
    Prepare boss enemy.
    
    :param pos:     Position
    :param speed:   Y Speed
    :param scl:     Scale
    """
    position = pos
    down_speed = speed
    move_speed *= 1.5
    scale = Vector2(scl * 2, scl * 2)
    velocity = Vector2(move_speed, down_speed)
    
    # Calculate hit points based on scale factor
    hit_points = int(ceil(scl * BOSS_BASE_HIT_POINTS))
    
#################
# Private methods

func _move(delta):
    acc += delta
    position += velocity * Vector2(delta, delta) * Vector2(cos(acc), 1)
    
    velocity.y *= 0.992
    if velocity.y <= 0.01:
        velocity.y = 0
 
    if firing:
        bullet_system.fire(muzzle.global_position)
    
    if position.y > screen_size.y:
        queue_free()