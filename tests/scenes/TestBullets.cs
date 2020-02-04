using Godot;

public class TestBullets : Node2D {

    [BindNode("PlayerBullets")]
    private BulletSystem playerBullets;
    [BindNode("EnemyBullets")]
    private BulletSystem enemyBullets;
    [BindNode("LaserBullets")]
    private BulletSystem laserBullets;
    [BindNodeRoot]
    private GameState gameState;

    async public override void _Ready() {
        this.BindNodes();

        var gameSize = gameState.GetGameSize();
        playerBullets.Position = new Vector2(gameSize.x / 4, gameSize.y / 2);
        enemyBullets.Position = new Vector2(gameSize.x / 2, gameSize.y / 2);
        laserBullets.Position = new Vector2(gameSize.x / 2 + gameSize.x / 4, gameSize.y / 2);

        while (true) {
            playerBullets.Fire(playerBullets.Position);
            enemyBullets.Fire(enemyBullets.Position);
            laserBullets.Fire(laserBullets.Position);
            await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
        }
    }
}
