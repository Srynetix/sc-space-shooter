using Godot;

public class Starfield : Control
{
    // On ready
    private Particles2D particles;
    
    // Exports
    [Export]
    public int velocity = 500;
    
    public override void _Ready()
    {
        // On ready
        particles = GetNode<Particles2D>("Particles2D");
        
        var material = (ParticlesMaterial)particles.ProcessMaterial;
        material.InitialVelocity = velocity;    
    }
}
