using Godot;

public class TestFXCamera : Control {
    private Timer timer;
    private FXCamera camera;
    private Player player;

    public override void _Ready() {
        timer = GetNode<Timer>("Timer");
        player = GetNode<Player>("Player");
        camera = GetTree().Root.GetNode<FXCamera>("FXCamera");

        timer.Connect("timeout", this, nameof(_On_Timeout));
        player.Connect("fire", this, nameof(_On_Fire));
    }

    private void _On_Timeout() {
        camera.Shake();
    }

    private void _On_Fire(Bullet.FireData fireData) {
        var bullet = (Bullet)fireData.bullet.Instance();
        bullet.Prepare(fireData);
        AddChild(bullet);
    }
}
