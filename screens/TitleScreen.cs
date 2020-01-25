using Godot;

public class TitleScreen : Control
{
    private GameState gameState;
    private Label highScore;
    private Button startGameButton;
    private Button optionsButton;
    private Button testsButton;
    private AudioStreamPlayer sound;
    private Label version;
    private Tween tween;

    async public override void _Ready() {
        // On ready
        highScore = GetNode<Label>("Margin/All/Top/HSValue");
        sound = GetNode<AudioStreamPlayer>("Sound");
        startGameButton = GetNode<Button>("Margin/All/Buttons/StartGame");
        optionsButton = GetNode<Button>("Margin/All/Buttons/Options");
        testsButton = GetNode<Button>("Margin/All/Buttons/Tests");
        version = GetNode<Label>("Margin/All/Bottom/Version");
        tween = GetNode<Tween>("Tween");
        gameState = GetTree().Root.GetNode<GameState>("GameState");

        var title = GetNode<Label>("Margin/All/Margin/Title");
        var buttons = GetNode<VBoxContainer>("Margin/All/Buttons");

        version.Text = Tr("VERSION") + " " + gameState.GetVersionNumber();

        var highScoreEntry = gameState.GetHighScore();
        highScore.Text = $"{highScoreEntry[0]} {highScoreEntry[1]}";

        // Hide title and buttons
        title.Modulate = Colors.Transparent;
        buttons.Modulate = Colors.Transparent;

        tween.InterpolateProperty(title, "modulate", null, Colors.White, 2.0f);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");

        tween.InterpolateProperty(buttons, "modulate", null, Colors.White, 2.0f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();

        _ConnectButtons();
        await ToSignal(tween, "tween_all_completed");
    }

    private void _ConnectButtons() {
        startGameButton.Connect("pressed", this, nameof(_LoadNext));
        optionsButton.Connect("pressed", this, nameof(_LoadOptions));
        testsButton.Connect("pressed", this, nameof(_LoadTests));
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

    private void _LoadOptions() {

    }

    private void _LoadTests() {
        sound.Play();
        SetProcess(false);
        SetProcessInput(false);
        gameState.LoadScreen(GameState.Screens.TESTS);
    }
}
