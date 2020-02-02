using Godot;

public class TestConsoleLog : Control
{
    [BindNodeRoot] private Debug debug;

    private Console.Logger logger;

    async public override void _Ready() {
        this.BindNodes();

        debug.EnableDebugMode();
        logger = debug.GetLogger("TestConsoleLog");
        logger.LogLevelFilter = Console.LogLevel.Debug;

        logger.Debug("This is a debug message.");
        logger.Info("This is an info message.");
        logger.Warn("This is a warn message.");
        logger.Error("This is an error message.");

        logger.Info("Setting log level to Warn ...");
        logger.LogLevelFilter = Console.LogLevel.Warn;
        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");

        logger.Debug("This debug message is hidden.");
        logger.Info("This info message is hidden.");
        logger.Warn("This warn message is shown.");
        logger.Error("This error message is shown.");
    }

    public override void _ExitTree() {
        debug.DisableDebugMode();
    }
}
