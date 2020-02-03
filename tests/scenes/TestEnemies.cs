using Godot;

using Dictionary = Godot.Collections.Dictionary;

public class TestEnemies : Control {
    [BindNode("EnemySpawner")]
    private Spawner spawner;

    public override void _Ready() {
        this.BindNodes();

        spawner.ConnectTargetScene(this, new Dictionary {
            { "fire", nameof(_On_Spawner_Fire) }
        });
    }

    private void _On_Spawner_Fire(Bullet.FireData fireData) {
        var instance = (Bullet)fireData.bullet.Instance();
        instance.Prepare(fireData);
        AddChild(instance);
    }
}
