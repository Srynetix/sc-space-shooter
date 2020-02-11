using Godot;

public class FXWave : Area2D {
    [Signal] public delegate void finished();

    private static PackedScene sparklesScene = (PackedScene)GD.Load("res://nodes/fx/Sparkles.tscn");

    // On ready
    [BindNode] private Tween tween;
    [BindNode] private Sprite sprite;
    [BindNode] private CollisionShape2D collisionShape;
    [BindNode] private Particles2D particles;
    [BindNode] private AudioStreamPlayer sound;
    [BindNodeRoot] private FXCamera fXCamera;
    [BindNodeRoot] private GameState gameState;

    private Shape2D shape;
    private ParticlesMaterial particlesMaterial;
    private ShaderMaterial shaderMaterial;

    public override void _Ready() {
        this.BindNodes();

        shape = (CircleShape2D)collisionShape.Shape;
        shaderMaterial = (ShaderMaterial)sprite.Material;
        particlesMaterial = (ParticlesMaterial)particles.ProcessMaterial;

        Connect("area_entered", this, nameof(_On_Area_Entered));

        var gameSize = gameState.GetGameSize();
        var maxv = gameSize.x;
        sprite.Scale = new Vector2(maxv, maxv) * 1.75f / sprite.Texture.GetSize();
    }

    async public void Start(Vector2 target, float force = 1.0f) {
        var gameSize = gameState.GetGameSize();
        var maxv = gameSize.x * force;

        sprite.Position = target;
        particles.Position = target;
        collisionShape.Position = target;

        var finalParticlesVelocity = maxv * 20 * force;

        tween.InterpolateProperty(shaderMaterial, "shader_param/radius", 0, force, 3.0f, Tween.TransitionType.Expo, Tween.EaseType.Out);
        tween.InterpolateProperty(shaderMaterial, "shader_param/waveColor", Colors.Transparent, Color.Color8(0, 255, 255, 128), 2.0f, Tween.TransitionType.Expo, Tween.EaseType.Out);
        tween.InterpolateProperty(shape, "radius", 0, maxv * 1.75f, 3.0f, Tween.TransitionType.Expo, Tween.EaseType.Out);
        tween.InterpolateProperty(particlesMaterial, "initial_velocity", 10, finalParticlesVelocity, 3.0f, Tween.TransitionType.Expo, Tween.EaseType.Out);
        tween.Start();
        sound.Play();
        fXCamera.Shake(true);
        await ToSignal(tween, "tween_all_completed");

        fXCamera.Reset();
        sound.Stop();
        particles.Emitting = false;

        tween.InterpolateProperty(shaderMaterial, "shader_param/radius", force, 0, 1.0f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
        tween.InterpolateProperty(shape, "radius", maxv * 1.75f, 0, 1.0f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
        tween.InterpolateProperty(shaderMaterial, "shader_param/waveColor", Color.Color8(0, 255, 255, 128), Colors.Transparent, 1.0f, Tween.TransitionType.Expo, Tween.EaseType.In);
        tween.InterpolateProperty(particlesMaterial, "initial_velocity", particlesMaterial.InitialVelocity, 0, 0.5f, Tween.TransitionType.Expo, Tween.EaseType.InOut);
        tween.InterpolateProperty(particlesMaterial, "orbit_velocity", particlesMaterial.OrbitVelocity, 0, 0.5f, Tween.TransitionType.Expo, Tween.EaseType.InOut);
        tween.InterpolateProperty(particles, "amount", 64, 1, 0.5f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();

        await ToSignal(tween, "tween_all_completed");
        EmitSignal("finished");
    }

    async public void StartThenFree(Vector2 target, float force = 1.0f) {
        Start(target, force);

        // Wait for termination
        await ToSignal(this, "finished");

        QueueFree();
    }

    private void _On_Area_Entered(Area2D area) {
        if (area.IsInGroup("rocks") || area.IsInGroup("enemies")) {
            var sparklesPosition = Position;
            var sparkles = sparklesScene.InstanceAs<Sparkles>();
            sparkles.Position = sparklesPosition;
            sparkles.ZIndex = 10;

            GetParent().AddChild(sparkles);
        }
    }
}
