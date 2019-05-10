extends Node

###########
# Utilities

const FORCE_MOBILE = false

################
# Public methods

func is_mobile_platform():
    """Check if current platform is mobile."""
    if FORCE_MOBILE:
        return true
    return OS.get_name() in ["Android", "iOS"]

func get_game_size():
    """Get game size."""
    var width = ProjectSettings.get_setting("display/window/size/width")
    var height = ProjectSettings.get_setting("display/window/size/height")
    return Vector2(width, height)
