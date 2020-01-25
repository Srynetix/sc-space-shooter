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

    private void _On_Fire(Bullet.FireData fireData) {
        var instance = (Bullet)fireData.bullet.Instance();
        instance.Prepare(fireData);
        AddChild(instance);
    }

    private void _On_Respawn() {
        player.Respawn();
    }
}
