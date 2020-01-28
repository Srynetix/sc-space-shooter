using Godot;

using Array = Godot.Collections.Array;

public class TestGameState : Control
{
    [BindNodeRoot]
    private GameState gameState;

    public override void _Ready() {
        this.BindNodes();

        Array firstHighScore = (Array)gameState.GetHighScores()[0];
        gameState.AddScore((int)firstHighScore[1] + 100);
    }
}
