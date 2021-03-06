using Godot;

using Dictionary = Godot.Collections.Dictionary;

public class GameScreen : Control {
    // Static
    private static PackedScene bossScene = (PackedScene)GD.Load("res://nodes/objects/BossEnemy.tscn");
    private static PackedScene statusToastScene = (PackedScene)GD.Load("res://nodes/ui/StatusToast.tscn");

    [BindNode] private Player player;
    [BindNode("Bullets")] private Node2D bullets;
    [BindNode("CanvasLayer/HUD")] private HUD hud;
    [BindNode] private AnimationPlayer animationPlayer;
    [BindNode] private WaveSystem waveSystem;
    [BindNode("Alarm")] private AudioStreamPlayer alarm;
    [BindNode("Spawners/RockSpawner")] private Spawner rockSpawner;
    [BindNode("Spawners/PowerupSpawner")] private Spawner powerupSpawner;
    [BindNode("Spawners/EnemySpawner")] private Spawner enemySpawner;
    [BindNode("Spawners/LifePowerupSpawner")] private Spawner lifeSpawner;
    [BindNode("Spawners/BombPowerupSpawner")] private Spawner bombPowerupSpawner;
    [BindNode] private Starfield starfield;
    [BindNodeRoot] private FXCamera camera;
    [BindNodeRoot] private GameState gameState;

    #region Overrides

    public override void _Ready() {
        this.BindNodes();

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
        });
        bombPowerupSpawner.ConnectTargetScene(this, new Dictionary {
            { "powerup", nameof(_On_Powerup_Powerup) }
        });

        enemySpawner.Connect("spawn", this, nameof(_On_Enemy_Spawned));

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

    public override void _Process(float delta) {
        if (Input.IsActionJustPressed("ui_cancel")) {
            gameState.LoadScreen(GameState.Screens.TITLE);
        }
    }

    #endregion

    #region Private methods

    private void _LoadNextWave() {
        var waveInfo = waveSystem.LoadNextWave();

        rockSpawner.SetFrequency((float)waveInfo["rocks_spawn_time"]);
        enemySpawner.SetFrequency((float)waveInfo["enemies_spawn_time"]);
        powerupSpawner.SetFrequency((float)waveInfo["powerup_spawn_time"]);

        rockSpawner.Reset();
        enemySpawner.Reset();
        powerupSpawner.Reset();
        bombPowerupSpawner.Reset();

        starfield.Velocity = 100 * waveSystem.GetCurrentWave();

        hud.ShowMessage(Tr("HUD_WAVE") + " " + waveSystem.GetCurrentWave().ToString());
    }

    async private void _LoadBoss() {
        rockSpawner.disabled = true;
        enemySpawner.disabled = true;
        starfield.EnableRadialAccel = true;

        var gameSize = gameState.GetGameSize();
        animationPlayer.Play("warning");
        hud.ShowMessage(Tr("HUD_WARNING"));

        alarm.Play();
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        alarm.Play();
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

        var bossInstance = bossScene.InstanceAs<BossEnemy>();
        bossInstance.Connect("exploded", this, nameof(_On_Boss_Exploded));
        bossInstance.Prepare(new Vector2(gameSize.x / 2.0f, -100.0f), 100.0f, 1.0f);
        AddChild(bossInstance);

        bossInstance.SetHitPointsFactor(1 + (waveSystem.GetCurrentWave() - 1) * 20);
        bossInstance.SetFireTimeFactor(1 + (waveSystem.GetCurrentWave() - 1) * 0.25f);
    }

    async private void _Show_Score_Message(Node2D node, int score) {
        var toastInstance = statusToastScene.InstanceAs<StatusToast>();
        toastInstance.Position = node.Position + new Vector2(0, 20.0f);
        toastInstance.Scale = node.Scale * 1.5f;
        toastInstance.MessageVisibleTime = 0.5f;
        toastInstance.MessageOffset = new Vector2(0, 0);
        AddChild(toastInstance);

        var message = (score > 0) ? "+" + score.ToString() : "-" + score.ToString();
        var color = (score > 0) ? Colors.Green : Colors.Red;

        toastInstance.ShowMessageWithColor(message, color);
        await ToSignal(toastInstance, "message_shown");

        toastInstance.QueueFree();
    }

    async private void _Start_Wave_Transition() {
        var gameSize = gameState.GetGameSize();
        lifeSpawner.SpawnAtPosition(new Vector2(gameSize.x / 2.0f, -50.0f));

        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        _LoadNextWave();
    }

    #endregion

    #region Event handlers

    private void _On_Player_Dead() {
        camera.Shake();
    }

    private void _On_Player_Respawn() {
        var lives = gameState.GetLives();
        if (lives > 0) {
            gameState.RemoveLife();
            gameState.UpdateHUD(hud);
            lives = gameState.GetLives();

            player.Respawn();
            if (lives == 0) {
                hud.ShowMessage(Tr("HUD_LASTLIFE"));
            }
        } else {
            gameState.LoadScreen(GameState.Screens.GAMEOVER);
        }
    }

    private void _On_Rock_Exploded(Node2D node) {
        gameState.AddScore(100);
        gameState.UpdateHUD(hud);

        _Show_Score_Message(node, 100);
    }

    private void _On_Enemy_Exploded(Node2D node) {
        gameState.AddScore(200);
        gameState.UpdateHUD(hud);

        _Show_Score_Message(node, 200);
    }

    private void _On_Enemy_Spawned(Node node) {
        var enemy = (Enemy)node;
        enemy.SetHitPointsFactor(1 + (waveSystem.GetCurrentWave() - 1) * 2);
        enemy.SetFireTimeFactor(1 + (waveSystem.GetCurrentWave() - 1) * 0.25f);
    }

    private void _On_Boss_Exploded(Node2D node) {
        int scoreToAdd = 2000 * waveSystem.GetCurrentWave();
        gameState.AddScore(scoreToAdd);
        gameState.UpdateHUD(hud);
        _Show_Score_Message(node, scoreToAdd);
        camera.Shake();

        starfield.EnableRadialAccel = false;

        CallDeferred(nameof(_Start_Wave_Transition));
    }

    private void _On_Powerup_Powerup(Powerup powerup) {
        var powerupType = powerup.powerupType;
        if (powerupType == Powerup.PowerupType.WeaponUpgrade) {
            if (player.CanUpgradeWeapon()) {
                player.UpgradeWeapon();
            } else {
                gameState.AddScore(200);
                gameState.UpdateHUD(hud);
                _Show_Score_Message(powerup, 200);
            }
        } else if (powerupType == Powerup.PowerupType.Life) {
            gameState.AddLife();
            gameState.UpdateHUD(hud);
            player.GetStatusToast().ShowMessage(Tr("PLAYER_LIFE_MSG"));
        } else if (powerupType == Powerup.PowerupType.Bomb) {
            player.GetBulletSystem().EnableBomb();
        }
    }

    private void _On_WaveSystem_Timeout() {
        _LoadBoss();
    }

    #endregion
}
