using Godot;
using Godot.Collections;
using System;

using Array = Godot.Collections.Array;

public class GameState : Node {
    public enum Screens {
        BOOT,
        TITLE,
        TUTORIAL,
        GAME,
        GAMEOVER,
        SCORE,
        TESTS,
    }

    private const int MAX_HIGH_SCORES = 5;
    private const string YOUR_NAME = "YOU";
    private const string VERSION_KEY = "version";
    private const string TUTORIAL_SHOWN_KEY = "tutorial_shown";
    private const string HIGH_SCORES_KEY = "high_scores";

    private Array DEFAULT_HIGH_SCORES = new Array {
        new Array { "ZZZ", 30000 },
        new Array { "AAA", 10000 },
        new Array { "BBB", 7000 }
    };
    private Dictionary<Screens, string> SCREEN_MAP = new Dictionary<Screens, string> {
        {Screens.BOOT, "res://nodes/screens/BootScreen.tscn"},
        {Screens.TITLE, "res://nodes/screens/TitleScreen.tscn"},
        {Screens.TUTORIAL, "res://nodes/screens/HowToPlayScreen.tscn"},
        {Screens.GAME, "res://nodes/screens/GameScreen.tscn"},
        {Screens.GAMEOVER, "res://nodes/screens/GameOverScreen.tscn"},
        {Screens.SCORE, "res://nodes/screens/ScoreScreen.tscn"},
        {Screens.TESTS, "res://tests/TestSuite.tscn"},
    };

    private int score = 0;
    private int lives = 3;
    private Array highScores = null;
    private Dictionary currentGameSave = null;
    private Dictionary configuration = new Dictionary();

    public static GameState GetInstance(Node origin) {
        return (GameState)origin.GetTree().Root.GetNode("GameState");
    }

    public void LoadScreen(Screens screen) {
        _ChangeScene(SCREEN_MAP[screen]);
    }

    public void AddScore(int value) {
        score += value;
    }

    public void RemoveLife() {
        lives = Math.Max(lives - 1, 0);
    }

    public void AddLife() {
        lives += 1;
    }

    public int GetLives() {
        return lives;
    }

    public void ResetGameState() {
        lives = 3;
        score = 0;
    }

    public void UpdateHUD(HUD hud) {
        Array highScore = GetHighScore();
        hud.UpdateScore(score);
        hud.UpdateLives(lives);
        hud.UpdateHighScore((string)highScore[0], (int)highScore[1]);
    }

    public Array GetHighScore() {
        var highScores = GetHighScores();
        var firstScore = (Array)highScores[0];

        if (score > (int)firstScore[1]) {
            return new Array { YOUR_NAME, score };
        } else {
            return firstScore;
        }
    }

    public Array GetHighScores() {
        if (highScores == null) {
            return DEFAULT_HIGH_SCORES;
        } else {
            return highScores;
        }
    }

    public bool WasTutorialShown() {
        if (currentGameSave.Contains(TUTORIAL_SHOWN_KEY)) {
            return (bool)currentGameSave[TUTORIAL_SHOWN_KEY];
        }

        // Key not present, first time
        return true;
    }

    public void SetTutorialShown() {
        currentGameSave[TUTORIAL_SHOWN_KEY] = false;
        _SaveGameSave(currentGameSave);
    }

    public string GetVersionNumber() {
        return (string)configuration[VERSION_KEY];
    }

    public Vector2 GetGameSize() {
        return GetViewport().Size;
    }

    public void SaveGameOver() {
        if (_HasHighScore()) {
            int idx = _GetHighScorePos();
            _InsertHighScore(idx);
            currentGameSave["high_scores"] = highScores;
            _SaveGameSave(currentGameSave);
        }
    }

    public Dictionary LoadGameSave() {
        var file = new File();
        if (!file.FileExists("user://save.dat")) {
            return _LoadEmptyGameSave();
        }

        var gameSave = new Dictionary();
        file.Open("user://save.dat", File.ModeFlags.Read);
        while (!file.EofReached()) {
            var currentLine = (Dictionary)JSON.Parse(file.GetLine()).Result;
            if (currentLine == null)
                continue;

            gameSave = currentLine;

            // Handle game save
            var loadedScores = (Array)gameSave[HIGH_SCORES_KEY];
            var newScores = new Array();
            foreach (Array entry in loadedScores) {
                newScores.Add(new Array { (string)entry[0], (int)(float)entry[1] });
            }
            gameSave[HIGH_SCORES_KEY] = newScores;

            break;
        }
        file.Close();

        if (gameSave.Count == 0) {
            gameSave = _LoadEmptyGameSave();
        }

        return gameSave;
    }

    private void _SaveGameSave(Dictionary gameSave) {
        File file = new File();
        file.Open("user://save.dat", File.ModeFlags.Write);
        file.StoreLine(JSON.Print(gameSave));
        file.Close();
    }

    private void _ApplyGameSave(Dictionary gameSave) {
        highScores = (Array)gameSave[HIGH_SCORES_KEY];
        currentGameSave = gameSave;
    }


    private Dictionary _LoadEmptyGameSave() {
        return new Dictionary {
            {HIGH_SCORES_KEY, DEFAULT_HIGH_SCORES}
        };
    }

    private void _ChangeScene(string path, float transitionSpeed = 1.0f) {
        var transition = Transition.GetInstance(this);
        transition.FadeToScene(path, transitionSpeed);
    }

    private int _GetHighScorePos() {
        int idx = 0;
        foreach (Array entry in highScores) {
            int highScore = (int)entry[1];
            if (score > highScore) {
                return idx;
            }

            idx += 1;
        }

        return -1;
    }

    private bool _HasHighScore() {
        int idx = _GetHighScorePos();
        if (idx != -1) {
            return true;
        }

        return highScores.Count < MAX_HIGH_SCORES;
    }

    private void _InsertHighScore(int idx) {
        if (idx == -1) {
            if (highScores.Count < MAX_HIGH_SCORES) {
                highScores.Add(new Array { YOUR_NAME, score });
            }
        } else {
            highScores.Insert(idx, new Array { YOUR_NAME, score });
            if (highScores.Count > MAX_HIGH_SCORES) {
                highScores.RemoveAt(highScores.Count - 1);
            }
        }
    }

    private void _LoadConfig() {
        File file = new File();
        file.Open("res://data/config.json", File.ModeFlags.Read);
        configuration = (Dictionary)JSON.Parse(file.GetAsText()).Result;
        file.Close();
    }

    public override void _Ready() {
        GD.Randomize();
        _LoadConfig();

        Dictionary gameSave = LoadGameSave();
        _ApplyGameSave(gameSave);
    }
}
