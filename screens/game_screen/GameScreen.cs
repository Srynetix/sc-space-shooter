using Godot;

using Dictionary = Godot.Collections.Dictionary;

public class GameScreen : Control
{
    // On ready
    private Player player;
    private Node2D bullets;
    private HUD hud;
    private AnimationPlayer animationPlayer;
    private WaveSystem waveSystem;
    private AudioStreamPlayer2D alarm;
    private FXCamera camera;
    private Spawner rockSpawner;
    private Spawner powerupSpawner;
    private Spawner enemySpawner;
    private Spawner lifeSpawner;
    private GameState gameState;
    
    public override void _Ready() {
        // On ready
        player = GetNode<Player>("Player");
        bullets = GetNode<Node2D>("Bullets");
        hud = GetNode<HUD>("CanvasLayer/HUD");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        waveSystem = GetNode<WaveSystem>("WaveSystem");
        alarm = GetNode<AudioStreamPlayer2D>("Alarm");
        camera = GetNode<FXCamera>("FXCamera");
        rockSpawner = GetNode<Spawner>("Spawners/RockSpawner");
        powerupSpawner = GetNode<Spawner>("Spawners/PowerupSpawner");
        enemySpawner = GetNode<Spawner>("Spawners/EnemySpawner");
        lifeSpawner = GetNode<Spawner>("Spawners/LifePowerupSpawner");
        gameState = GetTree().Root.GetNode<GameState>("GameState");
        
        player.Connect("fire", this, nameof(_On_Fire));
        player.Connect("dead", this, nameof(_On_Player_Dead));
        player.Connect("respawn", this, nameof(_On_Player_Respawn));
        
        rockSpawner.ConnectTargetScene(this, new Dictionary {
            { "exploded", nameof(_On_Rock_Exploded) }
        });
        powerupSpawner.ConnectTargetScene(this, new Dictionary {
            { "powerup", nameof(_On_Powerup_Powerup) }
        });
        lifeSpawner.ConnectTargetScene(this, new Dictionary {
            { "powerup", nameof(_On_Powerup_Powerup) }
        });
        enemySpawner.ConnectTargetScene(this, new Dictionary {
            { "exploded", nameof(_On_Enemy_Exploded) },
            { "fire", nameof(_On_Fire) }
        });
        
        waveSystem.Connect("timeout", this, nameof(_On_WaveSystem_Timeout));
        lifeSpawner.disabled = true;
        
        gameState.ResetGameState();
        gameState.UpdateHUD(hud);
        
        _LoadNextWave();
    }
    
    public override void _Notification(int what) {
        if (what == MainLoop.NotificationWmGoBackRequest) {
            gameState.LoadScreen(GameState.Screens.TITLE);
        }
    }
    
    private void _LoadNextWave() {
        var waveInfo = waveSystem.LoadNextWave();
        
        rockSpawner.SetFrequency((float)waveInfo["rocks_spawn_time"]);
        enemySpawner.SetFrequency((float)waveInfo["enemies_spawn_time"]);
        powerupSpawner.SetFrequency((float)waveInfo["powerup_spawn_time"]);
        
        rockSpawner.Reset();
        enemySpawner.Reset();
        powerupSpawner.Reset();
        
        hud.ShowMessage(Tr("WAVE") + " " + waveSystem.GetCurrentWave().ToString());
    }
    
    async private void _LoadBoss() {
        rockSpawner.disabled = true;
        enemySpawner.disabled = true;
        
        var gameSize = Utils.GetGameSize();
        animationPlayer.Play("warning");
        hud.ShowMessage(Tr("WARNING"));
        
        alarm.Play();
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        alarm.Play();
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        
        var bossScene = (PackedScene)GD.Load("res://objects/enemies/BossEnemy.tscn");
        var bossInstance = (BossEnemy)bossScene.Instance();
        bossInstance.fireTime = 0.25f + 0.25f * waveSystem.GetCurrentWave();
        bossInstance.Connect("exploded", this, nameof(_On_Boss_Exploded));
        bossInstance.Connect("fire", this, nameof(_On_Fire));
        bossInstance.Prepare(new Vector2(gameSize.x / 2.0f, -100.0f), 100.0f, 1.0f);
        
        AddChild(bossInstance);
    }
    
    private void _On_Fire(PackedScene bullet, Vector2 pos, float speed, Bullet.BulletType bulletType, Bullet.BulletTarget bulletTarget, bool automatic) {
        var instance = (Bullet)bullet.Instance();
        instance.Prepare(pos, speed, bulletType, bulletTarget, automatic);
        bullets.AddChild(instance);
    }
    
    private void _On_Player_Dead() {
        camera.Shake();
    }
    
    private void _On_Player_Respawn() {
        var lives = gameState.GetLives();
        if (lives > 1) {
            gameState.RemoveLife();
            gameState.UpdateHUD(hud);
            lives = gameState.GetLives();
            
            player.Respawn();
            if (lives == 1) {
                hud.ShowMessage(Tr("LASTLIFE"));
            }
        } else {
            gameState.LoadScreen(GameState.Screens.GAMEOVER);
        }
    }
    
    private void _On_Rock_Exploded() {
        gameState.AddScore(100);
        gameState.UpdateHUD(hud);
    }
    
    private void _On_Enemy_Exploded() {
        gameState.AddScore(200);
        gameState.UpdateHUD(hud);
    }
    
    async private void _On_Boss_Exploded() {
        gameState.AddScore(2000 * waveSystem.GetCurrentWave());
        gameState.UpdateHUD(hud);
        camera.Shake();
        
        var gameSize = Utils.GetGameSize();
        lifeSpawner.SpawnAtPosition(new Vector2(gameSize.x / 2.0f, -50.0f));
        
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        _LoadNextWave();
    }
    
    private void _On_Powerup_Powerup(Powerup.PowerupType powerupType) {
        if (powerupType == Powerup.PowerupType.Weapon) {
            player.GetBulletSystem().UpgradeWeapon();
        } else if (powerupType == Powerup.PowerupType.Life) {
            gameState.AddLife();
            gameState.UpdateHUD(hud);
        }
    }
    
    private void _On_WaveSystem_Timeout() {
        _LoadBoss();
    }
}
