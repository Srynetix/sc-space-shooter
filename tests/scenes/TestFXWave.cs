using Godot;

public class TestFXWave : Control {
    private static PackedScene fxWaveScene = (PackedScene)GD.Load("res://nodes/fx/FXWave.tscn");

    [BindNodeRoot]
    private GameState gameState;

    async public override void _Ready() {
        this.BindNodes();

        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

        while (true) {
            var wave = fxWaveScene.InstanceAs<FXWave>();
            AddChild(wave);
            wave.Start(gameState.GetGameSize() / 2, 0.35f);

            await ToSignal(wave, "finished");
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        }
    }
}
