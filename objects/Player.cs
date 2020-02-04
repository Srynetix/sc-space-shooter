using Godot;

public class Player : Area2D {

    public enum State {
        Idle,
        Spawning,
        Dead
    }

    // Consts
    private const float MESSAGE_SPEED = 0.05f;

    // Signals
    [Signal] public delegate void dead();
    [Signal] public delegate void respawn();

    // Exports
    [Export] public Bullet.BulletType initialBulletType = Bullet.BulletType.Simple;

    // On ready
    [BindNode("Timers/SpawningTimer")]
    private Timer spawnTimer;
    [BindNode]
    private Position2D muzzle;
    [BindNode]
    private Sprite sprite;
    [BindNode]
    private AnimationPlayer animationPlayer;
    [BindNode]
    private CollisionShape2D collisionShape;
    [BindNode]
    private BulletSystem bulletSystem;
    [BindNode]
    private StatusToast statusToast;
    [BindNode] private TouchController touchController;
    [BindNodeRoot] private GameState gameState;

    // Data
    private Vector2 initialPosition = new Vector2();
    private Vector2 velocity = new Vector2();
    private State state = State.Idle;
    private Vector2 moveSpeed = new Vector2(500, 500);
    private float damping = 0.9f;
    private float spawnTime = 3.0f;

    public override void _Ready() {
        this.BindNodes();

        Connect("area_entered", this, nameof(_On_Area_Entered));

        spawnTimer.WaitTime = spawnTime;
        spawnTimer.Connect("timeout", this, nameof(_On_SpawningTimer_Timeout));
        bulletSystem.Connect("bomb_available", this, nameof(_On_BombAvailable));
        bulletSystem.Connect("bomb_used", this, nameof(_On_BombUsed));
        bulletSystem.TargetContainer = GetParent();

        var gameSize = gameState.GetGameSize();
        initialPosition = new Vector2(gameSize.x / 2.0f, gameSize.y - gameSize.y / 8.0f);
        Position = initialPosition;

        bulletSystem.SwitchType(initialBulletType);

        statusToast.ShowPriorityMessage(Tr("Let's go!"));
    }

    public override void _Process(float delta) {
        if (state == State.Dead) {
            return;
        }

        var movement = _HandleMovement();
        if (movement.x == 0 && movement.y == 0) {
            velocity *= damping;
        } else {
            velocity = movement * moveSpeed;
        }

        if (touchController.Touching) {
            velocity = new Vector2();
            Position = touchController.ComputedPosition;
        } else {
            velocity = new Vector2();
        }

        _HandleFire();

        Position += velocity * delta;
        _ClampPosition();
    }

    public void Respawn() {
        _SetState(State.Spawning);
        Position = initialPosition;
        bulletSystem.ResetWeapons();
    }

    public BulletSystem GetBulletSystem() {
        return bulletSystem;
    }

    public StatusToast GetStatusToast() {
        return statusToast;
    }

    public void Hit() {
        _SetState(State.Dead);
    }

    private void _ClampPosition() {
        var gameSize = gameState.GetGameSize();

        Position = new Vector2(
            Mathf.Clamp(Position.x, 0, gameSize.x),
            Mathf.Clamp(Position.y, 0, gameSize.y)
        );
    }

    private void _SetState(State newState) {
        if (state == newState) {
            return;
        }

        state = newState;

        switch (newState) {
            case State.Dead:
                // Reset state
                _SetStateDead();
                break;
            case State.Idle:
                _SetStateIdle();
                break;
            case State.Spawning:
                _SetStateSpawning();
                break;
        }
    }

    private void _SetStateDead() {
        velocity = new Vector2();
        touchController.ResetState();
        touchController.SetProcess(false);
        touchController.SetProcessInput(false);

        statusToast.Stop();
        collisionShape.SetDeferred("disabled", true);

        EmitSignal("dead");
        CallDeferred(nameof(_PlayExplosion));
    }

    async private void _PlayExplosion() {
        animationPlayer.Play("explode");
        await ToSignal(animationPlayer, "animation_finished");

        EmitSignal("respawn");
    }

    private void _SetStateIdle() {
        collisionShape.SetDeferred("disabled", false);
        animationPlayer.Play("idle");
        statusToast.ShowPriorityMessage(Tr("Let's go!"));
    }

    private void _SetStateSpawning() {
        touchController.SetProcess(true);
        touchController.SetProcessInput(true);
        spawnTimer.Start();
        animationPlayer.Play("spawning");
    }

    public bool CanUpgradeWeapon() {
        return bulletSystem.CanUpgradeWeapon();
    }

    public void UpgradeWeapon() {
        bulletSystem.UpgradeWeapon();
        statusToast.ShowMessage(Tr("Weapon upgrade!"));
    }

    private Vector2 _HandleMovement() {
        var movement = new Vector2();

        if (Input.IsActionPressed("player_left")) {
            movement.x -= 1;
        }
        if (Input.IsActionPressed("player_right")) {
            movement.x += 1;
        }
        if (Input.IsActionPressed("player_up")) {
            movement.y -= 1;
        }
        if (Input.IsActionPressed("player_down")) {
            movement.y += 1;
        }

        return movement;
    }

    private void _HandleFire() {
        if (Input.IsActionJustPressed("player_bomb") || touchController.DoubleTouching) {
            bulletSystem.FireBomb(muzzle.GlobalPosition);
        } else if (Input.IsActionPressed("player_shoot") || touchController.Touching) {
            bulletSystem.Fire(muzzle.GlobalPosition);
        }
    }

    private void _On_SpawningTimer_Timeout() {
        _SetState(State.Idle);
    }

    private void _On_Area_Entered(Area2D area) {
        if (area.IsInGroup("rocks") || area.IsInGroup("enemies") || area.IsInGroup("bullets")) {
            _SetState(State.Dead);
        }
    }

    private void _On_BombAvailable() {
        sprite.Modulate = Colors.Green;
        statusToast.ShowMessage(Tr("Bomb picked!"));
    }

    private void _On_BombUsed() {
        sprite.Modulate = Colors.White;
        statusToast.ShowMessage(Tr("Bomb fired!"));
    }
}
