using Godot;

using Array = Godot.Collections.Array;
using Queue = System.Collections.Queue;

public enum StatusToastDirection {
    Up,
    Down
}

public class StatusToast : Node2D
{
    private enum AnimStep {
        None,
        FadeIn,
        Wait,
        FadeOut
    }

    [Signal] public delegate void message_shown(string message);
    [Signal] public delegate void message_all_shown();

    [BindNode] private Label label;
    [BindNode] private Timer timer;
    [BindNode] private Tween tween;

    [Export] public StatusToastDirection ToastDirection = StatusToastDirection.Up;
    [Export] public Vector2 MessageOffset = new Vector2(0.0f, -60.0f);
    [Export] public float MessageVisibleTime = 1.0f;
    [Export] public float MessageAnimSpeed = 0.5f;

    private Queue messageQueue = new Queue();
    private bool running = false;
    private Vector2 initialLabelPosition;
    private AnimStep animStep;

    public override void _Ready() {
        this.BindNodes();

        label.Text = "";

        timer.WaitTime = MessageVisibleTime;
        initialLabelPosition = label.RectPosition;
        animStep = AnimStep.None;

        timer.Connect("timeout", this, nameof(_On_TimerTimeout));
        tween.Connect("tween_all_completed", this, nameof(_On_TweenAllCompleted));
        label.RectMinSize = new Vector2(GetViewport().Size.x, 8);
    }

    public void Stop() {
        tween.ResetAll();
        timer.Stop();
        animStep = AnimStep.None;
        label.Text = "";
    }

    public void ShowMessage(string message) {
        ShowMessageWithColor(message, Colors.White);
    }

    public void ShowMessageWithColor(string message, Color color) {
        _ShowMessageWithColor(message, color, false);
    }

    public void ShowPriorityMessage(string message) {
        ShowPriorityMessageWithColor(message, Colors.White);
    }

    public void ShowPriorityMessageWithColor(string message, Color color) {
        _ShowMessageWithColor(message, color, true);
    }

    public void _ShowMessageWithColor(string message, Color color, bool priority) {
        if (running) {
            // If priority message, replace current message
            if (priority) {
                messageQueue.Clear();
                Stop();

            } else {
                messageQueue.Enqueue(new Array { message, color });
                return;
            }
        }

        _MessageStart(message, color);
    }

    private void _MessageFadeIn(string message, Color color) {
        var computedMessageOffset = ToastDirection == StatusToastDirection.Up ? MessageOffset : -MessageOffset;

        label.Modulate = Colors.Transparent;
        label.Text = message;
        label.RectPosition = new Vector2(initialLabelPosition.x - label.GetCombinedMinimumSize().x / 2.0f, initialLabelPosition.y);

        animStep = AnimStep.FadeIn;

        tween.InterpolateProperty(label, "modulate", Colors.Transparent, color, MessageAnimSpeed);
        tween.InterpolateProperty(label, "rect_position", label.RectPosition, label.RectPosition + computedMessageOffset, MessageAnimSpeed);
        tween.Start();
    }

    private void _MessageFadeOut() {
        animStep = AnimStep.FadeOut;

        tween.InterpolateProperty(label, "modulate", label.Modulate, Colors.Transparent, MessageAnimSpeed);
        tween.Start();
    }

    private void _MessageWait() {
        animStep = AnimStep.Wait;

        timer.Start();
    }

    private void _MessageStart(string message, Color color) {
        running = true;

        _MessageFadeIn(message, color);
    }

    private void _MessageEnd() {
        running = false;
        animStep = AnimStep.None;

        EmitSignal("message_shown");

        // Show more messages
        if (messageQueue.Count > 0) {
            var messageArgs = (Array)messageQueue.Dequeue();
            var newMessage = (string)messageArgs[0];
            var newColor = (Color)messageArgs[1];
            CallDeferred("_ShowMessageWithColor", newMessage, newColor, false);
        } else {
            EmitSignal("message_all_shown");
        }
    }

    private void _On_TweenAllCompleted() {
        if (!running) {
            return;
        }

        if (animStep == AnimStep.FadeIn) {
            // Start wait
            _MessageWait();
        }

        else if (animStep == AnimStep.FadeOut) {
            // End
            _MessageEnd();
        }
    }

    private void _On_TimerTimeout() {
        if (!running) {
            return;
        }

        // Fade out
        _MessageFadeOut();
    }
}
