extends Area2D

########
# Bullet

const BulletType = preload("res://objects/bullet/BulletType.gd").BulletType
const BulletTarget = preload("res://objects/bullet/BulletTarget.gd").BulletTarget
const Sparkles = preload("res://objects/fx/Sparkles.tscn")

const EnemyBulletSprite = preload("res://assets/textures/laserRed06.png")

# Default target: Enemy
export (BulletTarget) var bullet_target = BulletTarget.Enemy
export (BulletType) var bullet_type = BulletType.Simple
export (bool) var bullet_automatic = false

onready var visibility_notifier = $VisibilityNotifier2D
onready var slow_timer = $SlowTimer
onready var trail = $Trail
onready var sprite = $Sprite

var velocity = Vector2()
var base_speed = 0

###################
# Lifecycle methods

func _ready():
    self.connect("area_entered", self, "_on_area_entered")
    self.visibility_notifier.connect("screen_exited", self, "_on_VisibilityNotifier2D_screen_exited")

    # Prepare for player target
    if self.bullet_target == BulletTarget.Player:
        self.sprite.texture = EnemyBulletSprite
        self.trail.texture = EnemyBulletSprite
        self.set_collision_layer_bit(1, false)
        self.set_collision_layer_bit(5, true)
        self.set_collision_mask_bit(2, false)
        self.set_collision_mask_bit(4, false)
        self.set_collision_mask_bit(0, true)

    if self.bullet_automatic:
        self._handle_automatic_mode()

    if self.bullet_type == BulletType.Laser:
        self.sprite.scale *= 3
        self.trail.scale *= 3

    elif self.bullet_type == BulletType.SlowFast:
        self.velocity /= Vector2(6, 6)
        self.slow_timer.start()
        yield(self.slow_timer, "timeout")
        self._handle_automatic_mode()
        self.velocity *= Vector2(1.5, 1.5)

func _process(delta):
    self.position += self.velocity * delta

################
# Public methods

func prepare(pos, speed, type, target, automatic):
    """
    Prepare bullet.

    :param pos:     Position
    :param speed:   Y Speed
    :param type:    Bullet type
    :param target:  Bullet target
    """
    self.position = pos
    self.bullet_target = target
    self.bullet_automatic = automatic
    self.base_speed = speed

    if target == BulletTarget.Player:
        self.velocity = Vector2(0, speed)
    else:
        self.velocity = Vector2(0, -speed)

    self.bullet_type = type

#################
# Private methods

func _handle_automatic_mode():
    if self.bullet_target == BulletTarget.Player:
        var players = self.get_tree().get_nodes_in_group("player")
        if len(players) > 0:
            self._rotate_to_target(players[0])
    elif self.bullet_target == BulletTarget.Enemy:
        var enemies = self.get_tree().get_nodes_in_group("enemies")
        if len(enemies) > 0:
            self._rotate_to_target(enemies[0])

func _rotate_to_target(target):
    var direction = (target.position - self.position).normalized()
    var angle = Vector2(0, 1).angle_to(direction)
    self.rotation = angle
    self.trail.process_material.angle = 360 - rad2deg(angle)
    self.velocity = direction * self.base_speed

#################
# Event callbacks

func _on_VisibilityNotifier2D_screen_exited():
    self.queue_free()

func _on_area_entered(area):
    if area.is_in_group("rocks") or area.is_in_group("enemies") or area.is_in_group("player"):
        var sparkles_position = self.position
        var sparkles = Sparkles.instance()
        sparkles.position = sparkles_position
        sparkles.z_index = 10

        # Add child
        self.get_parent().add_child(sparkles)

        area.hit()

    self.queue_free()
