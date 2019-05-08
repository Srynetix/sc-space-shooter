extends CanvasLayer

###################
# Transition system

onready var animation_player = $AnimationPlayer

################
# Public methods

func fade_to_scene(scene_path, transition_speed=1):
    """
    Fade screen to another scene.
    
    :param scene_path:          Scene path
    :param transition_speed:    Transition speed
    """
    var scene = load(scene_path)

    self.animation_player.playback_speed = transition_speed
    self.animation_player.play("fadeout")
    yield(self.animation_player, "animation_finished")

    self.get_tree().change_scene_to(scene)
    self.animation_player.play("fadein")