extends "res://objects/enemies/Enemy.gd"

############
# Boss Enemy

# Base hit points
const BOSS_BASE_HIT_POINTS = 50

onready var weapon_swap = $WeaponSwap

var velocity = Vector2()

###################
# Lifecycle methods

func _ready():
    self.weapon_swap.connect("timeout", self, "_on_WeaponSwap_timeout")
    self.weapon_swap.start()

################
# Public methods

func prepare(pos, speed, scl):
    """
    Prepare boss enemy.

    :param pos:     Position
    :param speed:   Y Speed
    :param scl:     Scale
    """
    self.position = pos
    self.down_speed = speed
    self.move_speed *= 1.5
    self.scale = Vector2(scl * 2, scl * 2)
    self.velocity = Vector2(self.move_speed, self.down_speed)

    # Calculate hit points based on scale factor
    self.hit_points = int(ceil(scl * BOSS_BASE_HIT_POINTS))

#################
# Private methods

func _move(delta):
    self.acc += delta
    self.position += self.velocity * Vector2(delta, delta) * Vector2(cos(self.acc), 1)

    self.velocity.y *= 0.992
    if self.velocity.y <= 0.01:
        self.velocity.y = 0

    if self.firing:
        self.bullet_system.fire(self.muzzle.global_position)

    if self.position.y > self.screen_size.y:
        self.queue_free()

#################
# Event callbacks

func _on_WeaponSwap_timeout():
    self.bullet_system.switch_random_type()
