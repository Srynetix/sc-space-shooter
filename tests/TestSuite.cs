using Godot;

public class TestSuite : Control {
    [BindNodeRoot]
    private FXCamera fXCamera;
    [BindNodeRoot]
    private GameState gameState;

    public override void _Ready() {
        this.BindNodes();

        var runner = GetNode("SceneRunner");
        runner.Connect("scene_loaded", this, nameof(_On_SceneLoaded));
        runner.Connect("go_back", this, nameof(_On_GoBack));
    }

    private void _On_SceneLoaded(string name) {
        fXCamera.Reset();
    }

    private void _On_GoBack() {
        gameState.LoadScreen(GameState.Screens.TITLE);
    }
}
