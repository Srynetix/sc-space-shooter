using Godot;

public class Enemy : Area2D {
    // Signals
    [Signal] public delegate void exploded(Node2D node);

    // Constants
    public const int BASE_HIT_POINTS = 5;
    public const float BASE_FIRE_TIME = 0.5f;
    public const float BOMB_HIT_TIME = 0.1f;
    public const int BOMB_HIT_COUNT = 2;

    // Exports
    [Export] public float moveSpeed = 150.0f;
    [Export] public float downSpeed = 50.0f;
    [Export] public float fireTime = BASE_FIRE_TIME;
    [Export] public int hitPoints = BASE_HIT_POINTS;

    // On ready
    [BindNode] protected AnimationPlayer animationPlayer;
    [BindNode] protected CollisionShape2D collisionShape;
    [BindNode] protected Position2D muzzle;
    [BindNode("FireTimer")] protected Timer fireTimer;
    [BindNode] protected BulletSystem bulletSystem;
    [BindNode] protected StatusToast statusToast;
    [BindNode("ExplosionSound")] protected AudioStreamPlayer2D explosionSound;
    [BindNode] protected Sprite sprite;
    [BindNode] protected ProgressBar progressBar;
    [BindNode("BombHitTimer")] protected Timer bombHitTimer;
    [BindNodeRoot] protected GameState gameState;

    // Data
    protected int hitCount = 0;
    protected bool hasExploded = false;
    protected float acc = 0.0f;
    protected bool isFiring = false;
    protected bool showingMessage = false;
    protected bool currentlyInBomb = false;

    public override void _Ready() {
        this.BindNodes();

        Connect("area_entered", this, nameof(_On_Area_Entered));
        Connect("area_exited", this, nameof(_On_Area_Exited));

        fireTimer.WaitTime = fireTime;
        fireTimer.Connect("timeout", this, nameof(_On_FireTimer_Timeout));
        bombHitTimer.WaitTime = BOMB_HIT_TIME;
        bombHitTimer.Connect("timeout", this, nameof(_On_BombHitTimer_Timeout));
        statusToast.Connect("message_all_shown", this, nameof(_On_MessageAllShown));

        bulletSystem.TargetContainer = GetParent();

        SetFireTimeFactor(0.0f);
        SetHitPointsFactor(0.0f);

        _ScanPlayer();
    }

    public override void _Process(float delta) {
        acc += delta;

        _Move(delta);
        _HandleState();

        if (!showingMessage) {
            showingMessage = true;
            statusToast.ShowMessageWithColor(Tr("ENEMY_FIRE_MSG"), Colors.Red);
        }
    }

    public virtual void Prepare(Vector2 pos, float speed, float scale) {
        Position = pos;
        downSpeed = speed;
        Scale = new Vector2(scale, scale);
    }

    protected virtual int _CalculateBaseHitPoints() {
        return BASE_HIT_POINTS;
    }

    protected virtual float _CalculateBaseFireTime() {
        return BASE_FIRE_TIME;
    }

    public void SetHitPointsFactor(float factor) {
        hitPoints = Mathf.CeilToInt(_CalculateBaseHitPoints() + factor);
        progressBar.MaxValue = hitPoints;
        progressBar.Value = hitPoints;
    }

    public void SetFireTimeFactor(float factor) {
        fireTime = Mathf.CeilToInt(_CalculateBaseFireTime() + factor);
        fireTimer.WaitTime = fireTime;
        fireTimer.Start();
    }

    public void Hit(int count = 1) {
        if (!hasExploded) {
            animationPlayer.Play("tint");
            hitCount += count;
            _UpdateProgressBar();
            if (hitCount >= hitPoints) {
                Explode();
            }
        }
    }

    private void _UpdateProgressBar() {
        progressBar.Value = hitPoints - hitCount;
    }

    private Node2D _DetectPlayer() {
        var players = GetTree().GetNodesInGroup("player");
        if (players.Count > 0) {
            return (Node2D)players[0];
        }

        return null;
    }

    async public void Explode() {
        hasExploded = true;
        isFiring = false;
        fireTimer.Stop();

        EmitSignal(nameof(exploded), this);
        _DisableCollisions();
        statusToast.Stop();
        explosionSound.Play();
        animationPlayer.Play("explode");

        await ToSignal(animationPlayer, "animation_finished");
        QueueFree();
    }

    private void _DisableCollisions() {
        collisionShape.SetDeferred("disabled", true);
    }

    protected virtual void _Move(float delta) {
        Position += new Vector2(moveSpeed, downSpeed) * delta * new Vector2(Mathf.Cos(acc), 1);
    }

    private void _HandleState() {
        var gameSize = gameState.GetGameSize();

        if (isFiring) {
            bulletSystem.Fire(muzzle.GlobalPosition);
        }

        var spriteSize = sprite.Texture.GetSize();
        if (Position.y - spriteSize.y > gameSize.y) {
            QueueFree();
        }
    }

    private void _On_FireTimer_Timeout() {
        isFiring = !isFiring;
    }

    private void _On_BombHitTimer_Timeout() {
        if (currentlyInBomb) {
            Hit(BOMB_HIT_COUNT);
            bombHitTimer.Start();
        }
    }

    private void _On_MessageAllShown() {
        showingMessage = false;
    }

    private void _On_Area_Entered(Area2D area) {
        if (area.IsInGroup("wave")) {
            currentlyInBomb = true;
            Hit(BOMB_HIT_COUNT);
            bombHitTimer.Start();
        } else if (area.IsInGroup("bullets")) {
            Hit();
        }
    }

    private void _On_Area_Exited(Area2D area) {
        if (area.IsInGroup("wave")) {
            currentlyInBomb = false;
        }
    }

    private void _ScanPlayer() {
        var players = GetTree().GetNodesInGroup("player");
        foreach (Player player in players) {
            player.Connect("dead", this, nameof(_TauntPlayer));
        }
    }

    private void _TauntPlayer() {
        statusToast.ShowPriorityMessageWithColor(Tr("ENEMY_TAUNT_MSG"), Colors.Red);
    }
}
