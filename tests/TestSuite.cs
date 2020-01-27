using Godot;

public class TestSuite : Control {
    private FXCamera fXCamera;
    private GameState gameState;
    private Debug debug;

    public override void _Ready() {
        fXCamera = GetTree().Root.GetNode<FXCamera>("FXCamera");
        gameState = GetTree().Root.GetNode<GameState>("GameState");
        debug = GetTree().Root.GetNode<Debug>("Debug");

        var runner = GetNode("SceneRunner");
        runner.Connect("scene_loaded", this, nameof(_On_SceneLoaded));
        runner.Connect("go_back", this, nameof(_On_GoBack));
    }

    private void _On_SceneLoaded(string name) {
        fXCamera.Reset();

        if (name == "TestGameState") {
            debug.EnableDebugMode();
        } else {
            debug.DisableDebugMode();
        }
    }

    private void _On_GoBack() {
        gameState.LoadScreen(GameState.Screens.TITLE);
    }
}
