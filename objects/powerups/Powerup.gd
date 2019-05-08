extends Area2D

#########
# Powerup

# Sent on activation
signal powerup

# Powerup type
const PowerupType = preload("res://objects/powerups/PowerupType.gd").PowerupType

export (PowerupType) var powerup_type = PowerupType.Weapon

onready var visibility_notifier = $VisibilityNotifier2D
onready var animation_player = $AnimationPlayer
onready var collision_shape = $CollisionShape2D

var velocity = Vector2()

###################
# Lifecycle methods

func _ready():
    self.visibility_notifier.connect("screen_exited", self, "_on_VisibilityNotifier2D_screen_exited")
    self.connect("area_entered", self, "_on_area_entered")
    
func _process(delta):
    self.position += self.velocity * delta
    
################
# Public methods

func prepare(pos, speed, scl):
    """
    Prepare powerup.
    
    :param pos:     Position
    :param speed:   Y Speed
    :param scl:     Scale
    """
    self.position = pos
    self.scale = Vector2(scl, scl)
    self.velocity = Vector2(0, speed)
    
#################
# Event callbacks

func _on_area_entered(area):
    if area.is_in_group("player"):
        self.collision_shape.set_deferred("disabled", true)
        self.emit_signal("powerup", self.powerup_type)
        self.animation_player.play("fade")
        yield(self.animation_player, "animation_finished")
        self.queue_free()
        
func _on_VisibilityNotifier2D_screen_exited():
    self.queue_free()