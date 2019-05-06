extends Control

# Game Screen

const PowerupType = preload("res://objects/powerups/Powerup.gd").PowerupType
const BossModel = preload("res://objects/enemies/BossEnemy.tscn")

onready var player = $Player
onready var bullets = $Bullets
onready var hud = $CanvasLayer/HUD
onready var animation_player = $AnimationPlayer
onready var wave_system = $WaveSystem

onready var rock_spawner = $Spawners/RockSpawner
onready var powerup_spawner = $Spawners/PowerupSpawner
onready var enemy_spawner = $Spawners/EnemySpawner
onready var life_spawner = $Spawners/LifePowerupSpawner

###################
# Lifecycle methods

func _ready():
    player.connect("fire", self, "_on_Player_fire")
    player.connect("dead", self, "_on_Player_dead")
    
    rock_spawner.connect("exploded", self, "_on_Rock_exploded")
    powerup_spawner.connect("powerup", self, "_on_Powerup_powerup")
    life_spawner.connect("powerup", self, "_on_Powerup_powerup")
    enemy_spawner.connect("exploded", self, "_on_Enemy_exploded")
    enemy_spawner.connect("fire", self, "_on_Enemy_fire")
    wave_system.connect("timeout", self, "_on_WaveSystem_timeout")
    life_spawner.disabled = true
    
    GameState.update_hud(hud)
    
    _load_next_wave()
    

func _notification(what):
    if (what == MainLoop.NOTIFICATION_WM_QUIT_REQUEST):
        GameState.load_screen(GameState.Screens.TITLE)
    
#################
# Private methods

func _load_next_wave():
    var wave_info = wave_system.load_next_wave()
     
    rock_spawner.set_frequency(wave_info["rocks_spawn_time"])
    enemy_spawner.set_frequency(wave_info["enemies_spawn_time"])
    powerup_spawner.set_frequency(wave_info["powerup_spawn_time"])
    life_spawner.set_frequency(wave_info["life_spawn_time"])
    
    rock_spawner.reset()
    enemy_spawner.reset()
    powerup_spawner.reset()
    
    hud.show_message("Wave {w}".format({"w": wave_system.current_wave}))
    
func _load_boss():
    rock_spawner.disabled = true
    enemy_spawner.disabled = true
    
    var screen_size = Utils.get_game_size()
    
    animation_player.play("warning")
    hud.show_message("WARNING !")
    
    yield(get_tree().create_timer(2), "timeout")
    
    var boss_inst = BossModel.instance()
    boss_inst.connect("exploded", self, "_on_Boss_exploded")
    boss_inst.connect("fire", self, "_on_Enemy_fire")
    boss_inst.prepare(Vector2(screen_size.x / 2, -100), 100, 1.0)
    
    add_child(boss_inst)
    
#################
# Event callbacks

func _on_Player_fire(bullet, pos, speed, type, target, automatic):
    var inst = bullet.instance()
    inst.prepare(pos, speed, type, target, automatic)
    
    bullets.add_child(inst)
    
func _on_Enemy_fire(bullet, pos, speed, type, target, automatic):
    var inst = bullet.instance()
    inst.prepare(pos, speed, type, target, automatic)
    
    bullets.add_child(inst)

func _on_Player_dead():
    var lives = GameState.lives
    if lives > 0:
        GameState.remove_life()
        GameState.update_hud(hud)
        lives = GameState.lives
        
        player.respawn()
        if lives == 0:
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
    
    life_spawner.spawn()
    
    yield(get_tree().create_timer(1), "timeout")
    _load_next_wave()
   
func _on_Powerup_powerup(powerup_type):
    if powerup_type == PowerupType.Weapon:
        player.bullet_system.upgrade_weapon()
    elif powerup_type == PowerupType.Life:
        GameState.add_life()
        GameState.update_hud(hud)
        
func _on_WaveSystem_timeout():
    _load_boss()