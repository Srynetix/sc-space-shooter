using Godot;

public class Player : Area2D, IHittable {
    
    public enum State {
        Idle,
        Spawning,
        Dead
    }
    
    // Signals
    [Signal] public delegate void fire(PackedScene bullet, Vector2 pos, float speed, Bullet.BulletType bulletType, Bullet.BulletTarget bulletTarget, bool automatic);
    [Signal] public delegate void dead();
    [Signal] public delegate void respawn();
   
    // Exports
    [Export] public Vector2 moveSpeed = new Vector2(500, 500);
    [Export] public float damping = 0.9f;
    [Export] public float spawnTime = 3.0f;
    
    // On ready
    private Timer spawnTimer;
    private Position2D muzzle;
    private AnimationPlayer animationPlayer;
    private CollisionShape2D collisionShape;
    private BulletSystem bulletSystem;
    private Label statusLabel;
    
    // Data
    private Vector2 initialPosition = new Vector2();
    private Vector2 velocity = new Vector2();
    private State state = State.Idle;
    private bool isTouching = false;
    private Vector2 lastTouchPosition = new Vector2();
    private Vector2 touchDistance = new Vector2();
    
    public override void _Ready() {
        spawnTimer = GetNode<Timer>("Timers/SpawningTimer");
        muzzle = GetNode<Position2D>("Position2D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        bulletSystem = GetNode<BulletSystem>("BulletSystem");
        statusLabel = GetNode<Label>("Label");
        
        Connect("area_entered", this, nameof(_On_Area_Entered));
        
        spawnTimer.WaitTime = spawnTime;
        spawnTimer.Connect("timeout", this, nameof(_On_SpawningTimer_Timeout));
        bulletSystem.Connect("fire", this, nameof(_On_BulletSystem_Fire));
        statusLabel.Text = "";
        
        initialPosition = Position;
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
        
        if (isTouching) {
            velocity = new Vector2();
            Position = lastTouchPosition + touchDistance;
        }
        
        _HandleFire();
        
        Position += velocity * delta;
        _ClampPosition();
    }
    
    public override void _Input(InputEvent @event) {
        if (@event is InputEventScreenTouch touch) {
            lastTouchPosition = touch.Position;
            isTouching = touch.Pressed;
            touchDistance = Position - touch.Position;
            
            if (!isTouching) {
                velocity = new Vector2();
            }
        }
        
        else if (@event is InputEventScreenDrag drag) {
            lastTouchPosition = drag.Position;
        }
    }
    
    public void Respawn() {
        _SetState(State.Spawning);
        velocity = new Vector2();
        Position = initialPosition;
        bulletSystem.ResetWeapons();
    }
    
    public BulletSystem GetBulletSystem() {
        return bulletSystem;
    }
    
    public void Hit() {
        _SetState(State.Dead);
    }
    
    private void _ClampPosition() {
        var gameSize = Utils.GetGameSize();
        
        Position = new Vector2(
            Mathf.Clamp(Position.x, 0, gameSize.x),
            Mathf.Clamp(Position.y, 0, gameSize.y)
        );
    }
    
    async private void _SetState(State newState) {
        if (state == newState) {
            return;
        }
        
        switch (newState) {
            case State.Dead:
                collisionShape.SetDeferred("disabled", true);
                animationPlayer.Play("explode");
                EmitSignal("dead");
                
                await ToSignal(animationPlayer, "animation_finished");
                EmitSignal("respawn");
                break;
            case State.Idle:
                collisionShape.SetDeferred("disabled", false);
                animationPlayer.Play("idle");
                break;
            case State.Spawning:
                spawnTimer.Start();
                animationPlayer.Play("spawning");
                break;
        }
        
        state = newState;
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
        if (Input.IsActionPressed("player_shoot") || isTouching) {
            bulletSystem.Fire(muzzle.GlobalPosition);
        }
    }
    
    private void _On_SpawningTimer_Timeout() {
        _SetState(State.Idle);
    }
    
    private void _On_Area_Entered(Area2D area) {
        if (area.IsInGroup("rocks")) {
            ((IExplodable)area).Explode();
            _SetState(State.Dead);
        } else if (area.IsInGroup("enemies")) {
            _SetState(State.Dead);
        }
    }
    
    private void _On_BulletSystem_Fire(PackedScene bullet, Vector2 pos, float speed, Bullet.BulletType bulletType, Bullet.BulletTarget bulletTarget, bool automatic) {
        EmitSignal("fire", bullet, pos, speed, bulletType, bulletTarget, automatic);
    }
}
