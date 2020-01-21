using Godot;

public class Bullet : Area2D
{
    public enum BulletType {
        Simple,
        Double,
        Triple,
        Laser,
        SlowFast
    }

    public enum BulletTarget {
        Enemy,
        Player
    }

    private static Texture enemyBulletSprite = (Texture)GD.Load("res://assets/textures/laserRed06.png");
    private static PackedScene sparklesScene = (PackedScene)GD.Load("res://objects/Sparkles.tscn");

    // Exports
    [Export] public BulletTarget bulletTarget = BulletTarget.Enemy;
    [Export] public BulletType bulletType = BulletType.Simple;
    [Export] public bool bulletAutomatic = false;

    // On ready
    private VisibilityNotifier2D visibilityNotifier;
    private Timer slowTimer;
    private Particles2D trail;
    private Sprite sprite;

    // Data
    private Vector2 velocity;
    private float baseSpeed = 0.0f;

    async public override void _Ready() {
        // On ready
        visibilityNotifier = GetNode<VisibilityNotifier2D>("VisibilityNotifier2D");
        slowTimer = GetNode<Timer>("SlowTimer");
        trail = GetNode<Particles2D>("Trail");
        sprite = GetNode<Sprite>("Sprite");

        Connect("area_entered", this, nameof(_On_Area_Entered));
        visibilityNotifier.Connect("screen_exited", this, nameof(_On_VisibilityNotifier2D_ScreenExited));

        if (bulletTarget == BulletTarget.Player) {
            sprite.Texture = enemyBulletSprite;
            trail.Texture = enemyBulletSprite;

            SetCollisionLayerBit(1, false);
            SetCollisionLayerBit(5, true);
            SetCollisionMaskBit(2, false);
            SetCollisionMaskBit(4, false);
            SetCollisionMaskBit(0, true);
        }

        if (bulletAutomatic) {
            _HandleAutomaticMode();
        }

        if (bulletType == BulletType.Laser) {
            sprite.Scale *= 3;
            trail.Scale *= 3;
        } else if (bulletType == BulletType.SlowFast) {
            velocity /= 6.0f;
            slowTimer.Start();
            await ToSignal(slowTimer, "timeout");
            _HandleAutomaticMode();
            velocity *= 1.5f;
        }
    }

    public override void _Process(float delta) {
        Position += velocity * delta;
    }

    public void Prepare(Vector2 pos, float speed, BulletType bType, BulletTarget bTarget, bool automatic) {
        Position = pos;
        bulletType = bType;
        bulletTarget = bTarget;
        bulletAutomatic = automatic;
        baseSpeed = speed;

        if (bTarget == BulletTarget.Player) {
            velocity = new Vector2(0, speed);
        } else {
            velocity = new Vector2(0, -speed);
        }
    }

    private void _HandleAutomaticMode() {
        if (bulletTarget == BulletTarget.Player) {
            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0) {
                _RotateToTarget((Node2D)players[0]);
            }
        } else if (bulletTarget == BulletTarget.Enemy) {
            var enemies = GetTree().GetNodesInGroup("enemies");
            if (enemies.Count > 0) {
                _RotateToTarget((Node2D)enemies[0]);
            }
        }
    }

    private void _RotateToTarget(Node2D target) {
        var direction = (target.Position - Position).Normalized();
        var angle = new Vector2(0, 1).AngleTo(direction);

        Rotation = angle;
        ((ParticlesMaterial)trail.ProcessMaterial).Angle = 360 - Mathf.Rad2Deg(angle);
        velocity = direction * baseSpeed;
    }

    private void _On_Area_Entered(Area2D area) {
        if (area.IsInGroup("rocks") || area.IsInGroup("enemies") || area.IsInGroup("player")) {
            var sparklesPosition = Position;
            var sparkles = (Sparkles)sparklesScene.Instance();
            sparkles.Position = sparklesPosition;
            sparkles.ZIndex = 10;

            GetParent().AddChild(sparkles);
            var hittable = (IHittable)area;
            hittable.Hit();
        }

        QueueFree();
    }

    private void _On_VisibilityNotifier2D_ScreenExited() {
        QueueFree();
    }
}
