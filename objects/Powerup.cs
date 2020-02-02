using Godot;

public class Powerup : Area2D {
    public enum PowerupType {
        WeaponUpgrade,
        Bomb,
        Life
    }

    // On ready
    [BindNode]
    private VisibilityNotifier2D visibilityNotifier;
    [BindNode]
    private AnimationPlayer animationPlayer;
    [BindNode]
    private CollisionShape2D collisionShape;
    [BindNode("Sound")]
    private AudioStreamPlayer2D sound;

    // Signals
    [Signal] public delegate void powerup();

    // Exports
    [Export] public PowerupType powerupType = PowerupType.WeaponUpgrade;

    // Data
    private Vector2 velocity = new Vector2();

    public override void _Ready() {
        this.BindNodes();

        visibilityNotifier.Connect("screen_exited", this, nameof(_On_VisibilityNotifier2D_ScreenExited));
        Connect("area_entered", this, nameof(_On_Area_Entered));
    }

    public override void _Process(float delta) {
        Position += velocity * delta;
    }

    public void Prepare(Vector2 pos, float speed, float scale) {
        Position = pos;
        Scale = new Vector2(scale, scale);
        velocity = new Vector2(0, speed);
    }

    async private void _On_Area_Entered(Area2D area) {
        if (area.IsInGroup("player")) {
            collisionShape.SetDeferred("disabled", true);
            sound.Play();
            EmitSignal("powerup", powerupType);
            animationPlayer.Play("fade");

            await ToSignal(sound, "finished");
            QueueFree();
        }
    }

    private void _On_VisibilityNotifier2D_ScreenExited() {
        QueueFree();
    }
}
