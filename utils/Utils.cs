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
    
    public static Vector2 GetGameSize() {
        var width = (int)ProjectSettings.GetSetting("display/window/size/width");
        var height = (int)ProjectSettings.GetSetting("display/window/size/height");
        return new Vector2(width, height);
    }
}
