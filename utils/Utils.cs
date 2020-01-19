using Godot;

public class Utils : Node
{
    public static bool FORCE_MOBILE = false;

    public static bool IsMobilePlatform() {
        if (FORCE_MOBILE) {
            return true;
        }
        
        return OS.GetName() == "Android" || OS.GetName() == "iOS";
    }
}
