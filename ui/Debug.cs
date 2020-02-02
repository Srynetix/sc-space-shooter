using Godot;

public class Debug : CanvasLayer
{
    private const int MAX_TAP_COUNT = 10;
    private const float TAP_TIME_LIMIT_MS = 500;

    [BindNode("Margin/VBox/Stats")] private Label stats;
    [BindNode("Margin/VBox/Console")] private Console console;
    [BindNode] private TouchController touchController;
    [BindNodeRoot] private GameState gameState;

    private uint lastTime;
    private int tapCount;
    private bool enabled;

    public static Debug GetInstance(Node origin) {
        return (Debug)origin.GetTree().Root.GetNode("Debug");
    }

    public override void _Ready() {
        this.BindNodes();

        lastTime = OS.GetTicksMsec();
        tapCount = 0;

        DisableDebugMode();
    }

    public override void _Process(float delta) {
        if (enabled) {
            _ShowDebugData(delta);
        }
    }

    public Console.Logger GetLogger(string name) {
        return console.GetLogger(name);
    }

    public override void _Input(InputEvent @event) {
        if (@event is InputEventScreenTouch touchEvent) {
            // Register tap
            if (touchEvent.Pressed) {
                var time = OS.GetTicksMsec();
                var elapsed = time - lastTime;
                lastTime = time;

                if (elapsed < TAP_TIME_LIMIT_MS) {
                    tapCount += 1;
                } else {
                    tapCount = 1;
                }

                if (tapCount == MAX_TAP_COUNT) {
                    // Toggle
                    if (enabled) {
                        DisableDebugMode();
                    } else {
                        EnableDebugMode();
                    }

                    tapCount = 0;
                }
            }
        }
    }

    private void _ShowDebugData(float delta) {
        stats.Text = _GenerateDebugData(delta);
    }

    public void DisableDebugMode() {
        enabled = false;
        stats.Visible = false;
        console.Visible = false;
    }

    public void EnableDebugMode() {
        enabled = true;
        stats.Visible = true;
        console.Visible = true;
    }

    private string _GenerateDebugData(float delta) {
        string data = "";

        data += "[Perf]";
        data += "\nResolution: " + gameState.GetGameSize().ToString();
        data += "\nFPS: " + Mathf.RoundToInt(1.0f / delta);
        data += "\n";
        data += "\n[Input]";
        data += "\nTouch position: " + touchController.LastTouchPosition.ToString();
        data += "\nIs touching: " + touchController.Touching.ToString();
        data += "\nIs double touching: " + touchController.DoubleTouching.ToString();
        data += "\nTouch distance: " + touchController.TouchDistance.ToString();
        data += "\n";
        data += "\n[Debug]";
        data += "\nLog level: " + console.LogLevelFilter;
        data += "\n";
        data += "\n[Game]";
        data += "\nPlayer position: ";
        var player = _GetPlayer();
        if (player != null) {
            data += player.Position.ToString();
        } else {
            data += "N/A";
        }

        return data;
    }

    private Player _GetPlayer() {
        var players = GetTree().GetNodesInGroup("player");
        if (players.Count > 0) {
            return players[0] as Player;
        }

        return null;
    }
}
