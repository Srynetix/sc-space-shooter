using Godot;

using Dictionary = Godot.Collections.Dictionary;

public class TestEnemies : Control {
    [BindNode("EnemySpawner")]
    private Spawner spawner;

    public override void _Ready() {
        this.BindNodes();
    }
}
