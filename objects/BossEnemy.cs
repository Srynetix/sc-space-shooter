using Godot;

public class BossEnemy : Enemy
{
    // Constants
    public const int BOSS_BASE_HIT_POINTS = 50;
    public const float BOSS_BASE_FIRE_TIME = 0.5f;

    [BindNode("WeaponSwap")]
    private Timer weaponSwap;

    // Data
    private Vector2 velocity;

    public override void _Ready() {
        base._Ready();
        this.BindNodes();

        fireTimer.WaitTime = BOSS_BASE_FIRE_TIME;
        fireTimer.Start();

        weaponSwap.Connect("timeout", this, nameof(_On_WeaponSwap_Timeout));
        weaponSwap.Start();

        SetFireTimeFactor(0.0f);
        SetHitPointsFactor(0.0f);
    }

    public override void Prepare(Vector2 pos, float speed, float scale) {
        base.Prepare(pos, speed, scale);

        Scale = new Vector2(scale * 2, scale * 2);
        moveSpeed *= 1.5f;
        velocity = new Vector2(moveSpeed, downSpeed);
    }

    protected override int _CalculateBaseHitPoints() {
        return BOSS_BASE_HIT_POINTS;
    }

    protected override float _CalculateBaseFireTime() {
        return BOSS_BASE_FIRE_TIME;
    }

    protected override void _Move(float delta) {
        Position += velocity * delta * new Vector2(Mathf.Cos(acc), 1);

        velocity.y *= 0.992f;
        if (velocity.y <= 0.01f) {
            velocity.y = 0;
        }
    }

    private void _On_WeaponSwap_Timeout() {
        bulletSystem.SwitchRandomType();
    }
}
