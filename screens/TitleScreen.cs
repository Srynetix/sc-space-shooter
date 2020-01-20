using Godot;

public class TitleScreen : Control
{
    private GameState gameState;
    private Label instructions;
    private AnimationPlayer animationPlayer;
    private Label highScore;
    private AudioStreamPlayer sound;
    private Label version;
    
    private bool instructionsLoaded = false;
    
    async public override void _Ready()
    {        
        // On ready
        instructions = GetNode<Label>("Instructions");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        highScore = GetNode<Label>("VBoxContainer/HSValue");
        sound = GetNode<AudioStreamPlayer>("Sound");
        version = GetNode<Label>("MarginContainer/Version");
        gameState = GetTree().Root.GetNode<GameState>("GameState");
        
        if (Utils.IsMobilePlatform()) {
            instructions.Text = Tr("TITLE_START_MOBILE");
        } else {
            instructions.Text = Tr("TITLE_START_DESKTOP");
        }
        
        version.Text = Tr("VERSION") + " " + gameState.GetVersionNumber();
        
        var highScoreEntry = gameState.GetHighScore();
        highScore.Text = $"{highScoreEntry[0]} {highScoreEntry[1]}";
        
        animationPlayer.Play("title");
        await ToSignal(animationPlayer, "animation_finished");
        animationPlayer.Play("instructions");
        instructionsLoaded = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (!instructionsLoaded) {
            return;
        }    
        
        if (Input.IsActionJustPressed("player_shoot")) {
            _LoadNext();
        }
    }
    
    public override void _Input(InputEvent @event) {
        if (!instructionsLoaded) {
            return;
        }
        
        if (@event is InputEventScreenTouch inputTouch) {
            _LoadNext();
        }
    }
    
    public override void _Notification(int what) {
        if (what == MainLoop.NotificationWmGoBackRequest) {
            GetTree().Quit();
        }
    }
    
    private void _LoadNext() {
        sound.Play();
        SetProcess(false);
        SetProcessInput(false);
        gameState.LoadScreen(GameState.Screens.GAME);
    }
}
