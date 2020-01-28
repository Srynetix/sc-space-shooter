using Godot;

public class Sparkles : Node2D
{
    [BindNode]
    private Particles2D particles;
    [BindNode]
    private Timer timer;

    public override void _Ready() {
        this.BindNodes();

        timer.Connect("timeout", this, nameof(_On_Timer_Timeout));
        particles.Emitting = true;
    }

    private void _On_Timer_Timeout() {
        QueueFree();
    }
}
