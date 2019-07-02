extends Control

# Game Screen

const PowerupType = preload("res://objects/powerups/Powerup.gd").PowerupType
const BossModel = preload("res://objects/enemies/BossEnemy.tscn")

onready var player = $Player
onready var bullets = $Bullets
onready var hud = $CanvasLayer/HUD
onready var animation_player = $AnimationPlayer
onready var wave_system = $WaveSystem
onready var alarm = $Alarm
onready var camera = $FXCamera

onready var rock_spawner = $Spawners/RockSpawner
onready var powerup_spawner = $Spawners/PowerupSpawner
onready var enemy_spawner = $Spawners/EnemySpawner
onready var life_spawner = $Spawners/LifePowerupSpawner

###################
# Lifecycle methods

func _ready():
    self.player.connect("fire", self, "_on_fire")
    self.player.connect("dead", self, "_on_Player_dead")
    self.player.connect("respawn", self, "_on_Player_respawn")

    self.rock_spawner.connect_target_scene(self, {
        "exploded": "_on_Rock_exploded"
    })
    self.powerup_spawner.connect_target_scene(self, {
        "powerup": "_on_Powerup_powerup"
    })
    self.life_spawner.connect_target_scene(self, {
        "powerup": "_on_Powerup_powerup"
    })
    self.enemy_spawner.connect_target_scene(self, {
        "exploded": "_on_Enemy_exploded",
        "fire": "_on_fire"
    })

    self.wave_system.connect("timeout", self, "_on_WaveSystem_timeout")
    self.life_spawner.disabled = true

    GameState.reset_game_state()
    GameState.update_hud(self.hud)

    self._load_next_wave()


func _notification(what):
    if (what == MainLoop.NOTIFICATION_WM_GO_BACK_REQUEST):
        GameState.load_screen(GameState.Screens.TITLE)

#################
# Private methods

func _load_next_wave():
    var wave_info = self.wave_system.load_next_wave()

    self.rock_spawner.set_frequency(wave_info["rocks_spawn_time"])
    self.enemy_spawner.set_frequency(wave_info["enemies_spawn_time"])
    self.powerup_spawner.set_frequency(wave_info["powerup_spawn_time"])

    self.rock_spawner.reset()
    self.enemy_spawner.reset()
    self.powerup_spawner.reset()

    self.hud.show_message(tr("WAVE") + " " +str(self.wave_system.current_wave))

func _load_boss():
    self.rock_spawner.disabled = true
    self.enemy_spawner.disabled = true

    var screen_size = Utils.get_game_size()

    self.animation_player.play("warning")
    self.hud.show_message(tr("WARNING"))

    self.alarm.play()
    yield(get_tree().create_timer(1), "timeout")
    self.alarm.play()
    yield(get_tree().create_timer(1), "timeout")

    var boss_inst = BossModel.instance()
    boss_inst.fire_time = 0.25 + 0.25 * self.wave_system.current_wave
    boss_inst.connect("exploded", self, "_on_Boss_exploded")
    boss_inst.connect("fire", self, "_on_fire")
    boss_inst.prepare(Vector2(screen_size.x / 2, -100), 100, 1.0)
    self.add_child(boss_inst)

#################
# Event callbacks

func _on_fire(bullet, pos, speed, type, target, automatic):
    var inst = bullet.instance()
    inst.prepare(pos, speed, type, target, automatic)
    self.bullets.add_child(inst)

func _on_Player_dead():
    camera.shake()

func _on_Player_respawn():
    var lives = GameState.lives
    if lives > 1:
        GameState.remove_life()
        GameState.update_hud(self.hud)
        lives = GameState.lives

        self.player.respawn()
        if lives == 1:
            self.hud.show_message(tr("LASTLIFE"))
    else:
        GameState.load_screen(GameState.Screens.GAMEOVER)

func _on_Rock_exploded():
    GameState.add_score(100)
    GameState.update_hud(self.hud)

func _on_Enemy_exploded():
    GameState.add_score(200)
    GameState.update_hud(self.hud)

func _on_Boss_exploded():
    GameState.add_score(2000 * self.wave_system.current_wave)
    GameState.update_hud(self.hud)
    camera.shake()

    var game_size = Utils.get_game_size()
    self.life_spawner.spawn_at_position(Vector2(game_size.x / 2, -50))

    yield(self.get_tree().create_timer(1), "timeout")
    self._load_next_wave()

func _on_Powerup_powerup(powerup_type):
    if powerup_type == PowerupType.Weapon:
        self.player.bullet_system.upgrade_weapon()
    elif powerup_type == PowerupType.Life:
        GameState.add_life()
        GameState.update_hud(self.hud)

func _on_WaveSystem_timeout():
    self._load_boss()
