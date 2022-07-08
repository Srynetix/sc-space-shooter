extends Node2D

onready var _player_bullets: BulletSystem = $PlayerBullets
onready var _enemy_bullets: BulletSystem = $EnemyBullets
onready var _laser_bullets: BulletSystem = $LaserBullets

func _ready() -> void:
    var game_size := GameState.get_game_size()
    _player_bullets.position = Vector2(game_size.x / 4.0, game_size.y / 2)
    _enemy_bullets.position = Vector2(game_size.x / 2, game_size.y / 2)
    _laser_bullets.position = Vector2(game_size.x / 2 + game_size.x / 4, game_size.y / 2)

    while true:
        _player_bullets.fire(_player_bullets.position)
        _enemy_bullets.fire(_enemy_bullets.position)
        _laser_bullets.fire(_laser_bullets.position)
        yield(get_tree().create_timer(0.25), "timeout")
