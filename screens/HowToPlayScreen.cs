using Godot;

public class HowToPlayScreen : Control {
    private enum Step {
        Start,
        RocksStart,
        RocksEnd,
        EnemyBefore,
        EnemyStart,
        EnemyEnd,
        Powerup,
        BossBefore,
        BossStart,
        End
    }

    [BindNode] private Player player;
    [BindNode] private StatusToast statusToast;
    [BindNode] private Timer timer;
    [BindNode("RockSpawner")] private Spawner rockSpawner;
    [BindNode("EnemySpawner")] private Spawner enemySpawner;
    [BindNode("PowerupSpawner")] private Spawner powerupSpawner;
    [BindNode("BossSpawner")] private Spawner bossSpawner;
    [BindNode("LifeSpawner")] private Spawner lifeSpawner;
    [BindNode("BombSpawner")] private Spawner bombSpawner;
    [BindNode("CanvasLayer/Margin/Button")] private Button skipButton;

    private string initialStep = nameof(_StartStep3);
    private Step currentStep;
    private bool supportMessageShown = false;

    public override void _Ready() {
        this.BindNodes();

        var gameSize = GameState.GetInstance(this).GetGameSize();
        statusToast.SetTextSize(30);
        statusToast.SetMessageVisibleTime(4.0f);
        statusToast.Position = gameSize / 2;

        player.Connect(nameof(Player.dead), this, nameof(_On_Player_Dead));
        player.Connect(nameof(Player.respawn), this, nameof(_On_Player_Respawn));
        timer.Connect("timeout", this, nameof(_On_Timer_Timeout));
        powerupSpawner.ConnectTargetScene(this, new Godot.Collections.Dictionary {
            { nameof(Powerup.powerup), nameof(_On_Powerup_Powerup) }
        });
        bombSpawner.ConnectTargetScene(this, new Godot.Collections.Dictionary {
            { nameof(Powerup.powerup), nameof(_On_Powerup_Powerup) }
        });
        lifeSpawner.ConnectTargetScene(this, new Godot.Collections.Dictionary {
            { nameof(Powerup.powerup), nameof(_On_Powerup_Powerup) }
        });
        bossSpawner.ConnectTargetScene(this, new Godot.Collections.Dictionary {
            { nameof(Enemy.exploded), nameof(_On_Boss_Exploded) }
        });
        bossSpawner.Connect(nameof(Spawner.spawn), this, nameof(_On_Boss_Spawned));
        skipButton.Connect("pressed", this, nameof(_On_Skip_Button));

        CallDeferred(initialStep);
    }

    async private void _StartStep1() {
        currentStep = Step.Start;

        statusToast.ShowMessage(Tr("TUTORIAL_STEP1_MSG1"));
        await ToSignal(statusToast, "message_shown");

        statusToast.ShowMessage(Tr("TUTORIAL_STEP1_MSG2"));
        await ToSignal(statusToast, "message_shown");

        statusToast.ShowMessage(Tr("TUTORIAL_STEP1_MSG3"));
        await ToSignal(statusToast, "message_shown");

        statusToast.ShowMessage(Tr("TUTORIAL_STEP1_MSG4"));
        await ToSignal(statusToast, "message_shown");

        currentStep = Step.RocksStart;
        supportMessageShown = false;

        // Start spawner, 15 seconds
        rockSpawner.ResetThenSpawn();
        timer.WaitTime = 15;
        timer.Start();
    }

    async private void _StartStep2() {
        currentStep = Step.Powerup;

        // Disable spawner and destroy rocks
        rockSpawner.disabled = true;
        foreach (Rock rock in rockSpawner.GetElements()) {
            rock.Explode();
        }

        statusToast.ShowPriorityMessage(Tr("TUTORIAL_STEP2_MSG1"));
        await ToSignal(statusToast, "message_shown");

        statusToast.ShowPriorityMessage(Tr("TUTORIAL_STEP2_MSG2"));
        await ToSignal(statusToast, "message_shown");

        currentStep = Step.EnemyStart;
        supportMessageShown = false;

        // Start enemy spawner, 15 seconds
        enemySpawner.ResetThenSpawn();
        timer.WaitTime = 15;
        timer.Start();
    }

    async private void _StartStep3() {
        currentStep = Step.BossBefore;

        // Disable enemy spawner and destroy enemies and bullets
        enemySpawner.disabled = true;
        foreach (Node node in enemySpawner.GetElements()) {
            node.Call(nameof(Enemy.Explode));
        }

        statusToast.ShowPriorityMessage(Tr("TUTORIAL_STEP3_MSG1"));
        await ToSignal(statusToast, "message_shown");

        statusToast.ShowMessage(Tr("TUTORIAL_STEP3_MSG2"));
        await ToSignal(statusToast, "message_shown");

        // Spawn powerup
        powerupSpawner.SpawnCentered();

        statusToast.ShowMessage(Tr("TUTORIAL_STEP3_MSG3"));
        await ToSignal(statusToast, "message_shown");

        // Wait a little
        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");

        statusToast.ShowMessage(Tr("TUTORIAL_STEP3_MSG4"));
        await ToSignal(statusToast, "message_shown");

        statusToast.ShowMessage(Tr("TUTORIAL_STEP3_MSG5"));

        powerupSpawner.SpawnCentered();
        await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
        powerupSpawner.SpawnCentered();
        await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
        powerupSpawner.SpawnCentered();

        // Wait a little
        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");

        statusToast.ShowMessage(Tr("TUTORIAL_STEP3_MSG6"));
        await ToSignal(statusToast, "message_all_shown");

        statusToast.ShowMessage(Tr("TUTORIAL_STEP3_MSG7"));
        await ToSignal(statusToast, "message_shown");

        statusToast.ShowMessage(Tr("TUTORIAL_STEP3_MSG8"));
        await ToSignal(statusToast, "message_shown");

        // Lock weapon
        player.GetBulletSystem().LockWeapon();

        bombSpawner.SpawnCentered();
        statusToast.ShowMessage(Tr("TUTORIAL_STEP3_MSG9"));
        await ToSignal(statusToast, "message_shown");

        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");

        statusToast.ShowMessage(Tr("TUTORIAL_STEP3_MSG10"));
        await ToSignal(statusToast, "message_shown");

        await ToSignal(GetTree().CreateTimer(3.0f), "timeout");

        lifeSpawner.SpawnCentered();
        statusToast.ShowMessage(Tr("TUTORIAL_STEP3_MSG11"));
        await ToSignal(statusToast, "message_shown");

        statusToast.ShowMessage(Tr("TUTORIAL_STEP3_MSG12"));
        await ToSignal(statusToast, "message_shown");

        currentStep = Step.BossBefore;

        statusToast.ShowMessage(Tr("TUTORIAL_STEP3_MSG13"));
        await ToSignal(statusToast, "message_shown");

        currentStep = Step.BossStart;
        supportMessageShown = false;

        // Spawn boss
        bossSpawner.SpawnCentered();
    }

    async private void _StartStep4() {
        currentStep = Step.End;

        statusToast.ShowMessage(Tr("TUTORIAL_STEP4_MSG1"));
        await ToSignal(statusToast, "message_shown");

        statusToast.ShowMessage(Tr("TUTORIAL_STEP4_MSG2"));
        await ToSignal(statusToast, "message_shown");

        _StartGame();
    }

    private void _StartGame() {
        var gameState = GameState.GetInstance(this);
        gameState.SetTutorialShown();
        gameState.LoadScreen(GameState.Screens.GAME);
    }

    private void _On_Timer_Timeout() {
        if (currentStep == Step.RocksStart) {
            currentStep = Step.RocksEnd;
            CallDeferred(nameof(_StartStep2));
        } else if (currentStep == Step.EnemyStart) {
            currentStep = Step.EnemyEnd;
            CallDeferred(nameof(_StartStep3));
        }
    }

    private void _On_Player_Dead() {
        if (currentStep == Step.RocksStart) {
            if (!supportMessageShown) {
                statusToast.ShowPriorityMessage(Tr("TUTORIAL_SUPPORT_MSG1"));
                supportMessageShown = true;
            }
        } else if (currentStep == Step.EnemyStart) {
            if (!supportMessageShown) {
                statusToast.ShowPriorityMessage(Tr("TUTORIAL_SUPPORT_MSG2"));
                supportMessageShown = true;
            }
        } else if (currentStep == Step.BossStart) {
            if (!supportMessageShown) {
                statusToast.ShowPriorityMessage(Tr("TUTORIAL_SUPPORT_MSG3"));
                supportMessageShown = true;
            }
        }
    }

    private void _On_Player_Respawn() {
        player.Respawn();
    }

    private void _On_Powerup_Powerup(Powerup powerup) {
        if (powerup.powerupType == Powerup.PowerupType.WeaponUpgrade) {
            player.UpgradeWeapon();
        } else if (powerup.powerupType == Powerup.PowerupType.Life) {
            player.GetStatusToast().ShowMessage(Tr("1-UP!"));
        } else if (powerup.powerupType == Powerup.PowerupType.Bomb) {
            player.GetBulletSystem().EnableBomb();
        }
    }

    private void _On_Boss_Spawned(Enemy boss) {
        boss.SetHitPointsFactor(10);
    }


    private void _On_Boss_Exploded(Node2D node) {
        CallDeferred(nameof(_StartStep4));
    }

    private void _On_Skip_Button() {
        skipButton.MouseFilter = MouseFilterEnum.Ignore;
        _StartGame();
    }
}
