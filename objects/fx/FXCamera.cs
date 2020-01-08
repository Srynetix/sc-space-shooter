using Godot;

public class FXCamera : Node2D
{
    // On ready
    private AnimationPlayer animationPlayer;
    
    public override void _Ready() {
        // On ready
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        
        VisualServer.SetDefaultClearColor(new Color(0.0f, 0.0f, 0.0f, 1.0f));    
    }
    
    public void Shake() {
        animationPlayer.Play("shake");
    }
}
