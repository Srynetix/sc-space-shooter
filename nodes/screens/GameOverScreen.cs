using Godot;

public class GameOverScreen : Control {

    [BindNodeRoot]
    private GameState gameState;

    async public override void _Ready() {
        this.BindNodes();

        gameState.SaveGameOver();
        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
        gameState.LoadScreen(GameState.Screens.SCORE);
    }
}
