using Godot;

public class TestFXCamera : Control {
    private Timer timer;
    private FXCamera camera;

    public override void _Ready() {
        timer = GetNode<Timer>("Timer");
        camera = GetNode<FXCamera>("FXCamera");

        timer.Connect("timeout", this, nameof(_On_Timeout));
    }

    private void _On_Timeout() {
        camera.Shake();
    }
}
