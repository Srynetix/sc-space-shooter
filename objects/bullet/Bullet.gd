extends Area2D

########
# Bullet

const BulletType = preload("res://objects/player/BulletSystem.gd").BulletType

# Bullet target
enum Target {
    Enemy,
    Player
}

# Default target: Enemy
export (Target) var target = Target.Enemy

onready var visibility_notifier = $VisibilityNotifier2D
onready var slow_timer = $SlowTimer

var velocity = Vector2()
var bullet_type = null

###################
# Lifecycle methods

func _ready():
    connect("area_entered", self, "_on_area_entered")
    visibility_notifier.connect("screen_exited", self, "_on_VisibilityNotifier2D_screen_exited")
    
    if bullet_type == BulletType.SlowFast:
        var old_vel_y = velocity.y
        velocity.y = 100
        slow_timer.start()
        yield(slow_timer, "timeout")
        velocity.y = old_vel_y * 2
        
    elif bullet_type == BulletType.Automatic:
        var players = get_tree().get_nodes_in_group("player")
        if len(players) > 0:
            var player = players[0]
            var direction = (player.position - position).normalized()
            var angle = Vector2(0, 1).angle_to(direction)
            rotation = angle
            velocity = direction * 900

func _process(delta):
    position += velocity * delta

################
# Public methods

func prepare(pos, speed, type):
    """
    Prepare bullet.
    
    :param pos:     Position
    :param speed:   Y Speed
    :param type:    Bullet type
    """
    position = pos
    if target == Target.Player:
        velocity = Vector2(0, speed)
    else:
        velocity = Vector2(0, -speed)
        
    bullet_type = type
    
#################
# Event callbacks

func _on_VisibilityNotifier2D_screen_exited():
    queue_free()

func _on_area_entered(area):
    if area.is_in_group("rocks") or area.is_in_group("enemies") or area.is_in_group("player"):
        area.hit()
        
    queue_free()