using Godot;

using Dictionary = Godot.Collections.Dictionary;

public class TestBoss : Node2D {

    [BindNode] private Player player;
    [BindNode("BombSpawner")] private Spawner spawner;

    private static PackedScene bossScene = (PackedScene)GD.Load("res://nodes/objects/BossEnemy.tscn");

    public override void _Ready() {
        this.BindNodes();

        player.Connect("respawn", this, nameof(_On_Respawn));
        spawner.ConnectTargetScene(this, new Dictionary {
            { "powerup", nameof(_On_Powerup_Powerup) }
        });

        _SpawnBoss();
    }

    private void _On_Respawn() {
        player.Respawn();
    }

    private void _On_Boss_Exploded(Node2D instance) {
        CallDeferred(nameof(_SpawnBoss));
    }

    private void _On_Powerup_Powerup(Powerup powerup) {
        if (powerup.powerupType == Powerup.PowerupType.Bomb) {
            player.GetBulletSystem().EnableBomb();
        }
    }

    private void _SpawnBoss() {
        var gameSize = GameState.GetInstance(this).GetGameSize();
        var bossInstance = bossScene.InstanceAs<BossEnemy>();
        bossInstance.Connect("exploded", this, nameof(_On_Boss_Exploded));
        bossInstance.Prepare(new Vector2(gameSize.x / 2, -100), 100, 1.0f);
        AddChild(bossInstance);
    }
}
