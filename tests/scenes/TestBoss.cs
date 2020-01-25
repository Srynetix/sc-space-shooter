using Godot;

public class TestBoss : Node2D {

    private Player player;
    private GameState gameState;

    private static PackedScene bossScene = (PackedScene)GD.Load("res://objects/BossEnemy.tscn");

    public override void _Ready() {
        player = GetNode<Player>("Player");
        gameState = GetTree().Root.GetNode<GameState>("GameState");

        var gameSize = gameState.GetGameSize();
        player.Connect("fire", this, nameof(_On_Fire));
        player.Connect("respawn", this, nameof(_On_Respawn));

        var bossInstance = (BossEnemy)bossScene.Instance();
        bossInstance.Connect("fire", this, nameof(_On_Fire));
        bossInstance.Prepare(new Vector2(gameSize.x / 2, -100), 100, 1.0f);
        AddChild(bossInstance);
    }

    private void _On_Fire(Bullet.FireData fireData) {
        var instance = (Bullet)fireData.bullet.Instance();
        instance.Prepare(fireData);
        AddChild(instance);
    }

    private void _On_Respawn() {
        player.Respawn();
    }
}
