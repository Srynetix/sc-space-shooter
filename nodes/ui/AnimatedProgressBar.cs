using Godot;

public class AnimatedProgressBar : ProgressBar {

    private const float FADE_SPEED = 1.0f;

    // Binds
    [BindNode] private Timer timer;
    [BindNode] private Tween tween;

    private bool _running = false;

    public override void _Ready() {
        this.BindNodes();

        timer.Connect("timeout", this, nameof(_On_Timer_Timeout));

        // Start hidden
        Modulate = Colors.Transparent;
    }

    public void FadeIn() {
        if (!_running) {
            _running = true;
            _FadeIn();
        } else {
            // Reset timer
            timer.Stop();
            timer.Start();
        }
    }

    async private void _FadeIn() {
        // Stop all tweens
        tween.StopAll();

        tween.InterpolateProperty(this, "modulate", Colors.Transparent, Colors.White, FADE_SPEED);
        tween.Start();

        await ToSignal(tween, "tween_all_completed");
        timer.Start();
    }

    private void _FadeOut() {
        tween.InterpolateProperty(this, "modulate", Modulate, Colors.Transparent, FADE_SPEED);
        tween.Start();
    }

    private void _On_Timer_Timeout() {
        if (_running) {
            _running = false;
            CallDeferred(nameof(_FadeOut));
        }
    }
}
