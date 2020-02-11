using Godot;

/// <summary>
/// AnimatedText node.
/// Drawing part is inspired by Label's own drawing function from Godot source.
/// </summary>
[Tool]
public class AnimatedText : Label {

    public enum Style {
        Normal = 1,
        Shake,
        Wave
    }

    [Export] public string AnimatedTextValue = "";
    [Export] public Style AnimatedStyle = Style.Normal;
    [Export] public float WaveAmplitude = 2.0f;
    [Export] public float WaveFrequency = 10.0f;
    [Export] public float WaveSpeed = 1.0f;
    [Export] public float WSeparation = 0.0f;
    [Export] public float ShakeAmplitude = 0.5f;
    [Export] public float ShakeFrequency = 75.0f;

    private float waveX = 0;
    private float waveXLimit = 2 * Mathf.Pi;

    private Vector2 compPos;
    private Vector2 compVec;
    private Rect2 styleDraw;

    public override void _Process(float delta) {
        _WaveProcess(delta);
        Update();
    }

    private void _WaveProcess(float delta) {
        waveX += delta * WaveSpeed;

        // Regulate
        while (waveX > waveXLimit) {
            waveX -= waveXLimit;
        }
    }

    public override void _Notification(int what) {
        if (what == NotificationDraw) {
            var ci = GetCanvasItem();
            var font = GetFont("font");
            var style = GetStylebox("normal");
            var size = RectSize;
            var textValue = Tr(AnimatedTextValue);
            var textSize = font.GetStringSize(textValue);
            var charCount = textValue.Length;
            int lineSpacing = GetConstant("line_spacing");
            float fontH = font.GetHeight() + lineSpacing;
            int linesVisible = (int)((size.y + lineSpacing) / fontH);

            // Style
            styleDraw.Position = Vector2.Zero;
            styleDraw.Size = size;
            style.Draw(ci, styleDraw);

            // Compute X offset
            float xOfs = 0;
            switch (Align) {
                case AlignEnum.Fill:
                case AlignEnum.Left:
                    xOfs = style.GetOffset().x;
                    break;
                case AlignEnum.Center:
                    xOfs = (int)((size.x - textSize.x - (WSeparation * charCount)) / 2);
                    break;
                case AlignEnum.Right:
                    xOfs = (int)(size.x - textSize.x - (WSeparation * charCount) - style.GetMargin(Margin.Right));
                    break;
            }

            // Compute Y offset
            float yOfs = style.GetOffset().y;
            yOfs += font.GetAscent();

            // Initial draw position
            compPos.x = xOfs;
            compPos.y = yOfs;
            var drawPos = _CalculateDrawPos(0, compPos, compPos, 0);

            // Drawing
            for (int i = 0; i < textValue.Length; ++i) {
                var ch = textValue[i].ToString();
                var nextCh = (i + 1 < textValue.Length ? textValue[i + 1].ToString() : "");
                var advance = DrawChar(font, drawPos, ch, nextCh, Colors.White) + WSeparation;
                drawPos = _CalculateDrawPos(i, compPos, drawPos, advance);
            }
        }
    }

    private Vector2 _CalculateDrawPos(int charIdx, Vector2 initialPos, Vector2 prevPos, float xAdvance) {
        if (AnimatedStyle == Style.Wave) {
            var nx = Mathf.Cos(charIdx + (waveX * WaveFrequency / 2.0f)) * WaveAmplitude;
            var ny = Mathf.Sin(charIdx + (waveX * WaveFrequency)) * WaveAmplitude;
            compVec.x = prevPos.x + xAdvance + nx;
            compVec.y = initialPos.y + ny;
        } else if (AnimatedStyle == Style.Shake) {
            var ny = Mathf.Sin(charIdx + (waveX * ShakeFrequency)) * ShakeAmplitude;
            compVec.x = prevPos.x + xAdvance;
            compVec.y = initialPos.y + ny;
        } else {
            compVec.x = prevPos.x + xAdvance;
            compVec.y = prevPos.y;
        }

        return compVec;
    }
}
