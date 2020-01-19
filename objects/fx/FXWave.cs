using Godot;

public class FXWave : Node2D
{
    // On ready
    private AnimationPlayer animationPlayer;
    
    async public override void _Ready() {
        // On ready
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Play("wave");
        
        await ToSignal(animationPlayer, "animation_finished");
        QueueFree();    
    }
}
