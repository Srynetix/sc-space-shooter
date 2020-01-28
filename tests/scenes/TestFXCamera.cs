using Godot;

public class TestFXCamera : Control {
    [BindNode]
    private Timer timer;
    [BindNode]
    private Player player;
    [BindNodeRoot]
    private FXCamera camera;

    public override void _Ready() {
        this.BindNodes();

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
