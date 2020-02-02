using Godot;

public class Bullet : Area2D
{
    public enum BulletType {
        Simple,
        Double,
        Triple,
        Laser,
        SlowFast,
        Bomb
    }

    public enum BulletTarget {
        Enemy,
        Player
    }

    public class FireData: Resource {
        public PackedScene bullet;
        public Vector2 pos;
        public float speed;
        public BulletType bulletType;
        public BulletTarget bulletTarget;
        public bool automatic;

        public FireData(PackedScene bullet, Vector2 pos, float speed, BulletType bulletType, BulletTarget bulletTarget, bool automatic) {
            this.bullet = bullet;
            this.pos = pos;
            this.speed = speed;
            this.bulletType = bulletType;
            this.bulletTarget = bulletTarget;
            this.automatic = automatic;
        }
    }

    private static Texture enemyBulletSprite = (Texture)GD.Load("res://assets/textures/laserRed06.png");
    private static PackedScene sparklesScene = (PackedScene)GD.Load("res://objects/Sparkles.tscn");
    private static PackedScene fxWaveScene = (PackedScene)GD.Load("res://objects/FXWave.tscn");

    // Exports
    [Export] public BulletTarget bulletTarget = BulletTarget.Enemy;
    [Export] public BulletType bulletType = BulletType.Simple;
    [Export] public bool bulletAutomatic = false;

    // On ready
    [BindNode]
    private VisibilityNotifier2D visibilityNotifier;
    [BindNode("SlowTimer")]
    private Timer slowTimer;
    [BindNode("BombTimer")]
    private Timer bombTimer;
    [BindNode("Trail")]
    private Particles2D trail;
    [BindNode("Sprite")]
    private Sprite sprite;

    // Data
    private Vector2 velocity;
    private float baseSpeed = 0.0f;

    async public override void _Ready() {
        this.BindNodes();

        Connect("area_entered", this, nameof(_On_Area_Entered));
        visibilityNotifier.Connect("screen_exited", this, nameof(_On_VisibilityNotifier2D_ScreenExited));
        bombTimer.Connect("timeout", this, nameof(_On_BombTimer_Timeout));

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
        } else if (bulletType == BulletType.Bomb) {
            bombTimer.Start();
        }
    }

    public override void _Process(float delta) {
        Position += velocity * delta;

        if (bulletType == Bullet.BulletType.Bomb) {
            RotationDegrees += velocity.y * delta;
        }
    }

    public void Prepare(FireData fireData) {
        Position = fireData.pos;
        bulletType = fireData.bulletType;
        bulletTarget = fireData.bulletTarget;
        bulletAutomatic = fireData.automatic;
        baseSpeed = fireData.speed;

        if (bulletTarget == BulletTarget.Player) {
            velocity = new Vector2(0, baseSpeed);
        } else {
            velocity = new Vector2(0, -baseSpeed);
        }

        if (bulletType == BulletType.Bomb) {
            // Slow down a bit
            baseSpeed /= 2.0f;
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

    private void _TriggerWave(Vector2 position) {
        Debug.GetInstance(this).GetLogger(GetType().Name).Info("Bomb exploded");

        var wave = (FXWave)fxWaveScene.Instance();
        GetParent().AddChild(wave);
        wave.StartThenFree(position, 0.35f);
    }

    private void _TriggerSparkles(Vector2 position) {
        var sparkles = (Sparkles)sparklesScene.Instance();
        sparkles.Position = position;
        sparkles.ZIndex = 10;
        GetParent().AddChild(sparkles);
    }

    private void _On_Area_Entered(Area2D area) {
        if (area.IsInGroup("rocks") || area.IsInGroup("enemies") || area.IsInGroup("player")) {
            _TriggerSparkles(Position);

            var hittable = (IHittable)area;
            hittable.Hit();

            if (bulletType == BulletType.Bomb) {
                // Explode
                bombTimer.Stop();
                _TriggerWave(Position);
            }
        }

        QueueFree();
    }

    private void _On_VisibilityNotifier2D_ScreenExited() {
        QueueFree();
    }

    private void _On_BombTimer_Timeout() {
        _TriggerSparkles(Position);
        _TriggerWave(Position);
        QueueFree();
    }
}
