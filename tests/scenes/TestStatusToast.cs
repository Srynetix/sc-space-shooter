using Godot;

public class TestStatusToast : Control {
    [BindNode] private Player player;
    [BindNodeRoot] private Debug debug;

    private Console.Logger logger;

    async public override void _Ready() {
        this.BindNodes();

        debug.EnableDebugMode();
        logger = debug.GetLogger("TestStatusToast");
        logger.LogLevelFilter = Console.LogLevel.Error;

        player.Connect("fire", this, nameof(_On_Fire));
        player.Connect("respawn", this, nameof(_On_Respawn));

        player.GetStatusToast().ShowPriorityMessage("Hello World!");
        player.GetStatusToast().ShowMessage("Hello World 2!");

        await ToSignal(player.GetStatusToast(), "message_all_shown");

        logger.Debug("Beginning loop ...");

        while (true) {
            player.GetStatusToast().ShowMessage("Short");
            logger.Debug("Showing 'Short'");
            await ToSignal(player.GetStatusToast(), "message_shown");

            player.GetStatusToast().ShowMessage("Long long long long");
            logger.Debug("Showing 'Long long long long'");
            await ToSignal(player.GetStatusToast(), "message_shown");
        }
    }

    public override void _ExitTree() {
        debug.DisableDebugMode();
    }

    private void _On_Fire(Bullet.FireData fireData) {
        var instance = fireData.bullet.InstanceAs<Bullet>();
        instance.Prepare(fireData);
        AddChild(instance);
    }

    private void _On_Respawn() {
        player.Respawn();
    }
}
