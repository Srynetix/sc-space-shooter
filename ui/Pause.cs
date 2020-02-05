using Godot;

public class Pause : CanvasLayer {
    [BindNode("Panel")] private PopupPanel panel;
    [BindNode("Panel/Margin/VBox/Button")] private Button button;

    public override void _Ready() {
        this.BindNodes();

        button.Connect("pressed", this, nameof(_Resume));
    }

    public override void _Notification(int what) {
        if (what == MainLoop.NotificationWmFocusOut) {
            _Pause();
        }
    }

    private void _Pause() {
        panel.CallDeferred("popup_centered");
        GetTree().Paused = true;
    }

    private void _Resume() {
        panel.Hide();
        GetTree().Paused = false;
    }
}
