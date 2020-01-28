using Godot;

public class Starfield : Control
{
    // On ready
    [BindNode]
    private Particles2D particles;

    // Exports
    [Export]
    public int velocity = 500;

    public override void _Ready() {
        this.BindNodes();

        var material = (ParticlesMaterial)particles.ProcessMaterial;
        material.InitialVelocity = velocity;

        GetViewport().Connect("size_changed", this, nameof(_ResetPosition));

        _ResetPosition();
    }

    private void _ResetPosition() {
        // Set box extents from viewport
        var gameState = GetTree().Root.GetNode<GameState>("GameState");
        var gameSize = gameState.GetGameSize();

        particles.Position = gameSize / 2;
        var particlesMaterial = (ParticlesMaterial)particles.ProcessMaterial;
        particlesMaterial.EmissionBoxExtents = new Vector3(gameSize.x, gameSize.y, 1);
    }
}
