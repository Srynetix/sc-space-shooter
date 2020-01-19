using Godot;

public class Transition : CanvasLayer
{
    private AnimationPlayer animationPlayer;
    
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");     
    }

    async public void FadeToScene(string scenePath, float transitionSpeed = 1.0f) {
        var scene = GD.Load<PackedScene>(scenePath);
        
        animationPlayer.PlaybackSpeed = transitionSpeed;
        animationPlayer.Play("fadeout");
        await ToSignal(animationPlayer, "animation_finished");

        GetTree().ChangeSceneTo(scene);
        animationPlayer.Play("fadein");
    }
}
