using System.Linq;
using TinyTrans.Core;

namespace TinyTrans.Core.Tests;

public class TrayMarkGeometryTests
{
    [Fact]
    public void RadioDot_IsCenteredWithinSquareBounds()
    {
        var box = TrayMarkGeometry.RadioDot(0, 0, 16, 16);

        Assert.Equal(8f, box.CenterX, 3);
        Assert.Equal(8f, box.CenterY, 3);
    }

    [Fact]
    public void RadioDot_IsCenteredWithinOffsetNonSquareBounds()
    {
        // A wider-than-tall gutter offset from the origin: the dot must sit at
        // the geometric center of the rectangle, not at its top-left.
        var box = TrayMarkGeometry.RadioDot(10, 4, 20, 12);

        Assert.Equal(20f, box.CenterX, 3); // 10 + 20/2
        Assert.Equal(10f, box.CenterY, 3); // 4 + 12/2
    }

    [Fact]
    public void CheckMark_PointsBoundingBoxIsCenteredWithinBounds()
    {
        // The three checkmark vertices must be symmetric about the rectangle
        // center, so the glyph reads as centered rather than drifting left/up.
        var mark = TrayMarkGeometry.CheckMark(0, 0, 16, 16);

        var minX = new[] { mark.Left.X, mark.Mid.X, mark.Right.X }.Min();
        var maxX = new[] { mark.Left.X, mark.Mid.X, mark.Right.X }.Max();
        var minY = new[] { mark.Left.Y, mark.Mid.Y, mark.Right.Y }.Min();
        var maxY = new[] { mark.Left.Y, mark.Mid.Y, mark.Right.Y }.Max();

        Assert.Equal(8f, (minX + maxX) / 2f, 3);
        Assert.Equal(8f, (minY + maxY) / 2f, 3);
    }

    [Fact]
    public void CheckMark_PointsBoundingBoxIsCenteredWithinOffsetNonSquareBounds()
    {
        var mark = TrayMarkGeometry.CheckMark(10, 4, 20, 12);

        var minX = new[] { mark.Left.X, mark.Mid.X, mark.Right.X }.Min();
        var maxX = new[] { mark.Left.X, mark.Mid.X, mark.Right.X }.Max();
        var minY = new[] { mark.Left.Y, mark.Mid.Y, mark.Right.Y }.Min();
        var maxY = new[] { mark.Left.Y, mark.Mid.Y, mark.Right.Y }.Max();

        Assert.Equal(20f, (minX + maxX) / 2f, 3); // 10 + 20/2
        Assert.Equal(10f, (minY + maxY) / 2f, 3); // 4 + 12/2
    }

    // The runtime bug: marks were drawn into WinForms' narrow ImageRectangle,
    // pinned to the far left, so they read as left-aligned. The fix right-aligns
    // the mark in the full image-margin GUTTER strip (menu edge -> content) with
    // a small margin, positioning them closer to the text and making them appear
    // horizontally centered in the overall menu layout rather than stranded at
    // the far left edge.

    [Fact]
    public void CenteredInGutter_IsRightAlignedInTheStrip()
    {
        // A 28px-wide image-margin strip, 26px row: the mark anchor must be
        // right-aligned (near the right edge), not centered or left-aligned.
        var anchor = TrayMarkGeometry.CenteredInGutter(gutterWidth: 28, gutterHeight: 26, markSize: 16);

        // Anchor should be near the right edge: gutterWidth - markSize - margin
        // With 2px margin: 28 - 16 - 2 = 10px
        Assert.Equal(10f, anchor.X, 3);
        Assert.Equal(5f, anchor.Y, 3); // vertically centered: (26 - 16) / 2
    }

    [Fact]
    public void CenteredInGutter_RadioDotIsRightAligned()
    {
        var anchor = TrayMarkGeometry.CenteredInGutter(gutterWidth: 28, gutterHeight: 26, markSize: 16);
        var dot = TrayMarkGeometry.RadioDot(anchor.X, anchor.Y, anchor.Size, anchor.Size);

        // Dot center should be near the right edge of the gutter
        // anchor.X=10, markSize=16, so dot of diameter 8 centers at 10+8=18
        Assert.True(dot.CenterX >= 16f, $"Expected dot center >= 16px (right side), got {dot.CenterX}");
    }

    [Fact]
    public void CenteredInGutter_CheckMarkIsRightAligned()
    {
        var anchor = TrayMarkGeometry.CenteredInGutter(gutterWidth: 28, gutterHeight: 26, markSize: 16);
        var mark = TrayMarkGeometry.CheckMark(anchor.X, anchor.Y, anchor.Size, anchor.Size);

        var minX = new[] { mark.Left.X, mark.Mid.X, mark.Right.X }.Min();
        var maxX = new[] { mark.Left.X, mark.Mid.X, mark.Right.X }.Max();
        var centerX = (minX + maxX) / 2f;

        // Checkmark center should be near the right edge of the gutter
        Assert.True(centerX >= 16f, $"Expected check center >= 16px (right side), got {centerX}");
    }

    // The runtime bug (2026-06-13): markSize was calculated as
    // Math.Min(gutterWidth, rect.Height), producing a mark nearly as wide as
    // the gutter (e.g., 24px in a 25px gutter). This left only 0.5px centering
    // space, making the mark appear left-aligned. The fix ensures the mark is
    // proportional to the gutter width (50-60%), leaving visible centering space.

    [Fact]
    public void MarkSizeForGutter_ProducesProportionalSize()
    {
        // Real scenario from logs: 25px gutter, 24px rect.Height
        // Mark should be ~60% of gutter to allow visible centering while staying prominent
        var markSize = TrayMarkGeometry.MarkSizeForGutter(gutterWidth: 25, rectHeight: 24);

        // Mark should be roughly 60% of gutter width, not the full rect.Height
        Assert.True(markSize >= 14f && markSize <= 16f,
            $"Expected markSize between 14-16px (56-64% of 25px gutter), got {markSize}");
    }

    [Fact]
    public void MarkSizeForGutter_AllowsVisibleCentering()
    {
        // With proper mark sizing, the anchor should be right-aligned with visible margin
        var markSize = TrayMarkGeometry.MarkSizeForGutter(gutterWidth: 25, rectHeight: 24);
        var anchor = TrayMarkGeometry.CenteredInGutter(gutterWidth: 25, gutterHeight: 26, markSize);

        // Right-aligned: anchor.X should be near the right edge (gutterWidth - markSize - margin)
        // Expected: 25 - 15 - 2 = 8px
        Assert.True(anchor.X >= 7f && anchor.X <= 9f,
            $"Expected anchor.X ~8px (right-aligned), got {anchor.X}");
    }
}
