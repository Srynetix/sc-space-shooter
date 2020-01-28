using Godot;

public class TestNeon : Control {

    [BindNode]
    private Sprite sprite;
    [BindNode]
    private Tween tween;
    [BindNodeRoot]
    private GameState gameState;

    private ShaderMaterial shaderMaterial;

    async public override void _Ready() {
        this.BindNodes();

        shaderMaterial = (ShaderMaterial)sprite.Material;

        var gameSize = gameState.GetGameSize();
        sprite.Position = gameSize / 2;

        Color white = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        Color red = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        Color lightRed = new Color(1.0f, 0.8f, 0.2f, 1.0f);
        Color lightBlue = new Color(0.2f, 0.8f, 1.0f, 1.0f);
        Color superLightBlue = new Color(0.5f, 0.8f, 1.0f, 1.0f);
        Color grey = new Color(0.5f, 0.5f, 0.5f, 1.0f);

        while (true) {
            // tween.InterpolateProperty(shaderMaterial, "shader_param/alphaFade1", 0.5f, 0.1f, 1.0f, Tween.TransitionType.Bounce);
            tween.InterpolateProperty(shaderMaterial, "shader_param/alphaFade2", 0.0f, 3.0f, 1.0f, Tween.TransitionType.Bounce);
            tween.InterpolateProperty(shaderMaterial, "shader_param/tintColor1", grey, lightBlue, 1.0f);
            tween.InterpolateProperty(shaderMaterial, "shader_param/tintColor2", grey, superLightBlue, 1.0f);
            tween.InterpolateProperty(shaderMaterial, "shader_param/spriteColor", grey, white, 1.0f);
            // tween.InterpolateProperty(sprite, "rotation_degrees", 0, 360, 2.0f, Tween.TransitionType.Sine);
            tween.Start();
            await ToSignal(tween, "tween_all_completed");

            // tween.InterpolateProperty(shaderMaterial, "shader_param/alphaFade1", 0.1f, 0.5f, 1.0f, Tween.TransitionType.Bounce);
            tween.InterpolateProperty(shaderMaterial, "shader_param/alphaFade2", 3.0f, 0.0f, 1.0f, Tween.TransitionType.Bounce);
            tween.InterpolateProperty(shaderMaterial, "shader_param/tintColor1", lightBlue, grey, 1.0f);
            tween.InterpolateProperty(shaderMaterial, "shader_param/tintColor2", superLightBlue, grey, 1.0f);
            tween.InterpolateProperty(shaderMaterial, "shader_param/spriteColor", white, grey, 1.0f);
            // tween.InterpolateProperty(sprite, "rotation_degrees", 360, 0, 2.0f, Tween.TransitionType.Sine);
            tween.Start();
            await ToSignal(tween, "tween_all_completed");

            // tween.InterpolateProperty(shaderMaterial, "shader_param/alphaFade1", 0.5f, 0.1f, 1.0f, Tween.TransitionType.Bounce);
            tween.InterpolateProperty(shaderMaterial, "shader_param/alphaFade2", 0.0f, 3.0f, 1.0f, Tween.TransitionType.Bounce);
            tween.InterpolateProperty(shaderMaterial, "shader_param/tintColor1", grey, lightRed, 1.0f);
            tween.InterpolateProperty(shaderMaterial, "shader_param/tintColor2", grey, red, 1.0f);
            tween.InterpolateProperty(shaderMaterial, "shader_param/spriteColor", grey, red, 1.0f);
            // tween.InterpolateProperty(sprite, "rotation_degrees", 0, 360, 2.0f, Tween.TransitionType.Sine);
            tween.Start();
            await ToSignal(tween, "tween_all_completed");

            // tween.InterpolateProperty(shaderMaterial, "shader_param/alphaFade1", 0.1f, 0.5f, 1.0f, Tween.TransitionType.Bounce);
            tween.InterpolateProperty(shaderMaterial, "shader_param/alphaFade2", 3.0f, 0.0f, 1.0f, Tween.TransitionType.Bounce);
            tween.InterpolateProperty(shaderMaterial, "shader_param/tintColor1", lightRed, grey, 1.0f);
            tween.InterpolateProperty(shaderMaterial, "shader_param/tintColor2", red, grey, 1.0f);
            tween.InterpolateProperty(shaderMaterial, "shader_param/spriteColor", red, grey, 1.0f);
            // tween.InterpolateProperty(sprite, "rotation_degrees", 360, 0, 2.0f, Tween.TransitionType.Sine);
            tween.Start();
            await ToSignal(tween, "tween_all_completed");
        }
    }
}
