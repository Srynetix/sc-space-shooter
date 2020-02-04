using Godot;

using Dictionary = Godot.Collections.Dictionary;

public class GameScreen : Control {
    // Static
    private static PackedScene bossScene = (PackedScene)GD.Load("res://objects/BossEnemy.tscn");
    private static PackedScene statusToastScene = (PackedScene)GD.Load("res://objects/StatusToast.tscn");

    [BindNode]
    private Player player;
    [BindNode("Bullets")]
    private Node2D bullets;
    [BindNode("CanvasLayer/HUD")]
    private HUD hud;
    [BindNode("CanvasLayer/PopupPanel")]
    private PopupPanel pausePopup;
    [BindNode]
    private AnimationPlayer animationPlayer;
    [BindNode]
    private WaveSystem waveSystem;
    [BindNode("Alarm")]
    private AudioStreamPlayer alarm;
    [BindNode("Spawners/RockSpawner")]
    private Spawner rockSpawner;
    [BindNode("Spawners/PowerupSpawner")]
    private Spawner powerupSpawner;
    [BindNode("Spawners/EnemySpawner")]
    private Spawner enemySpawner;
    [BindNode("Spawners/LifePowerupSpawner")]
    private Spawner lifeSpawner;
    [BindNode("Spawners/BombPowerupSpawner")]
    private Spawner bombPowerupSpawner;
    [BindNodeRoot]
    private FXCamera camera;
    [BindNodeRoot]
    private GameState gameState;

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
        pausePopup.GetNode<Label>("Margin/VBox/Label").Text = Tr("Game is paused.\nPress Resume\nto continue.").CEscape();
        pausePopup.GetNode<Button>("Margin/VBox/Button").Connect("pressed", this, nameof(_ResumeGame));

        waveSystem.Connect("timeout", this, nameof(_On_WaveSystem_Timeout));
        lifeSpawner.disabled = true;

        gameState.ResetGameState();
        gameState.UpdateHUD(hud);

        _LoadNextWave();
    }

    public override void _Notification(int what) {
        if (what == MainLoop.NotificationWmGoBackRequest) {
            gameState.LoadScreen(GameState.Screens.TITLE);
        } else if (what == MainLoop.NotificationWmFocusOut) {
            _PauseGame();
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
        bombPowerupSpawner.Reset();

        hud.ShowMessage(Tr("WAVE") + " " + waveSystem.GetCurrentWave().ToString());
    }

    async private void _LoadBoss() {
        rockSpawner.disabled = true;
        enemySpawner.disabled = true;

        var gameSize = gameState.GetGameSize();
        animationPlayer.Play("warning");
        hud.ShowMessage(Tr("WARNING"));

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

        CallDeferred(nameof(_Start_Wave_Transition));
    }

    async private void _Start_Wave_Transition() {
        var gameSize = gameState.GetGameSize();
        lifeSpawner.SpawnAtPosition(new Vector2(gameSize.x / 2.0f, -50.0f));

        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        _LoadNextWave();
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
            player.GetStatusToast().ShowMessage(Tr("1-UP!"));
        } else if (powerupType == Powerup.PowerupType.Bomb) {
            player.GetBulletSystem().EnableBomb();
        }
    }

    private void _PauseGame() {
        var tree = GetTree();
        pausePopup.PopupCentered();
        tree.Paused = true;
    }

    private void _ResumeGame() {
        var tree = GetTree();
        pausePopup.Hide();
        tree.Paused = false;
    }

    private void _On_WaveSystem_Timeout() {
        _LoadBoss();
    }
}
