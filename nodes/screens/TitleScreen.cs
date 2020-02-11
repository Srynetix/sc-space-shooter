using Godot;

public class TitleScreen : Control {

    [BindNode("Margin/All/Top/HSValue")] private Label highScore;
    [BindNode("Margin/All/Buttons")] private VBoxContainer buttons;
    [BindNode("Margin/All/Buttons/StartGame")] private Button startGameButton;
    [BindNode("Margin/All/Buttons/Options")] private Button optionsButton;
    [BindNode("Margin/All/Buttons/Tests")] private Button testsButton;
    [BindNode("Margin/All/OptionsButtons")] private VBoxContainer optionsButtons;
    [BindNode("Margin/All/OptionsButtons/BackButton")] private Button optionsBackButton;
    [BindNode("Margin/All/OptionsButtons/HowToPlay")] private Button howToPlayButton;
    [BindNode("Margin/All/OptionsButtons/Languages")] private OptionButton languagesButton;
    [BindNode("Sound")] private AudioStreamPlayer sound;
    [BindNode("Margin/All/Bottom/Version")] private Label version;
    [BindNode] private Tween tween;
    [BindNodeRoot] private GameState gameState;

    async public override void _Ready() {
        this.BindNodes();

        version.Text = Tr("TITLE_VERSION") + " " + gameState.GetVersionNumber();

        var highScoreEntry = gameState.GetHighScore();
        highScore.Text = $"{highScoreEntry[0]} {highScoreEntry[1]}";

        // Define language button
        var locale = OS.GetLocale();
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

    private void _DisableButtons() {
        startGameButton.MouseFilter = MouseFilterEnum.Ignore;
        testsButton.MouseFilter = MouseFilterEnum.Ignore;
        optionsButton.MouseFilter = MouseFilterEnum.Ignore;
        optionsBackButton.MouseFilter = MouseFilterEnum.Ignore;
        howToPlayButton.MouseFilter = MouseFilterEnum.Ignore;
    }

    private void _EnableButtons() {
        startGameButton.MouseFilter = MouseFilterEnum.Pass;
        testsButton.MouseFilter = MouseFilterEnum.Pass;
        optionsButton.MouseFilter = MouseFilterEnum.Pass;
        optionsBackButton.MouseFilter = MouseFilterEnum.Pass;
        howToPlayButton.MouseFilter = MouseFilterEnum.Pass;
    }

    private void _ConnectButtons() {
        startGameButton.Connect("pressed", this, nameof(_LoadNext));
        testsButton.Connect("pressed", this, nameof(_LoadTests));
        optionsButton.Connect("pressed", this, nameof(_ShowOptions));
        optionsBackButton.Connect("pressed", this, nameof(_HideOptions));
        languagesButton.Connect("item_selected", this, nameof(_ChangeLanguage));
        howToPlayButton.Connect("pressed", this, nameof(_LoadTutorial));
    }

    public override void _Notification(int what) {
        if (what == MainLoop.NotificationWmGoBackRequest) {
            GetTree().Quit();
        }
    }

    async private void _ShowOptions() {
        _DisableButtons();

        tween.InterpolateProperty(buttons, "modulate", null, Colors.Transparent, 1.0f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");

        buttons.Hide();
        optionsButtons.Show();

        optionsButtons.Modulate = Colors.Transparent;
        tween.InterpolateProperty(optionsButtons, "modulate", null, Colors.White, 1.0f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");

        _EnableButtons();
    }

    async private void _HideOptions() {
        _DisableButtons();

        tween.InterpolateProperty(optionsButtons, "modulate", null, Colors.Transparent, 1.0f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");

        optionsButtons.Hide();
        buttons.Show();

        buttons.Modulate = Colors.Transparent;
        tween.InterpolateProperty(buttons, "modulate", null, Colors.White, 1.0f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");

        _EnableButtons();
    }

    private void _LoadNext() {
        if (gameState.WasTutorialShown()) {
            _LoadScreen(GameState.Screens.TUTORIAL);
        } else {
            _LoadScreen(GameState.Screens.GAME);
        }
    }

    private void _LoadScreen(GameState.Screens screen) {
        _DisableButtons();
        sound.Play();
        SetProcess(false);
        SetProcessInput(false);
        gameState.LoadScreen(screen);
    }

    private void _LoadTutorial() {
        _LoadScreen(GameState.Screens.TUTORIAL);
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
        _LoadScreen(GameState.Screens.TESTS);
    }
}
