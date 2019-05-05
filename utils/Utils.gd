extends Node

###########
# Utilities

var FORCE_MOBILE = false

################
# Public methods

func is_mobile_platform():
    """Check if current platform is mobile."""
    if FORCE_MOBILE:
        return true
    return OS.get_name() in ["Android", "iOS"]