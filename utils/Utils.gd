extends Node

###########
# Utilities

################
# Public methods

func is_mobile_platform():
    """Check if current platform is mobile."""
    return OS.get_name() in ["Android", "iOS"]