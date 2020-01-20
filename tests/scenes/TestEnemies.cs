using Godot;

using Dictionary = Godot.Collections.Dictionary;

public class TestEnemies : Control
{
    private Spawner spawner;
    
    public override void _Ready() {
        // On ready
        spawner = GetNode<Spawner>("EnemySpawner");
        spawner.ConnectTargetScene(this, new Dictionary {
            { "fire", nameof(_On_Spawner_Fire) }
        });
    }
    
    private void _On_Spawner_Fire(PackedScene bullet, Vector2 pos, float speed, Bullet.BulletType bulletType, Bullet.BulletTarget bulletTarget, bool automatic) {
        var instance = (Bullet)bullet.Instance();
        instance.Prepare(pos, speed, bulletType, bulletTarget, automatic);
        AddChild(instance);
    }
}
