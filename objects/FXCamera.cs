using Godot;

public class FXCamera : Node2D
{
    // On ready
    private AnimationPlayer animationPlayer;
    private Camera2D camera;
    private GameState gameState;
    
    public override void _Ready() {
        // On ready
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        camera = GetNode<Camera2D>("Camera2D");
        gameState = GetTree().Root.GetNode<GameState>("GameState");
        
        VisualServer.SetDefaultClearColor(new Color(0.0f, 0.0f, 0.0f, 1.0f));    
        
        GetViewport().Connect("size_changed", this, nameof(_ResizeCamera));
        
        _ResizeCamera();
    }
    
    public void _ResizeCamera() {
        var gameSize = gameState.GetGameSize();
        camera.Position = gameSize / 2;
    }
    
    public void Shake() {
        animationPlayer.Play("shake");
    }
}
