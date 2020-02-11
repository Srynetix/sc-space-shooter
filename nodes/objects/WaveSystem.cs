using Godot;

using Array = Godot.Collections.Array;
using Dictionary = Godot.Collections.Dictionary;

public class WaveSystem : Control {
    [Signal]
    public delegate void timeout();

    [BindNode("WaveTimer")]
    private Timer waveTimer;

    private Array waves;
    private int currentWave;

    public override void _Ready() {
        this.BindNodes();

        waves = _LoadWaveFile();
        waveTimer.Connect("timeout", this, nameof(_On_WaveTimer_Timeout));
    }

    public Dictionary LoadNextWave() {
        currentWave += 1;
        var waveInfo = _GetCurrentWaveInfo();

        waveTimer.WaitTime = (float)waveInfo["duration"];
        waveTimer.Start();

        return waveInfo;
    }

    public int GetCurrentWave() {
        return currentWave;
    }

    private Array _LoadWaveFile() {
        var file = new File();
        file.Open("res://data/waves.json", File.ModeFlags.Read);
        return (Array)JSON.Parse(file.GetAsText()).Result;
    }

    private Dictionary _GetCurrentWaveInfo() {
        if (waves.Count < currentWave) {
            return (Dictionary)waves[waves.Count - 1];
        } else {
            return (Dictionary)waves[currentWave - 1];
        }
    }

    private void _On_WaveTimer_Timeout() {
        EmitSignal("timeout");
    }
}
