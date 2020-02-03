using Godot;

using Dictionary = Godot.Collections.Dictionary;

public class Spawner : Node2D {
    // Signals
    [Signal] public delegate void spawn(Node node);

    // On ready
    [BindNode]
    private Timer timer;
    [BindNode("Elements")]
    private Node2D elements;
    [BindNodeRoot]
    private GameState gameState;

    // Exports
    [Export] public float frequency = 2.0f;
    [Export] public float speed = 100.0f;
    [Export] public float timeOffset = 0.0f;
    [Export] public Vector2 randScale = new Vector2(0.5f, 1.5f);
    [Export] public PackedScene element;
    [Export] public bool disabled = false;

    // Private
    private Node parentScene;
    private Dictionary signals = new Dictionary();

    async public override void _Ready() {
        this.BindNodes();

        timer.WaitTime = frequency;
        timer.Connect("timeout", this, nameof(_On_Timer_Timeout));

        if (timeOffset > 0) {
            await ToSignal(GetTree().CreateTimer(timeOffset), "timeout");
        }

        timer.Start();
    }

    public void ConnectTargetScene(Node scene, Dictionary signalsDef) {
        parentScene = scene;
        signals = signalsDef;
    }

    public void Reset() {
        disabled = false;
        timer.Start();
    }

    public void Disable() {
        disabled = true;
    }

    public void SetFrequency(float freq) {
        frequency = freq;
        timer.WaitTime = freq;
    }

    public Node SpawnAtPosition(Vector2 pos) {
        var instance = element.Instance();

        PrepareInstance(instance, pos);

        foreach (string signalName in signals.Keys) {
            var fnName = (string)signals[signalName];
            instance.Connect(signalName, parentScene, fnName);
        }

        elements.AddChild(instance);
        EmitSignal("spawn", instance);
        return instance;
    }

    private void PrepareInstance(Node instance, Vector2 pos) {
        instance.Call("Prepare", pos, speed, (float)GD.RandRange(randScale.x, randScale.y));
    }

    public Node Spawn() {
        var gameSize = gameState.GetGameSize();
        var minPos = gameSize.x / 4.0f;
        var maxPos = gameSize.x - gameSize.x / 4.0f;
        var pos = new Vector2((int)GD.RandRange(minPos, maxPos), -50.0f);

        return SpawnAtPosition(pos);
    }

    private void _On_Timer_Timeout() {
        if (disabled) {
            return;
        }

        Spawn();
    }
}
