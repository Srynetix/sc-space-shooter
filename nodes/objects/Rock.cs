using Godot;

public class Rock : Area2D {

    // On ready
    [BindNode("Sprite/Trail")]
    private Particles2D trail;
    [BindNode]
    private AnimationPlayer animationPlayer;
    [BindNode]
    private CollisionShape2D collisionShape;
    [BindNode]
    private Sprite sprite;
    [BindNode("ExplosionSound")]
    private AudioStreamPlayer2D explosionSound;
    [BindNodeRoot]
    private GameState gameState;

    // Signals
    [Signal]
    public delegate void exploded(Node2D node);

    // Constants
    private const int BASE_HIT_POINTS = 3;
    private const float X_VELOCITY_SPEED = 100.0f;

    // Data
    private Vector2 velocity = new Vector2();
    private int hitPoints = 0;
    private int hitCount = 0;
    private bool hasExploded = false;

    public override void _Ready() {
        this.BindNodes();

        Connect("area_entered", this, nameof(_On_Area_Entered));

        ((ParticlesMaterial)trail.ProcessMaterial).Scale = Scale.x;
    }

    public override void _Process(float delta) {
        Position += velocity * delta;
        Rotation += delta / 2.0f;
        ((ParticlesMaterial)trail.ProcessMaterial).Angle = 360 - Mathf.Rad2Deg(Rotation);

        _HandlePositionWrap();
    }

    public void Prepare(Vector2 pos, float speed, float scale) {
        Position = pos;
        Scale = new Vector2(scale, scale);
        velocity = new Vector2((float)GD.RandRange((double)-X_VELOCITY_SPEED, (double)X_VELOCITY_SPEED), speed);

        hitPoints = (int)Mathf.Ceil(scale * BASE_HIT_POINTS);
    }

    public void Hit() {
        if (!hasExploded) {
            animationPlayer.Play("tint");
            hitCount += 1;
            if (hitCount == hitPoints) {
                Explode();
            }
        }
    }

    public void Explode() {
        hasExploded = true;
        _Explode();
    }

    async private void _Explode() {
        EmitSignal(nameof(exploded), this);
        CallDeferred(nameof(_DisableShape));
        explosionSound.Play();
        animationPlayer.Play("explode");

        await ToSignal(animationPlayer, "animation_finished");
        QueueFree();
    }

    private void _DisableShape() {
        collisionShape.SetDeferred("disabled", true);
    }

    private void _HandlePositionWrap() {
        var spriteSize = sprite.Texture.GetSize() * Scale;
        var gameSize = gameState.GetGameSize();
        var xScreenLimits = new Vector2(-spriteSize.x / 2.0f, gameSize.x + spriteSize.x / 2.0f);

        if (Position.x > xScreenLimits.y) {
            Position = new Vector2(xScreenLimits.x, Position.y);
        } else if (Position.x < xScreenLimits.x) {
            Position = new Vector2(xScreenLimits.y, Position.y);
        }

        if (Position.y - sprite.Texture.GetSize().y > gameSize.y) {
            QueueFree();
        }
    }

    private void _On_Area_Entered(Area2D area) {
        if (area.IsInGroup("player") || area.IsInGroup("wave")) {
            Explode();
        } else if (area.IsInGroup("bullets")) {
            Hit();
        }
    }
}
