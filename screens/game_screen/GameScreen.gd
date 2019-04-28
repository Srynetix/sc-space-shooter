extends Control

# Game Screen

const PowerupType = preload("res://objects/powerups/Powerup.gd").PowerupType
const Waves = [
    {
        "duration": 30,
        "rocks_spawn_time": 3,
        "enemies_spawn_time": 8,
        "powerup_spawn_time": 10,
    },
    {
        "duration": 30,
        "rocks_spawn_time": 0.5,
        "enemies_spawn_time": 8,
        "powerup_spawn_time": 10,        
    }
]

const BossModel = preload("res://objects/enemies/BossEnemy.tscn")

onready var starfield = $Starfield
onready var player = $Player
onready var bullets = $Bullets
onready var hud = $CanvasLayer/HUD
onready var wave_timer = $WaveTimer
onready var animation_player = $AnimationPlayer

onready var rock_spawner = $Spawners/RockSpawner
onready var powerup_spawner = $Spawners/PowerupSpawner
onready var enemy_spawner = $Spawners/EnemySpawner

var starfield_velocity = 500
var current_wave = 0

###################
# Lifecycle methods

func _ready():
    randomize()

    starfield.update_velocity(starfield_velocity)
    player.connect("fire", self, "_on_Player_fire")
    player.connect("dead", self, "_on_Player_dead")
    wave_timer.connect("timeout", self, "_on_WaveTimer_timeout")
    
    rock_spawner.connect("exploded", self, "_on_Rock_exploded")
    powerup_spawner.connect("powerup", self, "_on_Powerup_powerup")
    enemy_spawner.connect("exploded", self, "_on_Enemy_exploded")
    enemy_spawner.connect("fire", self, "_on_Enemy_fire")
    
    GameState.update_hud(hud)
    
    _load_next_wave()
    
#################
# Private methods

func _load_next_wave():
    current_wave += 1
    var wave_info = _get_current_wave_info()
     
    wave_timer.wait_time = wave_info["duration"]
    rock_spawner.set_frequency(wave_info["rocks_spawn_time"])
    enemy_spawner.set_frequency(wave_info["enemies_spawn_time"])
    powerup_spawner.set_frequency(wave_info["powerup_spawn_time"])
    wave_timer.start()
    
    rock_spawner.reset()
    enemy_spawner.reset()
    
    hud.show_message("Wave {w}".format({"w": current_wave}))
    
func _load_boss():
    rock_spawner.disabled = true
    enemy_spawner.disabled = true
    
    var screen_size = get_viewport().size
    
    animation_player.play("warning")
    hud.show_message("WARNING !")
    
    yield(get_tree().create_timer(2), "timeout")
    
    var boss_inst = BossModel.instance()
    boss_inst.connect("exploded", self, "_on_Boss_exploded")
    boss_inst.connect("fire", self, "_on_Enemy_fire")
    boss_inst.prepare(Vector2(screen_size.x / 2, -100), 100, 1.0)
    
    add_child(boss_inst)

func _get_current_wave_info():
    if len(Waves) < current_wave:
        return Waves[len(Waves) - 1]
    return Waves[current_wave - 1]
    
#################
# Event callbacks

func _on_Player_fire(bullet, pos, speed, type):
    var inst = bullet.instance()
    inst.prepare(pos, speed, type)
    
    bullets.add_child(inst)
    
func _on_Enemy_fire(bullet, pos, speed, type):
    var inst = bullet.instance()
    inst.prepare(pos, speed, type)
    
    bullets.add_child(inst)

func _on_Player_dead():
    var lives = GameState.lives
    if lives > 0:
        GameState.remove_life()
        GameState.update_hud(hud)
        lives = GameState.lives
        
        player.respawn()
        if lives == 1:
            hud.show_message("LAST LIFE !")
    else:
        GameState.load_screen(GameState.Screens.GAMEOVER)
    
func _on_Rock_exploded():
    GameState.add_score(100)
    GameState.update_hud(hud)
    
func _on_Enemy_exploded():
    GameState.add_score(200)
    GameState.update_hud(hud)
    
func _on_Boss_exploded():
    GameState.add_score(2000)
    GameState.update_hud(hud)
    
    yield(get_tree().create_timer(1), "timeout")
    _load_next_wave()
   
func _on_Powerup_powerup(powerup_type):
    if powerup_type == PowerupType.Weapon:
        player.bullet_system.upgrade_weapon()
        
func _on_WaveTimer_timeout():
    _load_boss()