using Godot;

public class TouchController : Node2D
{
    public bool Touching { get; set; }
    public bool DoubleTouching { get; set; }
    public Vector2 LastTouchPosition { get; set; }
    public Vector2 TouchDistance { get; set; }
    public Vector2 ComputedPosition {
        get {
            return LastTouchPosition + TouchDistance;
        }
    }

    public override void _Ready() {
        Touching = false;
        DoubleTouching = false;
        LastTouchPosition = new Vector2();
        TouchDistance = new Vector2();
    }

    public override void _Input(InputEvent @event) {
        if (@event is InputEventScreenTouch touch) {
            // First finger
            if (touch.Index == 0) {
                LastTouchPosition = touch.Position;
                Touching = touch.Pressed;
                TouchDistance = GlobalPosition - touch.Position;
            }

            // Second finger
            else if (touch.Index == 1) {
                DoubleTouching = touch.Pressed;
            }
        }

        else if (@event is InputEventScreenDrag drag) {
            if (drag.Index == 0) {
                LastTouchPosition = drag.Position;
            }
        }
    }

    public void ResetState() {
        Touching = false;
        DoubleTouching = false;
        LastTouchPosition = new Vector2();
        TouchDistance = new Vector2();
    }
}
