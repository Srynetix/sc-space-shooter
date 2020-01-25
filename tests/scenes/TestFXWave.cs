using Godot;

using Dictionary = Godot.Collections.Dictionary;

public class TestFXWave : Control {
    private static PackedScene fxWaveScene = (PackedScene)GD.Load("res://objects/FXWave.tscn");
    private Spawner spawner;
    private Player player;
    private GameState gameState;

    public override void _Ready() {
        GD.Randomize();

        spawner = GetNode<Spawner>("Spawner");
        player = GetNode<Player>("Player");
        gameState = GetTree().Root.GetNode<GameState>("GameState");

        spawner.ConnectTargetScene(this, new Dictionary {
            { "fire", nameof(_On_Fire) }
        });
        player.Connect("fire", this, nameof(_On_Fire));
        player.Connect("respawn", this, nameof(_On_Respawn));
        player.GetBulletSystem().Connect("type_switch", this, nameof(_On_TypeSwitch));

        // var gameSize = gameState.GetGameSize();
        // while (true) {
        //     // var randPos = new Vector2((float)GD.RandRange(0.0f, gameSize.x), (float)GD.RandRange(0.0f, gameSize.y));
        //     var randPos = gameSize / 2;
        //     var fxWave = (FXWave)fxWaveScene.Instance();
        //     AddChild(fxWave);

        //     // Start wave
        //     fxWave.StartThenFree(randPos, 0.25f);
        //     await ToSignal(fxWave, "finished");

        //     // Wait 2 seconds
        //     await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
        // }
    }

    private void _On_Fire(Bullet.FireData fireData) {
        var bullet = (Bullet)fireData.bullet.Instance();
        bullet.Prepare(fireData);
        AddChild(bullet);
    }

    private void _On_Respawn() {
        player.Respawn();
    }

    private void _On_TypeSwitch(Bullet.BulletType prevType, Bullet.BulletType newType) {
        if (newType != Bullet.BulletType.Bomb) {
            player.GetBulletSystem().SwitchType(Bullet.BulletType.Bomb);
        }
    }
}
