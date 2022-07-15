extends Control
class_name GameScreen

const BOSS_SCENE := preload("res://nodes/objects/BossEnemy.tscn")
const STATUS_TOAST_SCENE := preload("res://nodes/ui/StatusToast.tscn")

onready var _player: Player = $PlayerContainer/Player
onready var _enemy_container: Node2D = $EnemyContainer
onready var _hud: HUD = $CanvasLayer/HUD
onready var _animation_player: AnimationPlayer = $AnimationPlayer
onready var _wave_system: WaveSystem = $WaveSystem
onready var _alarm: AudioStreamPlayer = $Alarm
onready var _rock_spawner: Spawner = $Spawners/RockSpawner
onready var _powerup_spawner: Spawner = $Spawners/PowerupSpawner
onready var _enemy_spawner: Spawner = $Spawners/EnemySpawner
onready var _life_spawner: Spawner = $Spawners/LifePowerupSpawner
onready var _bomb_powerup_spawner: Spawner = $Spawners/BombPowerupSpawner
onready var _starfield: Starfield = $Starfield

func _ready() -> void:
    _player.connect("dead", self, "_on_player_dead")
    _player.connect("respawn", self, "_on_player_respawn")

    _rock_spawner.connect_target_scene(self, {
        "exploded": "_on_rock_exploded"
    })
    _powerup_spawner.connect_target_scene(self, {
        "powerup": "_on_powerup"
    })
    _life_spawner.connect_target_scene(self, {
        "powerup": "_on_powerup"
    })
    _enemy_spawner.connect_target_scene(self, {
        "exploded": "_on_enemy_exploded"
    })
    _bomb_powerup_spawner.connect_target_scene(self, {
        "powerup": "_on_powerup"
    })

    _enemy_spawner.connect("spawn", self, "_on_enemy_spawned")
    _wave_system.connect("timeout", self, "_on_wavesystem_timeout")
    _life_spawner.disabled = true

    GameState.reset_game_state()

    _load_next_wave()

func _notification(what: int) -> void:
    if what == NOTIFICATION_WM_GO_BACK_REQUEST:
        GameState.load_screen(GameState.Screens.TITLE)

func _process(_delta: float) -> void:
    if Input.is_action_just_pressed("ui_cancel"):
        GameState.load_screen(GameState.Screens.TITLE)

func _load_next_wave() -> void:
    var _wave_info := _wave_system.load_next_wave()
    _rock_spawner.set_frequency(_wave_info["rocks_spawn_time"])
    _enemy_spawner.set_frequency(_wave_info["enemies_spawn_time"])
    _powerup_spawner.set_frequency(_wave_info["powerup_spawn_time"])

    _rock_spawner.reset()
    _enemy_spawner.reset()
    _powerup_spawner.reset()
    _bomb_powerup_spawner.reset()

    _starfield.velocity = 100 * _wave_system.get_current_wave()
    _hud.show_message(tr("HUD_WAVE") + " " + str(_wave_system.get_current_wave()))

func _load_boss() -> void:
    _rock_spawner.disabled = true
    _enemy_spawner.disabled = true
    _starfield.enable_radial_accel = true

    var game_size := GameState.get_game_size()
    _animation_player.play("warning")
    _hud.show_message(tr("HUD_WARNING"))

    _alarm.play()
    yield(get_tree().create_timer(1.0), "timeout")
    _alarm.play()
    yield(get_tree().create_timer(1.0), "timeout")

    var boss_instance: BossEnemy = BOSS_SCENE.instance()
    boss_instance.connect("exploded", self, "_on_boss_exploded")
    boss_instance.prepare(Vector2(game_size.x / 2.0, -100.0), 100.0, 1.0)
    _enemy_container.add_child(boss_instance)

    boss_instance.set_hit_points_factor(1 + (_wave_system.get_current_wave() - 1) * 20.0)
    boss_instance.set_fire_time_factor(1 + (_wave_system.get_current_wave() - 1) * 0.25)

func _show_score_message(node: Node2D, score: int) -> void:
    var toast_instance: StatusToast = STATUS_TOAST_SCENE.instance()
    toast_instance.position = node.position + Vector2(0, 20.0)
    toast_instance.scale = node.scale * 1.5
    toast_instance.message_visible_time = 0.5
    toast_instance.message_offset = Vector2.ZERO
    add_child(toast_instance)

    var message := "+%d" % score if score > 0 else "-%d" % score
    var color := Color.green if score > 0 else Color.red
    toast_instance.show_message_with_color(message, color)
    yield(toast_instance, "message_shown")

    toast_instance.queue_free()

func _start_wave_transition() -> void:
    var game_size := GameState.get_game_size()
    _life_spawner.spawn_at_position(Vector2(game_size.x / 2.0, -50.0))
    yield(get_tree().create_timer(1.0), "timeout")

    _load_next_wave()

func _on_player_dead() -> void:
    FXCamera.shake()

func _on_player_respawn() -> void:
    var lives := GameState.get_lives()
    if lives > 0:
        GameState.remove_life()
        lives = GameState.get_lives()

        _player.respawn()
        if lives == 0:
            _hud.show_message(tr("HUD_LASTLIFE"))
    else:
        GameState.load_screen(GameState.Screens.GAMEOVER)


func _add_score(target: Node2D, amount: int) -> void:
    GameState.add_score(amount)
    _show_score_message(target, amount)

func _on_rock_exploded(node: Node2D) -> void:
    _add_score(node, 100)

func _on_enemy_exploded(node: Node2D) -> void:
    _add_score(node, 200)

func _on_enemy_spawned(node: Node) -> void:
    var enemy: Enemy = node
    enemy.set_hit_points_factor(1 + (_wave_system.get_current_wave() - 1) * 2)
    enemy.set_fire_time_factor(1 + (_wave_system.get_current_wave() - 1) * 0.25)

func _on_boss_exploded(node: Node2D) -> void:
    var score_to_add := 2000 * _wave_system.get_current_wave()
    _add_score(node, score_to_add)

    FXCamera.shake()

    _starfield.enable_radial_accel = false
    call_deferred("_start_wave_transition")

func _on_powerup(powerup: Powerup) -> void:
    match powerup.powerup_type:
        Powerup.PowerupType.WEAPON_UPGRADE:
            if _player.can_upgrade_weapon():
                _player.upgrade_weapon()
            else:
                _add_score(powerup, 200)
        Powerup.PowerupType.LIFE:
            GameState.add_life()
            _player.get_status_toast().show_message(tr("PLAYER_LIFE_MSG"))
        Powerup.PowerupType.BOMB:
            _player.get_bullet_system().enable_bomb()

func _on_wavesystem_timeout() -> void:
    _load_boss()
