using Godot;
using System;

using Array = Godot.Collections.Array;

public class ScoreScreen : Control
{
    private Label scores;

    async public override void _Ready()
    {
        // On ready
        scores = GetNode<Label>("MarginTop/MarginContainer/Scores");
        var gameState = GetTree().Root.GetNode<GameState>("GameState");
        
        var highScores = gameState.GetHighScores();
        var highScoresStr = "";
        
        foreach (Array entry in highScores) {
            var name = (string)entry[0];
            var score = (int)entry[1];
            highScoresStr += $"{name} {score}\n";
        }
        
        scores.Text = highScoresStr;
        
        await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
        gameState.LoadScreen(GameState.Screens.TITLE);
    }
}
