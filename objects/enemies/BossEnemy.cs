using Godot;

public class BossEnemy : Enemy
{
    // Constants
    private const int BOSS_BASE_HIT_POINTS = 50;
    
    // On ready
    private Timer weaponSwap;
    
    // Data
    private Vector2 velocity;

    public override void _Ready() {
        // On ready
        weaponSwap = GetNode<Timer>("WeaponSwap");
        
        weaponSwap.Connect("timeout", this, nameof(_On_WeaponSwap_Timeout));
        weaponSwap.Start();        
    }
    
    public override void Prepare(Vector2 pos, float speed, float scale) {
        Position = pos;
        downSpeed = speed;
        moveSpeed *= 1.5f;
        Scale = new Vector2(scale * 2, scale * 2);
        velocity = new Vector2(moveSpeed, downSpeed);
        
        hitPoints = (int)Mathf.Ceil(scale * BOSS_BASE_HIT_POINTS);
    }
    
    protected override void _Move(float delta) {
        acc += delta;
        Position += velocity * delta * new Vector2(Mathf.Cos(acc), 1);
        
        velocity.y *= 0.992f;
        if (velocity.y <= 0.01f) {
            velocity.y = 0;
        }
        
        if (isFiring) {
            bulletSystem.Fire(muzzle.GlobalPosition);
        }
        
        if (Position.y > gameSize.y) {
            QueueFree();
        }
    }
    
    private void _On_WeaponSwap_Timeout() {
        bulletSystem.SwitchRandomType();
    }
}
