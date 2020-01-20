using Godot;
using System;

public class HUD : Control
{
    // On ready
    private Label scoreValue;
    private Label highValue;
    private Label livesValue;
    private Label notification;
    private AnimationPlayer animationPlayer;
    
    public override void _Ready()
    {
        // On ready
        scoreValue = GetNode<Label>("MarginContainer/HBoxContainer/VBoxContainer/ScoreValue");
        highValue = GetNode<Label>("MarginContainer/HBoxContainer/VBoxContainer2/HighValue");
        livesValue = GetNode<Label>("MarginContainer/HBoxContainer/VBoxContainer3/LivesValue");
        notification = GetNode<Label>("MarginContainer/HBoxContainer2/Notification");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }
    
    public void UpdateLives(int lives) {
        livesValue.Text = lives.ToString();
    }
    
    public void UpdateScore(int score) {
        scoreValue.Text = score.ToString();
    }
    
    public void ShowMessage(string msg) {
        notification.Text = msg;
        animationPlayer.Stop();
        animationPlayer.Play("message");
    }
    
    public void UpdateHighScore(string name, int score) {
        highValue.Text = $"{name} {score}";
    }
}
