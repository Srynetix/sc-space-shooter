using Godot;

public class BulletSystem : Node2D
{
    // Signals
    [Signal] public delegate void fire(Bullet.FireData fireData);
    [Signal] public delegate void type_switch(Bullet.BulletType prevType, Bullet.BulletType newType);
    [Signal] public delegate void bomb_available();
    [Signal] public delegate void bomb_used();

    // Exports
    [Export] public float fireCooldown = 0.2f;
    [Export] public float fireSpeed = 500.0f;
    [Export] public PackedScene bulletModel = null;
    [Export] public Bullet.BulletType bulletType = Bullet.BulletType.Simple;
    [Export] public Bullet.BulletTarget bulletTarget = Bullet.BulletTarget.Enemy;
    [Export] public bool bulletAutomatic = false;
    [Export] public bool bombAvailable = false;

    // On ready
    [BindNode("FireTimer")]
    private Timer fireTimer;
    [BindNode("Sound")]
    private AudioStreamPlayer2D sound;

    // Data
    private bool canShoot = true;
    private Bullet.BulletType previousBulletType = Bullet.BulletType.Simple;

    public override void _Ready() {
        this.BindNodes();

        fireTimer.WaitTime = fireCooldown;
        fireTimer.Connect("timeout", this, nameof(_On_FireTimer_Timeout));
    }

    public void EnableBomb() {
        EmitSignal("bomb_available");
        bombAvailable = true;
    }

    public void ResetWeapons() {
        previousBulletType = Bullet.BulletType.Simple;
        bombAvailable = false;
        SwitchType(Bullet.BulletType.Simple);
    }

    public void UpgradeWeapon() {
        if (bulletType == Bullet.BulletType.Simple) {
            SwitchType(Bullet.BulletType.Double);
        } else if (bulletType == Bullet.BulletType.Double) {
            SwitchType(Bullet.BulletType.Triple);
        } else if (bulletType == Bullet.BulletType.Triple) {
            SwitchType(Bullet.BulletType.Laser);
        }
    }

    public void SwitchType(Bullet.BulletType bType) {
        if (bType == bulletType) {
            return;
        }

        if (bType == Bullet.BulletType.Laser) {
            fireTimer.WaitTime = fireCooldown / 4.0f;
        } else {
            fireTimer.WaitTime = fireCooldown;
        }

        // Store previous bullet type
        previousBulletType = bulletType;
        bulletType = bType;

        EmitSignal("type_switch", previousBulletType, bulletType);
    }

    public void SwitchRandomType() {
        int weaponId = (int)GD.RandRange(0, 5);
        switch (weaponId) {
            case 0:
                SwitchType(Bullet.BulletType.Simple);
                break;
            case 1:
                SwitchType(Bullet.BulletType.Double);
                break;
            case 2:
                SwitchType(Bullet.BulletType.Triple);
                break;
            case 3:
                SwitchType(Bullet.BulletType.Laser);
                break;
            case 4:
                SwitchType(Bullet.BulletType.SlowFast);
                break;
        }
    }

    public bool CanUpgradeWeapon() {
        return bulletType != Bullet.BulletType.Laser;
    }

    public void Fire(Vector2 pos) {
        _Fire(pos, bulletType);
    }

    public void FireBomb(Vector2 pos) {
        if (bombAvailable) {
            bombAvailable = false;
            EmitSignal("bomb_used");
            _Fire(pos, Bullet.BulletType.Bomb);
            Debug.GetInstance(this).GetLogger(GetType().Name).Info("Bomb fired at", pos);
        }
    }

    public void _Fire(Vector2 pos, Bullet.BulletType bType) {
        if (canShoot) {
            canShoot = false;

            if (bType == Bullet.BulletType.Double) {
                EmitSignal("fire", new Bullet.FireData(bulletModel, pos - new Vector2(20, 0), fireSpeed, bType, bulletTarget, bulletAutomatic));
                EmitSignal("fire", new Bullet.FireData(bulletModel, pos + new Vector2(20, 0), fireSpeed, bType, bulletTarget, bulletAutomatic));
            } else if (bType == Bullet.BulletType.Triple) {
                EmitSignal("fire", new Bullet.FireData(bulletModel, pos - new Vector2(20, 0), fireSpeed, bType, bulletTarget, bulletAutomatic));
                EmitSignal("fire", new Bullet.FireData(bulletModel, pos - new Vector2(0, 40), fireSpeed, bType, bulletTarget, bulletAutomatic));
                EmitSignal("fire", new Bullet.FireData(bulletModel, pos + new Vector2(20, 0), fireSpeed, bType, bulletTarget, bulletAutomatic));
            } else {
                EmitSignal("fire", new Bullet.FireData(bulletModel, pos, fireSpeed, bType, bulletTarget, bulletAutomatic));
            }

            sound.Play();
            fireTimer.Start();
        }
    }

    private void _On_FireTimer_Timeout() {
        canShoot = true;
    }
}
