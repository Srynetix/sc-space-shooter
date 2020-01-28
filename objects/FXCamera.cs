using Godot;

public class FXCamera : Node2D
{
    // On ready
    [BindNode]
    private AnimationPlayer animationPlayer;
    [BindNode]
    private Camera2D camera;
    [BindNodeRoot]
    private GameState gameState;

    public override void _Ready() {
        this.BindNodes();

        VisualServer.SetDefaultClearColor(new Color(0.0f, 0.0f, 0.0f, 1.0f));

        GetViewport().Connect("size_changed", this, nameof(_ResizeCamera));

        _ResizeCamera();
    }

    public void _ResizeCamera() {
        var gameSize = gameState.GetGameSize();
        camera.Position = gameSize / 2;
    }

    public void Shake(bool loop = false) {
        if (!loop && animationPlayer.CurrentAnimation == "shake_loop") {
            // Do not stop the loop with simple shake
            return;
        }

        if (loop) {
            animationPlayer.Play("shake_loop");
        } else {
            animationPlayer.Play("shake");
        }
    }

    public void Reset() {
        animationPlayer.Stop();
        camera.Offset = new Vector2(0, 0);
    }
}
