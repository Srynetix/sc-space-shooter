using Godot;

public class Debug : CanvasLayer
{
    private const int MAX_TAP_COUNT = 10;
    private const float TAP_TIME_LIMIT_MS = 500;
    
    private Label label;
    private GameState gameState;
    private Vector2 touchPosition;
    private bool isTouching;
    private Vector2 dragPosition;
    private Vector2 dragRelative;
    private uint lastTime;
    private int tapCount;
    private bool enabled;
    
    public override void _Ready() {
        // On ready
        label = GetNode<Label>("Margin/VBox/Label");    
        gameState = GetTree().Root.GetNode<GameState>("GameState");
        touchPosition = new Vector2();
        dragPosition = new Vector2();
        dragRelative = new Vector2();
        
        lastTime = OS.GetTicksMsec();
        tapCount = 0;
        _DisableDebugMode();
    }
    
    public override void _Process(float delta) {
        if (enabled) {
            _ShowDebugData(delta);
        }
    }
    
    public override void _Input(InputEvent @event) {
        if (@event is InputEventScreenTouch touchEvent) {
            touchPosition = touchEvent.Position;
            isTouching = touchEvent.Pressed;
            
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
                        _DisableDebugMode();
                    } else {
                        _EnableDebugMode();
                    }

                    tapCount = 0;
                }
            }
        }
        
        else if (@event is InputEventScreenDrag dragEvent) {
            dragPosition = dragEvent.Position;
            dragRelative = dragEvent.Relative;
        }
    }
    
    private void _ShowDebugData(float delta) {
        label.Text = _GenerateDebugData(delta);
    }
    
    private void _DisableDebugMode() {
        enabled = false;
        label.Text = "";
    }
    
    private void _EnableDebugMode() {
        enabled = true;
    }
    
    private string _GenerateDebugData(float delta) {
        string data = "";
        
        data += "Resolution: " + gameState.GetGameSize().ToString(); 
        data += "\nFPS: " + Mathf.RoundToInt(1.0f / delta);
        data += "\n";
        data += "\nTouch position: " + touchPosition.ToString();
        data += "\nIs touching: " + isTouching.ToString();
        data += "\nDrag position: " + dragPosition.ToString();
        data += "\nDrag relative: " + dragRelative.ToString();
        data += "\n";
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
