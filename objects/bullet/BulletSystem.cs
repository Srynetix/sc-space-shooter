using Godot;

public class BulletSystem : Node2D
{
    // Signals
    [Signal] public delegate void fire(PackedScene bullet, Vector2 pos, float speed, Bullet.BulletType bulletType, Bullet.BulletTarget bulletTarget, bool automatic);
    
    // Exports
    [Export] public float fireCooldown = 0.2f;
    [Export] public float fireSpeed = 1000.0f;
    [Export] public PackedScene bulletModel = null;
    [Export] public Bullet.BulletType bulletType = Bullet.BulletType.Simple;
    [Export] public Bullet.BulletTarget bulletTarget = Bullet.BulletTarget.Enemy;
    [Export] public bool bulletAutomatic = false;
    
    // On ready
    private Timer fireTimer;
    private AudioStreamPlayer2D sound;
    
    // Data
    private bool canShoot = true;
    
    public override void _Ready() {
        // On ready
        fireTimer = GetNode<Timer>("FireTimer");
        sound = GetNode<AudioStreamPlayer2D>("Sound");    
        
        fireTimer.WaitTime = fireCooldown;
        fireTimer.Connect("timeout", this, nameof(_On_FireTimer_Timeout));
    }
    
    public void ResetWeapons() {
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
        if (bType == Bullet.BulletType.Laser) {
            fireTimer.WaitTime = fireCooldown / 4.0f;
        } else {
            fireTimer.WaitTime = fireCooldown;
        }
        
        bulletType = bType;
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
    
    public void Fire(Vector2 pos) {
        if (canShoot) {
            canShoot = false;
            
            if (bulletType == Bullet.BulletType.Simple) {
                EmitSignal("fire", bulletModel, pos, fireSpeed, bulletType, bulletTarget, bulletAutomatic);
            } else if (bulletType == Bullet.BulletType.Double) {
                EmitSignal("fire", bulletModel, pos - new Vector2(20, 0), fireSpeed, bulletType, bulletTarget, bulletAutomatic);
                EmitSignal("fire", bulletModel, pos + new Vector2(20, 0), fireSpeed, bulletType, bulletTarget, bulletAutomatic);
            } else if (bulletType == Bullet.BulletType.Triple) {
                EmitSignal("fire", bulletModel, pos - new Vector2(20, 0), fireSpeed, bulletType, bulletTarget, bulletAutomatic);
                EmitSignal("fire", bulletModel, pos - new Vector2(0, 40), fireSpeed, bulletType, bulletTarget, bulletAutomatic);
                EmitSignal("fire", bulletModel, pos + new Vector2(20, 0), fireSpeed, bulletType, bulletTarget, bulletAutomatic);
            } else if (bulletType == Bullet.BulletType.Laser) {
                EmitSignal("fire", bulletModel, pos, fireSpeed, bulletType, bulletTarget, bulletAutomatic);
            } else if (bulletType == Bullet.BulletType.SlowFast) {
                EmitSignal("fire", bulletModel, pos, fireSpeed, bulletType, bulletTarget, bulletAutomatic);
            }
            
            sound.Play();
            fireTimer.Start();
        }
    }
    
    private void _On_FireTimer_Timeout() {
        canShoot = true;
    }
}
