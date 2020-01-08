using Godot;

public class BootScreen : Control
{
    async public override void _Ready()
    {
        GameState gameState = GetTree().Root.GetNode<GameState>("GameState");
        await ToSignal(GetTree().CreateTimer(1), "timeout");
        gameState.LoadScreen(GameState.Screens.TITLE);   
    }
}
