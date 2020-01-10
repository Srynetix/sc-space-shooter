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
        SCORE
    }
    
    private const int MAX_HIGH_SCORES = 5;
    private const String YOUR_NAME = "YOU";
    
    private Array DEFAULT_HIGH_SCORES = new Array {
        new Array { "ZZZ", 30000 },
        new Array { "AAA", 10000 },
        new Array { "BBB", 7000 }
    };
    private Dictionary<Screens, String> SCREEN_MAP = new Dictionary<Screens, String> {
        {Screens.BOOT, "res://screens/boot_screen/BootScreen.tscn"},
        {Screens.TITLE, "res://screens/title_screen/TitleScreen.tscn"},
        {Screens.GAME, "res://screens/game_screen/GameScreen.tscn"},
        {Screens.GAMEOVER, "res://screens/game_over_screen/GameOverScreen.tscn"},
        {Screens.SCORE, "res://screens/score_screen/ScoreScreen.tscn"},
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
        hud.UpdateHighScore((String)highScore[0], (int)(Single)highScore[1]);
    }
    
    public Array GetHighScore() {
        var highScores = GetHighScores();
        var firstScore = (Array)highScores[0];
        
        if (score > (int)(Single)firstScore[1]) {
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
    
    public void SaveGameOver() {
        if (_HasHighScore()) {
            int idx = _GetHighScorePos();
            _InsertHighScore(idx);
            currentGameSave["high_scores"] = highScores;
            _SaveGameSave(currentGameSave);
        }
    }
    
    private Dictionary _LoadGameSave() {
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
        file.StoreLine(gameSave.ToString());
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
    
    private void _ChangeScene(String path, float transitionSpeed = 1.0f) {
        var transition = GetTree().Root.GetNode<Transition>("Transition");
        transition.FadeToScene(path, transitionSpeed);
    }
    
    private int _GetHighScorePos() {
        int idx = 0;
        foreach (Array entry in highScores) {
            int highScore = (int)(Single)entry[1];
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
        
        Dictionary gameSave = _LoadGameSave();
        _ApplyGameSave(gameSave);
    }
}
