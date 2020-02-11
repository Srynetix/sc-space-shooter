using Godot;

public class Transition : CanvasLayer {
    [BindNode]
    private AnimationPlayer animationPlayer;

    public override void _Ready() {
        this.BindNodes();
    }

    public static Transition GetInstance(Node origin) {
        return (Transition)origin.GetTree().Root.GetNode(nameof(Transition));
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
