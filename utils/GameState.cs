using Godot;
using Godot.Collections;
using System;

using Array = Godot.Collections.Array;

public class GameState : Node
{
    public enum Screens {
        BOOT,
        TITLE,
        GAME,
        GAMEOVER,
        SCORE,
        TESTS,
    }

    private const int MAX_HIGH_SCORES = 5;
    private const string YOUR_NAME = "YOU";

    private Array DEFAULT_HIGH_SCORES = new Array {
        new Array { "ZZZ", 30000 },
        new Array { "AAA", 10000 },
        new Array { "BBB", 7000 }
    };
    private Dictionary<Screens, string> SCREEN_MAP = new Dictionary<Screens, string> {
        {Screens.BOOT, "res://screens/BootScreen.tscn"},
        {Screens.TITLE, "res://screens/TitleScreen.tscn"},
        {Screens.GAME, "res://screens/GameScreen.tscn"},
        {Screens.GAMEOVER, "res://screens/GameOverScreen.tscn"},
        {Screens.SCORE, "res://screens/ScoreScreen.tscn"},
        {Screens.TESTS, "res://tests/TestSuite.tscn"},
    };

    private int score = 0;
    private int lives = 3;
    private Array highScores = null;
    private Dictionary currentGameSave = null;
    private Dictionary configuration = new Dictionary();

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

    public string GetVersionNumber() {
        return (string)configuration["version"];
    }

    public Vector2 GetGameSize() {
        return GetViewport().Size;
    }

    public void SaveGameOver() {
        GD.Print("Trying to save at game over");

        if (_HasHighScore()) {
            int idx = _GetHighScorePos();
            GD.Print("High score found at position", idx);
            _InsertHighScore(idx);
            currentGameSave["high_scores"] = highScores;
            _SaveGameSave(currentGameSave);
        } else {
            GD.Print("No new high score found");
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
            var loadedScores = (Array)gameSave["high_scores"];
            var newScores = new Array();
            foreach (Array entry in loadedScores) {
                newScores.Add(new Array { (string)entry[0], (int)(float)entry[1] } );
            }
            gameSave["high_scores"] = newScores;

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
        highScores = (Array)gameSave["high_scores"];
        currentGameSave = gameSave;
    }


    private Dictionary _LoadEmptyGameSave() {
        return new Dictionary {
            {"high_scores", DEFAULT_HIGH_SCORES}
        };
    }

    private void _ChangeScene(string path, float transitionSpeed = 1.0f) {
        var transition = GetTree().Root.GetNode<Transition>("Transition");
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
