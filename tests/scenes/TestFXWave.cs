using Godot;

public class TestFXWave : Control {
    private static PackedScene fxWaveScene = (PackedScene)GD.Load("res://objects/FXWave.tscn");

    [BindNodeRoot]
    private GameState gameState;

    async public override void _Ready() {
        this.BindNodes();

        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

        while (true) {
            var wave = (FXWave)fxWaveScene.Instance();
            AddChild(wave);
            wave.Start(gameState.GetGameSize() / 2, 0.35f);

            await ToSignal(wave, "finished");
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        }
    }
}
