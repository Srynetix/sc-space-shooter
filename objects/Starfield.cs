using Godot;

[Tool]
public class Starfield : Control {
    // Consts
    private const int RADIAL_ACCEL = 100;

    // Exports
    [Export] public int Velocity {
        get => _velocity;
        set {
            _velocity = value;
            _shouldUpdate = true;
        }
    }
    [Export] public bool EnableRadialAccel {
        get => _enableRadialAccel;
        set {
            _enableRadialAccel = true;
            _shouldUpdate = true;
        }
    }

    // On ready
    [BindNode] private Particles2D particles;
    private ParticlesMaterial particlesMaterial;
    private bool _enableRadialAccel;
    private int _velocity = 100;
    private bool _shouldUpdate = false;

    public override void _Ready() {
        this.BindNodes();

        particlesMaterial = (ParticlesMaterial)particles.ProcessMaterial;
        particlesMaterial.InitialVelocity = Velocity;

        GetViewport().Connect("size_changed", this, nameof(_ResetState));

        _ResetState();
    }

    public override void _Process(float delta) {
        if (_shouldUpdate) {
            _shouldUpdate = false;
            _ResetState();
        }
    }

    private void _ResetState() {
        Vector2 gameSize = GetViewport().GetVisibleRect().Size;

        particles.Position = gameSize / 2 - new Vector2(0, particlesMaterial.InitialVelocity);
        particlesMaterial.EmissionBoxExtents = new Vector3(gameSize.x / 2, gameSize.y / 2, 1);
        particlesMaterial.InitialVelocity = _velocity;

        if (_enableRadialAccel) {
            particlesMaterial.RadialAccel = RADIAL_ACCEL;
        } else {
            particlesMaterial.RadialAccel = 0;
        }
    }
}
