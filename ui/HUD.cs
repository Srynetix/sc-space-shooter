using Godot;
using System;

public class HUD : Control
{
    [BindNode("MarginContainer/HBoxContainer/VBoxContainer/ScoreValue")]
    private Label scoreValue;
    [BindNode("MarginContainer/HBoxContainer/VBoxContainer2/HighValue")]
    private Label highValue;
    [BindNode("MarginContainer/HBoxContainer/VBoxContainer3/LivesValue")]
    private Label livesValue;
    [BindNode("MarginContainer/HBoxContainer2/Notification")]
    private Label notification;
    [BindNode]
    private AnimationPlayer animationPlayer;

    public override void _Ready() {
        this.BindNodes();
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
