using Godot;

using Dictionary = Godot.Collections.Dictionary;

public class Spawner : Node2D
{
    // On ready
    private Timer timer;
    private Node elements;
    
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
    
    async public override void _Ready()
    {
        // On ready
        timer = GetNode<Timer>("Timer");
        elements = GetNode<Node>("Elements");
        
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
    
    public void SpawnAtPosition(Vector2 pos) {
        var instance = element.Instance();
        
        instance.Call("Prepare", pos, speed, GD.RandRange(randScale.x, randScale.y));
        
        foreach (string signalName in signals.Keys) {
            var fnName = (string)signals[signalName];
            instance.Connect(signalName, parentScene, fnName);
        }
    }
    
    public void Spawn() {
        var gameSize = Utils.GetGameSize();
        var minPos = gameSize.x / 4.0f;
        var maxPos = gameSize.y / 4.0f;
        var pos = new Vector2((int)GD.RandRange(minPos, maxPos), -50.0f);
        
        SpawnAtPosition(pos);
    }
    
    private void _On_Timer_Timeout() {
        if (disabled) {
            return;
        }
        
        Spawn();
    }
}
