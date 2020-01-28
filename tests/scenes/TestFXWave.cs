using Godot;

using Dictionary = Godot.Collections.Dictionary;

public class TestFXWave : Control {
    private static PackedScene fxWaveScene = (PackedScene)GD.Load("res://objects/FXWave.tscn");

    [BindNode]
    private Spawner spawner;
    [BindNode]
    private Player player;
    [BindNodeRoot]
    private GameState gameState;

    public override void _Ready() {
        this.BindNodes();

        GD.Randomize();

        spawner.ConnectTargetScene(this, new Dictionary {
            { "fire", nameof(_On_Fire) }
        });
        player.Connect("fire", this, nameof(_On_Fire));
        player.Connect("respawn", this, nameof(_On_Respawn));
        player.GetBulletSystem().Connect("type_switch", this, nameof(_On_TypeSwitch));
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
