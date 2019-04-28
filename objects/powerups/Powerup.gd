extends Area2D

#########
# Powerup

# Sent on activation
signal powerup

# Powerup types
enum PowerupType {
    Weapon
}

export (PowerupType) var powerup_type = PowerupType.Weapon

onready var visibility_notifier = $VisibilityNotifier2D
onready var animation_player = $AnimationPlayer
onready var collision_shape = $CollisionShape2D

var velocity = Vector2()

###################
# Lifecycle methods

func _ready():
    visibility_notifier.connect("screen_exited", self, "_on_VisibilityNotifier2D_screen_exited")
    connect("area_entered", self, "_on_area_entered")
    
func _process(delta):
    position += velocity * delta
    
################
# Public methods

func prepare(pos, speed, scl):
    """
    Prepare powerup.
    
    :param pos:     Position
    :param speed:   Y Speed
    :param scl:     Scale
    """
    position = pos
    scale = Vector2(scl, scl)
    velocity = Vector2(0, speed)
    
#################
# Event callbacks

func _on_area_entered(area):
    if area.is_in_group("player"):
        collision_shape.set_deferred("disabled", true)
        emit_signal("powerup", powerup_type)
        animation_player.play("fade")
        yield(animation_player, "animation_finished")
        queue_free()
        
func _on_VisibilityNotifier2D_screen_exited():
    queue_free()