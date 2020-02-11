using Godot;

public class BootScreen : Control {

    [BindNodeRoot]
    private GameState gameState;

    async public override void _Ready() {
        this.BindNodes();

        await ToSignal(GetTree().CreateTimer(1), "timeout");
        gameState.LoadScreen(GameState.Screens.TITLE);
    }
}
