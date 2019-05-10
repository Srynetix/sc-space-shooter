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
    self.trail.process_material.scale = self.scale.x

    self._cache_limits()

func _process(delta):
    self.position += self.velocity * delta
    self.rotation += delta / 2
    self.trail.process_material.angle = 360 - rad2deg(self.rotation)

    self._handle_position_wrap()

################
# Public methods

func prepare(pos, speed, scl):
    """
    Prepare rock attributes.

    :param pos:     Position
    :param speed:   Y Speed
    :param scl:     Scale
    """
    self.position = pos
    self.scale = Vector2(scl, scl)
    self.velocity = Vector2(rand_range(-X_VELOCITY_SPEED, X_VELOCITY_SPEED), speed)

    # Calculate hit points based on scale factor
    self.hit_points = int(ceil(scl * BASE_HIT_POINTS))

func hit():
    """Hit rock."""
    if not self.exploded:
        self.animation_player.play("tint")
        self.hit_count += 1
        if self.hit_count == self.hit_points:
            self.exploded = true
            self._explode()

func explode():
    """Explode rock."""
    self.exploded = true
    self._explode()

#################
# Private methods

func _cache_limits():
    var sprite_size = self.sprite.texture.get_size() * self.scale
    self._x_screen_limits = [-sprite_size.x / 2, self.game_size.x + sprite_size.x / 2]

func _handle_position_wrap():
    if self.position.x > self._x_screen_limits[1]:
        self.position.x = self._x_screen_limits[0]
    elif self.position.x < self._x_screen_limits[0]:
        self.position.x = self._x_screen_limits[1]

func _disable_shape():
    self.collision_shape.disabled = true

func _explode():
    self.emit_signal("exploded")
    self.call_deferred("_disable_shape")
    self.explosion_sound.play()
    self.animation_player.play("explode")
    yield(self.animation_player, "animation_finished")
    self.queue_free()
