extends Area2D

#########
# Player

# Sent when player fires
signal fire(bullet, pos, speed, type, target, automatic)
# Sent when dead
signal dead

# Player state
enum State {
    IDLE,
    SPAWNING,
    DEAD
}

const TOUCH_OFFSET = Vector2(0, -100)

export (Vector2) var move_speed = Vector2(500, 500)
export (float) var damping = 0.9
export (float) var spawning_time = 3

onready var spawning_timer = $Timers/SpawningTimer
onready var muzzle = $Position2D
onready var animation_player = $AnimationPlayer
onready var collision_shape = $CollisionShape2D
onready var bullet_system = $BulletSystem
onready var status_label = $Label

var initial_position = Vector2()
var velocity = Vector2()
var state = State.IDLE

var is_touching = false
var last_touch_position = Vector2()

###################
# Lifecycle methods

func _ready():
    self.connect("area_entered", self, "_on_area_entered")

    self.spawning_timer.wait_time = self.spawning_time
    self.spawning_timer.connect("timeout", self, "_on_SpawningTimer_timeout")
    self.bullet_system.connect("fire", self, "_on_BulletSystem_fire")
    self.status_label.text = ""

    self.initial_position = self.position

func _process(delta):
    if self.state == State.DEAD:
        return

    var movement = self._handle_movement()
    if movement == Vector2():
        # Damp velocity if idle
        self.velocity *= self.damping
    else:
        # Update velocity
        self.velocity = movement * self.move_speed

    if self.is_touching:
        self.velocity = Vector2()
        self.position = self.last_touch_position + TOUCH_OFFSET

    # Handle fire
    self._handle_fire()

    # Update position
    self.position += self.velocity * delta
    self._clamp_position()

func _input(event):
    if event is InputEventScreenTouch:
        self.last_touch_position = event.position
        self.is_touching = event.pressed

        # Screen released
        if not event.pressed:
            self.velocity = Vector2()

    elif event is InputEventScreenDrag:
        self.last_touch_position = event.position

################
# Public methods

func respawn():
    """Respawn player."""
    self._set_state(State.SPAWNING)
    self.velocity = Vector2()
    self.position = self.initial_position
    self.bullet_system.reset_weapons()

func hit():
    """Hit player."""
    self._set_state(State.DEAD)

#################
# Private methods

func _clamp_position():
    var game_size = self.get_viewport().size

    self.position.x = clamp(self.position.x, 0, game_size.x)
    self.position.y = clamp(self.position.y, 0, game_size.y)

func _set_state(new_state):
    if self.state != new_state:
        self.state = new_state

        match new_state:
            State.DEAD:
                self.collision_shape.set_deferred("disabled", true)
                self.animation_player.play("explode")
                self.emit_signal("dead")
                yield(self.animation_player, "animation_finished")
            State.IDLE:
                self.collision_shape.set_deferred("disabled", false)
                self.animation_player.play("idle")
            State.SPAWNING:
                self.spawning_timer.start()
                self.animation_player.play("spawning")

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
    if Input.is_action_pressed("player_shoot") || self.is_touching:
        self.bullet_system.fire(self.muzzle.global_position)

#################
# Event callbacks

func _on_SpawningTimer_timeout():
    self._set_state(State.IDLE)

func _on_area_entered(area):
    if area.is_in_group("rocks"):
        area.explode()
        self._set_state(State.DEAD)
    elif area.is_in_group("enemies"):
        self._set_state(State.DEAD)

func _on_BulletSystem_fire(bullet, pos, speed, type, target, automatic):
    self.emit_signal("fire", bullet, pos, speed, type, target, automatic)
