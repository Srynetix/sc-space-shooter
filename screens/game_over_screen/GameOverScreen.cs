using Godot;

public class GameOverScreen : Control
{
    async public override void _Ready()
    {
        GameState gameState = GetTree().Root.GetNode<GameState>("GameState");
        gameState.SaveGameOver();
        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
        gameState.LoadScreen(GameState.Screens.SCORE);      
    }
}
