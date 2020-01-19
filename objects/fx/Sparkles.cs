using Godot;

public class Sparkles : Node2D
{
    // On ready
    private Particles2D particles;
    private Timer timer;
    
    public override void _Ready() {
        // On ready
        particles = GetNode<Particles2D>("Particles2D");
        timer = GetNode<Timer>("Timer");
        
        timer.Connect("timeout", this, nameof(_On_Timer_Timeout));
        particles.Emitting = true;    
    }
    
    private void _On_Timer_Timeout() {
        QueueFree();
    }
}
