using Godot;

using Array = Godot.Collections.Array;

public class TestGameState : Control
{
    private GameState gameState;
    
    public override void _Ready() {
        gameState = GetTree().Root.GetNode<GameState>("GameState");
        Array firstHighScore = (Array)gameState.GetHighScores()[0];
        gameState.AddScore((int)firstHighScore[1] + 100);
        
        // Save game
        gameState.SaveGameOver();
        
        // Reload saved game
        var gameSave2 = gameState.LoadGameSave();
        GD.Print(gameSave2);
    }
}