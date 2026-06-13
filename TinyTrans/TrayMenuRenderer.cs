using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TinyTrans.Core;

namespace TinyTrans;

/// <summary>
/// Role of a tray menu item, tagged at construction so the renderer can
/// special-case headers and draw the correct accent mark without string
/// matching or relying on <see cref="ToolStripMenuItem.CheckState"/> (both
/// language radios and on/off toggles use Checked).
/// </summary>
internal enum TrayMenuItemRole
{
    Normal,
    HeaderTitle,
    HeaderHint,
    LanguageRadio,
    Toggle,
}

/// <summary>
/// Accent mark a checked item should draw in place of the native WinForms
/// check glyph. Kept as a pure mapping (no GDI types) so the role -> mark
/// decision is trivial to follow and verify.
/// </summary>
internal enum TrayCheckMark
{
    None,
    RadioDot,
    CheckMark,
}

/// <summary>
/// Flat light palette for the tray menu. Values mirror the WPF
/// <c>Styles.xaml</c> resources; WinForms cannot read the WPF resource
/// dictionary, so the hex values are duplicated here by design.
/// </summary>
internal static class TrayMenuColors
{
    internal static readonly Color Surface = ColorTranslator.FromHtml("#F5F5F5");   // WindowBackgroundColor
    internal static readonly Color Hover = ColorTranslator.FromHtml("#E0E0E0");     // ButtonHoverColor
    internal static readonly Color Border = ColorTranslator.FromHtml("#CCCCCC");    // WindowBorderColor
    internal static readonly Color Subdued = ColorTranslator.FromHtml("#888888");   // SubduedTextColor

    // Primary item text. Not in Styles.xaml (WPF inherits a system default);
    // a near-black keeps actionable rows legible on the light surface.
    internal static readonly Color Text = ColorTranslator.FromHtml("#202020");

    // Accent marks are derived from the pressed tone (#D0D0D0) but darkened so
    // the radio dot / checkmark stay legible against the #F5F5F5 surface and
    // #E0E0E0 hover fill; a literal #D0D0D0 glyph would be near-invisible.
    internal static readonly Color AccentMark = ColorTranslator.FromHtml("#4A4A4A");
}

/// <summary>
/// Flat light renderer for the tray <see cref="ContextMenuStrip"/>. Recolors
/// the surface, hover/selection, separators and border to the app palette;
/// flattens the image margin; draws custom accent marks for checked items; and
/// styles the leading header rows. Item roles are read from
/// <see cref="ToolStripItem.Tag"/> (a <see cref="TrayMenuItemRole"/>).
/// </summary>
internal sealed class TrayMenuRenderer : ToolStripProfessionalRenderer
{
    private const int SelectionCornerRadius = 4;
    private const int SelectionInset = 2;

    // Width of the image-margin gutter strip (menu edge -> content), captured
    // from OnRenderImageMargin so OnRenderItemCheck can center the mark in the
    // FULL strip rather than in WinForms' narrow, far-left ImageRectangle.
    // Width is translation-invariant, so a menu-space width is valid to reuse
    // in item-relative space. Both callbacks fire within the same paint, image
    // margin before items.
    private int _gutterWidth;

    internal TrayMenuRenderer()
        : base(new TrayMenuColorTable())
    {
        RoundedEdges = false;
    }

    /// <summary>Pure role -> mark mapping for a (role, checked) pair.</summary>
    internal static TrayCheckMark MarkFor(TrayMenuItemRole role, bool isChecked)
    {
        if (!isChecked)
        {
            return TrayCheckMark.None;
        }

        return role switch
        {
            TrayMenuItemRole.LanguageRadio => TrayCheckMark.RadioDot,
            TrayMenuItemRole.Toggle => TrayCheckMark.CheckMark,
            _ => TrayCheckMark.None,
        };
    }

    private static TrayMenuItemRole RoleOf(ToolStripItem item)
        => item.Tag is TrayMenuItemRole role ? role : TrayMenuItemRole.Normal;

    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
        // Capture the real gutter width (menu edge -> content) so OnRenderItemCheck
        // can center the mark across the whole strip instead of the narrow
        // ImageRectangle. This is the strip the marks were previously stranded at
        // the left edge of.
        _gutterWidth = e.AffectedBounds.Width;

        // Flatten the left gutter so it matches the surface instead of the
        // default shaded image margin; custom marks are drawn on top of it.
        using var brush = new SolidBrush(TrayMenuColors.Surface);
        e.Graphics.FillRectangle(brush, e.AffectedBounds);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        var item = e.Item;
        var bounds = new Rectangle(Point.Empty, item.Size);

        using (var surface = new SolidBrush(TrayMenuColors.Surface))
        {
            e.Graphics.FillRectangle(surface, bounds);
        }

        if (item.Selected && item.Enabled)
        {
            var fill = Rectangle.Inflate(bounds, -SelectionInset, -SelectionInset);
            var previousMode = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RoundedRectangle(fill, SelectionCornerRadius))
            using (var brush = new SolidBrush(TrayMenuColors.Hover))
            {
                e.Graphics.FillPath(brush, path);
            }
            e.Graphics.SmoothingMode = previousMode;
        }
    }

    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
    {
        // Suppress the native check glyph entirely and draw our own mark.
        var mark = MarkFor(RoleOf(e.Item), (e.Item as ToolStripMenuItem)?.Checked ?? false);
        if (mark == TrayCheckMark.None)
        {
            return;
        }

        var rect = e.ImageRectangle;
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        // Center the mark in the FULL image-margin strip (menu edge -> content),
        // not in WinForms' narrow far-left ImageRectangle. The strip width was
        // captured in OnRenderImageMargin; fall back to the rect if it wasn't.
        // The item shares the strip's left origin, so [0, gutterWidth] in
        // item-relative space is the strip; vertical centering uses item height.
        float gutterWidth = _gutterWidth > 0 ? _gutterWidth : rect.Right;
        float gutterHeight = e.Item.Height;
        float markSize = TrayMarkGeometry.MarkSizeForGutter(gutterWidth, rect.Height);
        var anchor = TrayMarkGeometry.CenteredInGutter(gutterWidth, gutterHeight, markSize);

        var previousMode = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        if (mark == TrayCheckMark.RadioDot)
        {
            var dot = TrayMarkGeometry.RadioDot(anchor.X, anchor.Y, anchor.Size, anchor.Size);
            using var brush = new SolidBrush(TrayMenuColors.AccentMark);
            e.Graphics.FillEllipse(brush, dot.X, dot.Y, dot.Diameter, dot.Diameter);
        }
        else // CheckMark
        {
            var check = TrayMarkGeometry.CheckMark(anchor.X, anchor.Y, anchor.Size, anchor.Size);
            using var pen = new Pen(TrayMenuColors.AccentMark, 2f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round,
            };
            e.Graphics.DrawLines(pen, new[]
            {
                new PointF(check.Left.X, check.Left.Y),
                new PointF(check.Mid.X, check.Mid.Y),
                new PointF(check.Right.X, check.Right.Y),
            });
        }

        e.Graphics.SmoothingMode = previousMode;
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        switch (RoleOf(e.Item))
        {
            case TrayMenuItemRole.HeaderTitle:
                // Bold font is set on the item itself at construction (the menu
                // owns and disposes it), so we only recolor here.
                e.TextColor = TrayMenuColors.Text;
                break;
            case TrayMenuItemRole.HeaderHint:
                e.TextColor = TrayMenuColors.Subdued;
                break;
            default:
                e.TextColor = e.Item.Enabled ? TrayMenuColors.Text : TrayMenuColors.Subdued;
                break;
        }

        base.OnRenderItemText(e);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        var bounds = new Rectangle(Point.Empty, e.ToolStrip.Size);
        bounds.Width -= 1;
        bounds.Height -= 1;
        using var pen = new Pen(TrayMenuColors.Border);
        e.Graphics.DrawRectangle(pen, bounds);
    }

    private static GraphicsPath RoundedRectangle(Rectangle rect, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();
        if (d <= 0 || rect.Width <= d || rect.Height <= d)
        {
            path.AddRectangle(rect);
            return path;
        }

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

/// <summary>
/// Color table flattening the WinForms "professional" gradients into the flat
/// light palette: surface background, flat hover/selection fill, palette
/// separators, and a clean image margin.
/// </summary>
internal sealed class TrayMenuColorTable : ProfessionalColorTable
{
    internal TrayMenuColorTable()
    {
        UseSystemColors = false;
    }

    public override Color ToolStripDropDownBackground => TrayMenuColors.Surface;

    public override Color MenuItemSelected => TrayMenuColors.Hover;
    public override Color MenuItemSelectedGradientBegin => TrayMenuColors.Hover;
    public override Color MenuItemSelectedGradientEnd => TrayMenuColors.Hover;
    public override Color MenuItemPressedGradientBegin => TrayMenuColors.Hover;
    public override Color MenuItemPressedGradientMiddle => TrayMenuColors.Hover;
    public override Color MenuItemPressedGradientEnd => TrayMenuColors.Hover;
    public override Color MenuItemBorder => TrayMenuColors.Hover;

    public override Color MenuBorder => TrayMenuColors.Border;

    public override Color ImageMarginGradientBegin => TrayMenuColors.Surface;
    public override Color ImageMarginGradientMiddle => TrayMenuColors.Surface;
    public override Color ImageMarginGradientEnd => TrayMenuColors.Surface;

    public override Color SeparatorDark => TrayMenuColors.Border;
    public override Color SeparatorLight => TrayMenuColors.Surface;

    public override Color CheckBackground => TrayMenuColors.Surface;
    public override Color CheckSelectedBackground => TrayMenuColors.Hover;
    public override Color CheckPressedBackground => TrayMenuColors.Hover;
}
