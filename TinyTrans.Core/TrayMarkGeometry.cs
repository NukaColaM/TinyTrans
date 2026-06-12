namespace TinyTrans.Core;

/// <summary>
/// Bounding box of the radio dot, in the same coordinate space as the check
/// rectangle passed in. Pure value type (no GDI dependency) so the centering
/// math can be unit-tested off-Windows; the WinForms renderer turns this into
/// an <c>FillEllipse</c> call.
/// </summary>
public readonly record struct DotGeometry(float X, float Y, float Diameter)
{
    public float CenterX => X + Diameter / 2f;
    public float CenterY => Y + Diameter / 2f;
}

/// <summary>A point in the check rectangle's coordinate space.</summary>
public readonly record struct MarkPoint(float X, float Y);

/// <summary>
/// A square anchor box (in gutter-relative coordinates) that a mark is drawn
/// into. Produced by <see cref="TrayMarkGeometry.CenteredInGutter"/> so the
/// mark sits centered in the full image-margin strip rather than pinned to the
/// far-left edge of WinForms' narrow check rectangle.
/// </summary>
public readonly record struct AnchorBox(float X, float Y, float Size);

/// <summary>
/// The three vertices of the checkmark stroke (down-stroke to the low corner,
/// then up-stroke to the high right). Pure values so the WinForms renderer can
/// turn them into a <c>DrawLines</c> call.
/// </summary>
public readonly record struct CheckGeometry(MarkPoint Left, MarkPoint Mid, MarkPoint Right);

/// <summary>
/// Pure geometry for the tray menu's custom check marks. Keeps the
/// "where exactly do the dot and checkmark sit" decision out of the GDI
/// rendering code so it can be verified directly.
/// </summary>
public static class TrayMarkGeometry
{
    /// <summary>
    /// A square anchor for the mark, centered in the image-margin gutter strip
    /// (<paramref name="gutterWidth"/> x <paramref name="gutterHeight"/>) rather
    /// than pinned to the far-left edge of WinForms' narrow check rectangle.
    /// This is the fix for marks reading as left-aligned: the renderer captures
    /// the real margin strip from <c>OnRenderImageMargin</c>, calls this, and
    /// draws the mark into the returned box. Text stays left-aligned.
    /// </summary>
    public static AnchorBox CenteredInGutter(float gutterWidth, float gutterHeight, float markSize)
    {
        float x = (gutterWidth - markSize) / 2f;
        float y = (gutterHeight - markSize) / 2f;
        return new AnchorBox(x, y, markSize);
    }

    /// <summary>
    /// A filled radio dot centered within the (x, y, width, height) box.
    /// Diameter is half the smaller side (min 6px) so the dot is proportional
    /// and never larger than the box.
    /// </summary>
    public static DotGeometry RadioDot(float x, float y, float width, float height)
    {
        float diameter = Math.Max(6, Math.Min(width, height) / 2f);
        float dotX = x + (width - diameter) / 2f;
        float dotY = y + (height - diameter) / 2f;
        return new DotGeometry(dotX, dotY, diameter);
    }

    /// <summary>
    /// The three vertices of a checkmark, centered within the (x, y, width,
    /// height) box. The stroke spans a square inscribed in the box so the
    /// glyph stays centered on non-square boxes; offsets are symmetric about
    /// the center so the points' bounding box is centered too.
    /// </summary>
    public static CheckGeometry CheckMark(float x, float y, float width, float height)
    {
        float cx = x + width / 2f;
        float cy = y + height / 2f;

        // Span a square inscribed in the rectangle so width != height does not
        // skew the glyph; the checkmark occupies ~70% of that square.
        float side = Math.Min(width, height);
        float half = side * 0.35f;

        // Down-stroke from upper-left to the bottom vertex, then up-stroke to
        // the upper-right. The bottom vertex sits below center; the two top
        // vertices sit above center. Horizontal offsets are symmetric so the
        // {left, mid, right} bounding box centers on (cx, cy).
        var left = new MarkPoint(cx - half, cy);
        var mid = new MarkPoint(cx - half * 0.30f, cy + half * 0.7f);
        var right = new MarkPoint(cx + half, cy - half * 0.7f);
        return new CheckGeometry(left, mid, right);
    }
}
