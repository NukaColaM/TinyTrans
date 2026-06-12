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
    // pinned to the far left, so they read as left-aligned. The fix centers the
    // mark in the full image-margin GUTTER strip (menu edge -> content), leaving
    // text left-aligned. These tests pin the anchor's center to the gutter
    // strip's center - the coordinate space the earlier ImageRectangle-relative
    // tests missed - so a green test matches the on-screen placement.

    [Fact]
    public void CenteredInGutter_IsHorizontallyCenteredInTheStrip()
    {
        // A 28px-wide image-margin strip, 26px row: the mark anchor must sit at
        // the strip's center (14), not at its left edge.
        var anchor = TrayMarkGeometry.CenteredInGutter(gutterWidth: 28, gutterHeight: 26, markSize: 16);

        Assert.Equal(14f, anchor.X + anchor.Size / 2f, 3); // 28 / 2
        Assert.Equal(13f, anchor.Y + anchor.Size / 2f, 3); // 26 / 2
    }

    [Fact]
    public void CenteredInGutter_RadioDotCentersInStrip()
    {
        var anchor = TrayMarkGeometry.CenteredInGutter(gutterWidth: 28, gutterHeight: 26, markSize: 16);
        var dot = TrayMarkGeometry.RadioDot(anchor.X, anchor.Y, anchor.Size, anchor.Size);

        Assert.Equal(14f, dot.CenterX, 3); // centered in the 28px strip
    }

    [Fact]
    public void CenteredInGutter_CheckMarkCentersInStrip()
    {
        var anchor = TrayMarkGeometry.CenteredInGutter(gutterWidth: 28, gutterHeight: 26, markSize: 16);
        var mark = TrayMarkGeometry.CheckMark(anchor.X, anchor.Y, anchor.Size, anchor.Size);

        var minX = new[] { mark.Left.X, mark.Mid.X, mark.Right.X }.Min();
        var maxX = new[] { mark.Left.X, mark.Mid.X, mark.Right.X }.Max();

        Assert.Equal(14f, (minX + maxX) / 2f, 3); // centered in the 28px strip
    }
}
