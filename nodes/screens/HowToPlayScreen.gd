extends Control
class_name HowToPlayScreen

enum Step {
    START = 0,
    ROCKS_START,
    ROCKS_END,
    ENEMY_BEFORE,
    ENEMY_START,
    ENEMY_END,
    POWERUP,
    BOSS_BEFORE,
    BOSS_START,
    END
}

onready var _player: Player = $PlayerContainer/Player
onready var _status_toast: StatusToast = $StatusToast
onready var _timer: Timer = $Timer
onready var _rock_spawner: Spawner = $RockSpawner
onready var _enemy_spawner: Spawner = $EnemySpawner
onready var _powerup_spawner: Spawner = $PowerupSpawner
onready var _boss_spawner: Spawner = $BossSpawner
onready var _life_spawner: Spawner = $LifeSpawner
onready var _bomb_spawner: Spawner = $BombSpawner
onready var _skip_button: Button = $CanvasLayer/Margin/Button

var _initial_step := "_start_step_1"
var _current_step: int = Step.START
var _support_message_shown := false

func _ready() -> void:
    var game_size := GameState.get_game_size()
    _status_toast.set_text_size(30)
    _status_toast.set_message_visible_time(4.0)
    _status_toast.position = game_size / 2

    _player.connect("dead", self, "_on_player_dead")
    _player.connect("respawn", self, "_on_player_respawn")
    _timer.connect("timeout", self, "_on_timer_timeout")
    _powerup_spawner.connect_target_scene(self, {
        "powerup": "_on_powerup"
    })
    _bomb_spawner.connect_target_scene(self, {
        "powerup": "_on_powerup"
    })
    _life_spawner.connect_target_scene(self, {
        "powerup": "_on_powerup"
    })
    _boss_spawner.connect_target_scene(self, {
        "exploded": "_on_boss_exploded"
    })
    _boss_spawner.connect("spawn", self, "_on_boss_spawned")
    _skip_button.connect("pressed", self, "_on_skip_button")

    call_deferred(_initial_step)

func _start_step_1() -> void:
    _current_step = Step.START
    _status_toast.show_message(tr("TUTORIAL_STEP1_MSG1"))
    yield(_status_toast, "message_shown")

    _status_toast.show_message(tr("TUTORIAL_STEP1_MSG2"))
    yield(_status_toast, "message_shown")

    var msg := "TUTORIAL_STEP1_MSG3_%s" % ["MOBILE" if SxOS.is_mobile() else "PC"]
    _status_toast.show_message(tr(msg))
    yield(_status_toast, "message_shown")

    _status_toast.show_message(tr("TUTORIAL_STEP1_MSG4"))
    yield(_status_toast, "message_shown")

    _current_step = Step.ROCKS_START
    _support_message_shown = false

    _rock_spawner.reset_then_spawn()
    _timer.wait_time = 15
    _timer.start()

func _start_step_2() -> void:
    _current_step = Step.POWERUP

    _rock_spawner.disabled = true
    for item in _rock_spawner.get_elements():
        var rock: Rock = item
        rock.explode()

    _status_toast.show_priority_message(tr("TUTORIAL_STEP2_MSG1"))
    yield(_status_toast, "message_all_shown")

    _status_toast.show_message(tr("TUTORIAL_STEP2_MSG2"))
    yield(_status_toast, "message_shown")

    _current_step = Step.ENEMY_START
    _support_message_shown = false

    _enemy_spawner.reset_then_spawn()
    _timer.wait_time = 15
    _timer.start()

func _start_step_3() -> void:
    _current_step = Step.BOSS_BEFORE

    _enemy_spawner.disabled = true
    for item in _enemy_spawner.get_elements():
        if item is Enemy:
            var enemy: Enemy = item
            enemy.explode()

    _status_toast.show_priority_message(tr("TUTORIAL_STEP3_MSG1"))
    yield(_status_toast, "message_all_shown")

    _status_toast.show_message(tr("TUTORIAL_STEP3_MSG2"))
    yield(_status_toast, "message_shown")

    _powerup_spawner.spawn_centered()
    _status_toast.show_message(tr("TUTORIAL_STEP3_MSG3"))
    yield(_status_toast, "message_shown")
    yield(get_tree().create_timer(2.0), "timeout")

    _status_toast.show_message(tr("TUTORIAL_STEP3_MSG4"))
    yield(_status_toast, "message_shown")

    _status_toast.show_message(tr("TUTORIAL_STEP3_MSG5"))
    _powerup_spawner.spawn_centered()
    yield(get_tree().create_timer(0.25), "timeout")
    _powerup_spawner.spawn_centered()
    yield(get_tree().create_timer(0.25), "timeout")
    _powerup_spawner.spawn_centered()
    yield(get_tree().create_timer(2.0), "timeout")

    _status_toast.show_message(tr("TUTORIAL_STEP3_MSG6"))
    yield(_status_toast, "message_all_shown")
    _status_toast.show_message(tr("TUTORIAL_STEP3_MSG7"))
    yield(_status_toast, "message_shown")
    _status_toast.show_message(tr("TUTORIAL_STEP3_MSG8"))
    yield(_status_toast, "message_shown")

    _player.get_bullet_system().lock_weapon()
    _bomb_spawner.spawn_centered()
    _status_toast.show_message(tr("TUTORIAL_STEP3_MSG9"))
    yield(_status_toast, "message_shown")
    yield(get_tree().create_timer(2.0), "timeout")

    var msg := "TUTORIAL_STEP3_MSG10_%s" % ["MOBILE" if SxOS.is_mobile() else "PC"]
    _status_toast.show_message(tr(msg))
    yield(_status_toast, "message_shown")
    yield(get_tree().create_timer(3.0), "timeout")

    _life_spawner.spawn_centered()
    _status_toast.show_message(tr("TUTORIAL_STEP3_MSG11"))
    yield(_status_toast, "message_shown")
    _status_toast.show_message(tr("TUTORIAL_STEP3_MSG12"))
    yield(_status_toast, "message_shown")

    _current_step = Step.BOSS_BEFORE
    _status_toast.show_message(tr("TUTORIAL_STEP3_MSG13"))
    yield(_status_toast, "message_shown")

    _current_step = Step.BOSS_START
    _support_message_shown = false
    _boss_spawner.spawn_centered()

func _start_step_4() -> void:
    _current_step = Step.END
    _status_toast.show_priority_message(tr("TUTORIAL_STEP4_MSG1"))
    yield(_status_toast, "message_all_shown")
    _status_toast.show_message(tr("TUTORIAL_STEP4_MSG2"))
    yield(_status_toast, "message_shown")
    _start_game()

func _start_game() -> void:
    GameState.set_tutorial_shown()
    GameState.load_screen(GameState.Screens.GAME)

func _on_timer_timeout() -> void:
    if _current_step == Step.ROCKS_START:
        _current_step = Step.ROCKS_END
        call_deferred("_start_step_2")
    elif _current_step == Step.ENEMY_START:
        _current_step = Step.ENEMY_END
        call_deferred("_start_step_3")

func _on_player_dead() -> void:
    if _current_step == Step.ROCKS_START:
        if !_support_message_shown:
            _status_toast.show_priority_message(tr("TUTORIAL_SUPPORT_MSG1"))
            _support_message_shown = true
    elif _current_step == Step.ENEMY_START:
        if !_support_message_shown:
            _status_toast.show_priority_message(tr("TUTORIAL_SUPPORT_MSG2"))
            _support_message_shown = true
    elif _current_step == Step.BOSS_START:
        if !_support_message_shown:
            _status_toast.show_priority_message(tr("TUTORIAL_SUPPORT_MSG3"))
            _support_message_shown = true

func _on_player_respawn() -> void:
    _player.respawn()

func _on_powerup(powerup: Powerup) -> void:
    if powerup.powerup_type == Powerup.PowerupType.WEAPON_UPGRADE:
        _player.upgrade_weapon()
    elif powerup.powerup_type == Powerup.PowerupType.LIFE:
        _player.get_status_toast().show_message(tr("PLAYER_LIFE_MSG"))
    elif powerup.powerup_type == Powerup.PowerupType.BOMB:
        _player.get_bullet_system().enable_bomb()

func _on_boss_spawned(boss: Enemy) -> void:
    boss.set_hit_points_factor(10)

func _on_boss_exploded(_node: Node2D) -> void:
    call_deferred("_start_step_4")

func _on_skip_button() -> void:
    _skip_button.mouse_filter = MOUSE_FILTER_IGNORE
    _start_game()
