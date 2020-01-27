using Godot;

public class TitleScreen : Control
{
    private GameState gameState;
    private Label highScore;
    private Button startGameButton;
    private Button optionsButton;
    private VBoxContainer buttons;
    private VBoxContainer optionsButtons;
    private Button optionsBackButton;
    private OptionButton languagesButton;
    private Button testsButton;
    private AudioStreamPlayer sound;
    private Label version;
    private Tween tween;

    async public override void _Ready() {
        // On ready
        sound = GetNode<AudioStreamPlayer>("Sound");
        tween = GetNode<Tween>("Tween");
        highScore = GetNode<Label>("Margin/All/Top/HSValue");
        buttons = GetNode<VBoxContainer>("Margin/All/Buttons");
        startGameButton = GetNode<Button>("Margin/All/Buttons/StartGame");
        optionsButton = GetNode<Button>("Margin/All/Buttons/Options");
        optionsButtons = GetNode<VBoxContainer>("Margin/All/OptionsButtons");
        optionsBackButton = optionsButtons.GetNode<Button>("BackButton");
        languagesButton = optionsButtons.GetNode<OptionButton>("Languages");
        testsButton = GetNode<Button>("Margin/All/Buttons/Tests");
        buttons = GetNode<VBoxContainer>("Margin/All/Buttons");
        version = GetNode<Label>("Margin/All/Bottom/Version");
        gameState = GetTree().Root.GetNode<GameState>("GameState");

        version.Text = Tr("VERSION") + " " + gameState.GetVersionNumber();

        var highScoreEntry = gameState.GetHighScore();
        highScore.Text = $"{highScoreEntry[0]} {highScoreEntry[1]}";

        // Define language button
        var locale = OS.GetLocale();
        GD.Print(locale);

        if (locale.BeginsWith("fr")) {
            languagesButton.Select(1);
        } else {
            languagesButton.Select(0);
        }

        // Hide title and buttons
        var title = GetNode<Label>("Margin/All/Margin/Title");
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
        testsButton.Connect("pressed", this, nameof(_LoadTests));
        optionsButton.Connect("pressed", this, nameof(_ShowOptions));
        optionsBackButton.Connect("pressed", this, nameof(_HideOptions));
        languagesButton.Connect("item_selected", this, nameof(_ChangeLanguage));
    }

    public override void _Notification(int what) {
        if (what == MainLoop.NotificationWmGoBackRequest) {
            GetTree().Quit();
        }
    }

    async private void _ShowOptions() {
        tween.InterpolateProperty(buttons, "modulate", null, Colors.Transparent, 1.0f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");

        buttons.Hide();
        optionsButtons.Show();

        optionsButtons.Modulate = Colors.Transparent;
        tween.InterpolateProperty(optionsButtons, "modulate", null, Colors.White, 1.0f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");
    }

    async private void _HideOptions() {
        tween.InterpolateProperty(optionsButtons, "modulate", null, Colors.Transparent, 1.0f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");

        optionsButtons.Hide();
        buttons.Show();

        buttons.Modulate = Colors.Transparent;
        tween.InterpolateProperty(buttons, "modulate", null, Colors.White, 1.0f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");
    }

    private void _LoadNext() {
        sound.Play();
        SetProcess(false);
        SetProcessInput(false);
        gameState.LoadScreen(GameState.Screens.GAME);
    }

    private void _ChangeLanguage(int selected) {
        if (selected == 0) {
            // English
            TranslationServer.SetLocale("en");
        } else if (selected == 1) {
            // French
            TranslationServer.SetLocale("fr");
        }
    }

    private void _LoadTests() {
        sound.Play();
        SetProcess(false);
        SetProcessInput(false);
        gameState.LoadScreen(GameState.Screens.TESTS);
    }
}
