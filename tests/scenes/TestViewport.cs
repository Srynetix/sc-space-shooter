using Godot;

public class TestViewport : Control
{
    private Player player;

    public override void _Ready() {
        // On ready
        player = GetNode<Player>("Player");
        
        player.Connect("fire", this, nameof(_On_Fire));
        player.Connect("respawn", this, nameof(_On_Respawn));
    }
    
    private void _On_Fire(PackedScene bullet, Vector2 pos, float speed, Bullet.BulletType bulletType, Bullet.BulletTarget bulletTarget, bool automatic) {
        var instance = (Bullet)bullet.Instance();
        instance.Prepare(pos, speed, bulletType, bulletTarget, automatic);
        AddChild(instance);
    }
    
    private void _On_Respawn() {
        player.Respawn();
    }
}
