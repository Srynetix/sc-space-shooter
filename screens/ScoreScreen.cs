using Godot;

using Array = Godot.Collections.Array;

public class ScoreScreen : Control {

    [BindNode("MarginTop/MarginContainer/Scores")]
    private Label scores;
    [BindNodeRoot]
    private GameState gameState;

    async public override void _Ready() {
        this.BindNodes();

        var highScores = gameState.GetHighScores();
        var highScoresStr = "";

        foreach (Array entry in highScores) {
            var name = (string)entry[0];
            var score = (int)entry[1];
            highScoresStr += $"{name} {score}\n";
        }

        scores.Text = highScoresStr;

        await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
        gameState.LoadScreen(GameState.Screens.TITLE);
    }
}
