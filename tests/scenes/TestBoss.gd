extends Node2D

onready var _player: Player = $Player
onready var _spawner: Spawner = $BombSpawner

const BOSS_SCENE := preload("res://nodes/objects/BossEnemy.tscn")

func _ready() -> void:
    _player.connect("respawn", self, "_on_respawn")
    _spawner.connect_target_scene(self, {
        "powerup": "_on_powerup"
    })

    _spawn_boss()

func _on_respawn() -> void:
    _player.respawn()

func _on_boss_exploded(_instance: BossEnemy) -> void:
    call_deferred("_spawn_boss")

func _on_powerup(powerup: Powerup) -> void:
    if powerup.powerup_type == Powerup.PowerupType.BOMB:
        _player.get_bullet_system().enable_bomb()

func _spawn_boss() -> void:
    var game_size := GameState.get_game_size()
    var boss_instance: BossEnemy = BOSS_SCENE.instance()
    boss_instance.connect("exploded", self, "_on_boss_exploded")
    boss_instance.prepare(Vector2(game_size.x / 2, -100), 100, 1.0)
    add_child(boss_instance)
