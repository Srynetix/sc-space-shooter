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
    }

    private void _On_Timeout() {
        camera.Shake();
    }
}
