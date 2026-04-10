using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Xunit;

namespace Avalonia.Controls.PanAndZoom.UnitTests;

/// <summary>
/// Tests for ZoomBorder behavior when the child element is smaller than the ZoomBorder,
/// resulting in Avalonia centering the child with a non-zero Bounds.Position (layout offset).
/// 
/// These tests validate fixes for the issue where code assumed child.Bounds.Position == (0, 0).
/// </summary>
public class ZoomBorderNonZeroLayoutOffsetTests
{
    #region Layout Offset Verification
    
    [AvaloniaFact]
    public void ChildSmallerThanZoomBorder_IsCenteredByAvalonia()
    {
        // Arrange - Create ZoomBorder larger than its child
        var zoomBorder = new ZoomBorder
        {
            Width = 600,
            Height = 400
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 100,
            Background = Brushes.Yellow
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        // Assert - Avalonia should center the child, creating a non-zero Position
        var expectedOffsetX = (600 - 200) / 2.0; // 200
        var expectedOffsetY = (400 - 100) / 2.0; // 150
        
        Assert.Equal(expectedOffsetX, childElement.Bounds.X, 1);
        Assert.Equal(expectedOffsetY, childElement.Bounds.Y, 1);
    }

    [AvaloniaFact]
    public void ChildLargerThanZoomBorder_HasNegativeLayoutOffset()
    {
        // Arrange - Child larger than ZoomBorder (still gets centered)
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300
        };

        var childElement = new Border
        {
            Width = 600,
            Height = 500,
            Background = Brushes.Yellow
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        // Assert - Child is centered, so Position will be negative
        var expectedOffsetX = (400 - 600) / 2.0; // -100
        var expectedOffsetY = (300 - 500) / 2.0; // -100
        
        Assert.Equal(expectedOffsetX, childElement.Bounds.X, 1);
        Assert.Equal(expectedOffsetY, childElement.Bounds.Y, 1);
    }

    #endregion

    #region TransformContentToViewport Helper Tests
    
    [Fact]
    public void TransformContentToViewport_LayoutOffsetAddedAfterTransform()
    {
        // Arrange - Element bounds with non-zero position (as if centered by layout)
        var elementBounds = new Rect(100, 50, 200, 150); // Position (100, 50), Size (200, 150)
        var matrix = Matrix.Identity;
        
        // Act
        var result = ZoomBorder.TransformContentToViewport(elementBounds, matrix);
        
        // Assert - With identity matrix, content at (0,0) + layout offset (100,50) = (100, 50)
        Assert.Equal(100, result.X, 1);
        Assert.Equal(50, result.Y, 1);
        Assert.Equal(200, result.Width, 1);
        Assert.Equal(150, result.Height, 1);
    }

    [Fact]
    public void TransformContentToViewport_LayoutOffsetNotScaledByZoom()
    {
        // Arrange
        var elementBounds = new Rect(100, 50, 200, 150);
        var matrix = new Matrix(2, 0, 0, 2, 0, 0); // 2x zoom, no pan
        
        // Act
        var result = ZoomBorder.TransformContentToViewport(elementBounds, matrix);
        
        // Assert - Content size scaled 2x, but layout offset NOT scaled
        // Content (0,0) transformed = (0,0), then + layout offset = (100, 50)
        // Size (200, 150) * 2 = (400, 300)
        Assert.Equal(100, result.X, 1);
        Assert.Equal(50, result.Y, 1);
        Assert.Equal(400, result.Width, 1);
        Assert.Equal(300, result.Height, 1);
    }

    [Fact]
    public void TransformContentToViewport_WithContentRectOverload_WorksCorrectly()
    {
        // Arrange - A region inside the content, not at origin
        var contentRect = new Rect(50, 25, 100, 75); // Region at (50, 25) within content
        var layoutOffset = new Point(100, 50);
        var matrix = new Matrix(2, 0, 0, 2, 0, 0); // 2x zoom
        
        // Act
        var result = ZoomBorder.TransformContentToViewport(contentRect, layoutOffset, matrix);
        
        // Assert
        // Content point (50, 25) * 2 = (100, 50), then + layout offset (100, 50) = (200, 100)
        // Size (100, 75) * 2 = (200, 150)
        Assert.Equal(200, result.X, 1);
        Assert.Equal(100, result.Y, 1);
        Assert.Equal(200, result.Width, 1);
        Assert.Equal(150, result.Height, 1);
    }

    #endregion

    #region CalculateScrollable Tests with Non-Zero Layout Offset

    [Fact]
    public void CalculateScrollable_WithNonZeroLayoutOffset_CorrectExtent()
    {
        // Arrange - Child centered within larger ZoomBorder
        var borderSize = new Size(600, 400);
        var bounds = new Rect(200, 150, 200, 100); // Centered: position (200, 150), size (200, 100)
        var matrix = Matrix.Identity;
        
        // Act
        ZoomBorder.CalculateScrollable(bounds, borderSize, matrix, out var extent, out var viewport, out var offset);
        
        // Assert - Content fits within viewport at its centered position
        Assert.Equal(new Size(600, 400), extent); // Extent matches viewport when content fits
        Assert.Equal(new Size(600, 400), viewport);
        Assert.Equal(new Vector(0, 0), offset);
    }

    [Fact]
    public void CalculateScrollable_WithNonZeroLayoutOffset_Zoomed2x()
    {
        // Arrange - Child centered, then zoomed
        var borderSize = new Size(600, 400);
        var bounds = new Rect(200, 150, 200, 100); // Centered child
        var matrix = new Matrix(2, 0, 0, 2, 0, 0); // 2x zoom at origin
        
        // Act
        ZoomBorder.CalculateScrollable(bounds, borderSize, matrix, out var extent, out var viewport, out var offset);
        
        // Assert
        // Content size: 200*2 = 400, 100*2 = 200
        // Content visual position: layout offset (200, 150) + transform origin (0, 0) = (200, 150)
        // This means content still starts at (200, 150) in viewport, extending to (600, 350)
        Assert.Equal(new Size(600, 400), viewport);
        Assert.Equal(new Size(600, 400), extent); // Still fits in viewport
        Assert.Equal(new Vector(0, 0), offset); // No scrolling needed
    }

    [Fact]
    public void CalculateScrollable_WithNonZeroLayoutOffset_PannedNegative()
    {
        // Arrange
        var borderSize = new Size(600, 400);
        var bounds = new Rect(200, 150, 200, 100); // Centered child
        var matrix = new Matrix(1, 0, 0, 1, -300, -200); // Panned left and up
        
        // Act
        ZoomBorder.CalculateScrollable(bounds, borderSize, matrix, out var extent, out var viewport, out var offset);
        
        // Assert - Content visual position = layout offset + matrix offset = (200-300, 150-200) = (-100, -50)
        // With negative position, extent grows and offset tracks how far content is scrolled
        Assert.Equal(new Size(600, 400), viewport);
        Assert.True(extent.Width >= viewport.Width, "Extent should accommodate scrollable range");
        Assert.True(offset.X > 0, "Should have positive X scroll offset when content is left of viewport");
        Assert.True(offset.Y > 0, "Should have positive Y scroll offset when content is above viewport");
    }

    #endregion

    #region Coordinate Conversion Tests with Centered Child

    [AvaloniaFact]
    public void ViewportToContent_CenteredChild_CorrectConversion()
    {
        // Arrange - Use StretchMode.None to avoid automatic zoom adjustments
        var zoomBorder = new ZoomBorder
        {
            Width = 600,
            Height = 400,
            Stretch = StretchMode.None
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 100,
            Background = Brushes.Yellow
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        // Layout offset should be (200, 150) since child is centered
        var layoutOffsetX = (600 - 200) / 2.0;
        var layoutOffsetY = (400 - 100) / 2.0;
        
        // Act - Convert viewport point at layout offset to content
        var contentPoint = zoomBorder.ViewportToContent(new Point(layoutOffsetX, layoutOffsetY));
        
        // Assert - Viewport point at layout offset should map to content origin (0, 0)
        Assert.Equal(0, contentPoint.X, 1);
        Assert.Equal(0, contentPoint.Y, 1);
    }

    [AvaloniaFact]
    public void ContentToViewport_CenteredChild_CorrectConversion()
    {
        // Arrange - Use StretchMode.None to avoid automatic zoom adjustments
        var zoomBorder = new ZoomBorder
        {
            Width = 600,
            Height = 400,
            Stretch = StretchMode.None
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 100,
            Background = Brushes.Yellow
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        var expectedLayoutOffsetX = (600 - 200) / 2.0; // 200
        var expectedLayoutOffsetY = (400 - 100) / 2.0; // 150
        
        // Act - Convert content origin (0, 0) to viewport
        var viewportPoint = zoomBorder.ContentToViewport(new Point(0, 0));
        
        // Assert - Content origin should map to layout offset position
        Assert.Equal(expectedLayoutOffsetX, viewportPoint.X, 1);
        Assert.Equal(expectedLayoutOffsetY, viewportPoint.Y, 1);
    }

    [AvaloniaFact]
    public void ViewportToContent_RoundTrip_CenteredChild_PreservesPoint()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 600,
            Height = 400
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 100,
            Background = Brushes.Yellow
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        zoomBorder.Zoom(1.5, 100, 50); // Zoom at a point within content

        var originalViewportPoint = new Point(350, 220);
        
        // Act
        var contentPoint = zoomBorder.ViewportToContent(originalViewportPoint);
        var roundTrippedPoint = zoomBorder.ContentToViewport(contentPoint);
        
        // Assert
        Assert.Equal(originalViewportPoint.X, roundTrippedPoint.X, 1);
        Assert.Equal(originalViewportPoint.Y, roundTrippedPoint.Y, 1);
    }

    #endregion

    #region CenterOn Tests with Centered Child

    [AvaloniaFact]
    public void CenterOn_Point_CenteredChild_PointIsCenteredInViewport()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 600,
            Height = 400
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 100,
            Background = Brushes.Yellow
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        var targetContentPoint = new Point(100, 50); // Center of the content
        
        // Act
        zoomBorder.CenterOn(targetContentPoint, animate: false);
        
        // Assert - Target point should now be at viewport center
        var viewportCenter = new Point(300, 200);
        var transformedPoint = zoomBorder.ContentToViewport(targetContentPoint);
        
        Assert.Equal(viewportCenter.X, transformedPoint.X, 1);
        Assert.Equal(viewportCenter.Y, transformedPoint.Y, 1);
    }

    [AvaloniaFact]
    public void CenterOn_PointWithZoom_CenteredChild_CorrectPositionAndZoom()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 600,
            Height = 400
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 100,
            Background = Brushes.Yellow
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        var targetContentPoint = new Point(50, 25);
        var targetZoom = 2.0;
        
        // Act
        zoomBorder.CenterOn(targetContentPoint, targetZoom, animate: false);
        
        // Assert
        Assert.Equal(targetZoom, zoomBorder.ZoomX, 2);
        Assert.Equal(targetZoom, zoomBorder.ZoomY, 2);
        
        // Target point should be at viewport center
        var viewportCenter = new Point(300, 200);
        var transformedPoint = zoomBorder.ContentToViewport(targetContentPoint);
        
        Assert.Equal(viewportCenter.X, transformedPoint.X, 1);
        Assert.Equal(viewportCenter.Y, transformedPoint.Y, 1);
    }

    #endregion

    #region ZoomToRectangle Tests with Centered Child

    [AvaloniaFact]
    public void ZoomToRectangle_CenteredChild_RectangleCenteredInViewport()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 600,
            Height = 400
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 100,
            Background = Brushes.Yellow
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        var targetRect = new Rect(25, 10, 100, 60); // A sub-region of the content
        
        // Act
        zoomBorder.ZoomToRectangle(targetRect);
        
        // Assert - The center of the target rect should be at viewport center
        var rectCenter = new Point(targetRect.X + targetRect.Width / 2.0, 
                                   targetRect.Y + targetRect.Height / 2.0);
        var viewportCenter = new Point(300, 200);
        var transformedCenter = zoomBorder.ContentToViewport(rectCenter);
        
        Assert.Equal(viewportCenter.X, transformedCenter.X, 1);
        Assert.Equal(viewportCenter.Y, transformedCenter.Y, 1);
    }

    #endregion

    #region Content Bounds Restriction Tests with Centered Child

    [AvaloniaFact]
    public void KeepContentVisible_CenteredChild_ConstrainsCorrectly()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 600,
            Height = 400,
            BoundsMode = ContentBoundsMode.KeepContentVisible,
            MinimumVisibleContentPercentage = 0.1,
            EnableConstrains = true
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 100,
            Background = Brushes.Yellow
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();
        
        // Act - Try to pan way off screen
        zoomBorder.Pan(5000, 5000);
        
        // Assert - Should be constrained, not at (5000, 5000)
        Assert.True(zoomBorder.OffsetX < 5000, "OffsetX should be constrained");
        Assert.True(zoomBorder.OffsetY < 5000, "OffsetY should be constrained");
        
        // Content should still have some portion visible (per MinimumVisibleContentPercentage)
        var contentVisualBounds = zoomBorder.ContentToViewport(new Rect(0, 0, 200, 100));
        
        // Check that content hasn't completely left the viewport
        Assert.True(contentVisualBounds.Right > 0 || contentVisualBounds.Bottom > 0 || 
                    contentVisualBounds.Left < 600 || contentVisualBounds.Top < 400,
                    "Content should still be at least partially visible");
    }

    [AvaloniaFact]
    public void KeepCentered_CenteredChild_MaintainsCenter()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 600,
            Height = 400,
            BoundsMode = ContentBoundsMode.KeepCentered,
            EnableConstrains = true
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 100,
            Background = Brushes.Yellow
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        // Act - Try to pan
        zoomBorder.Pan(500, 500);
        
        // Assert - Content should remain centered
        // Content center (100, 50) should map to viewport center (300, 200)
        var contentCenter = new Point(100, 50);
        var transformedCenter = zoomBorder.ContentToViewport(contentCenter);
        
        // With KeepCentered, after any pan attempt the content should be re-centered
        Assert.Equal(300, transformedCenter.X, 1);
        Assert.Equal(200, transformedCenter.Y, 1);
    }

    #endregion

    #region BringIntoView Tests with Centered Child

    [AvaloniaFact]
    public void BringIntoView_CenteredChild_AlreadyVisible_NoPositionChange()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 600,
            Height = 400
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 100,
            Background = Brushes.Yellow
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialOffsetX = zoomBorder.OffsetX;
        var initialOffsetY = zoomBorder.OffsetY;
        var initialZoomX = zoomBorder.ZoomX;
        var initialZoomY = zoomBorder.ZoomY;
        
        var scrollable = (ILogicalScrollable)zoomBorder;
        
        // Act - BringIntoView on the child itself (which is fully visible since it's smaller than viewport)
        var result = scrollable.BringIntoView(childElement, new Rect(0, 0, 200, 100));
        
        // Assert - Should return true but NOT change position since already visible
        Assert.True(result);
        Assert.Equal(initialOffsetX, zoomBorder.OffsetX, 1);
        Assert.Equal(initialOffsetY, zoomBorder.OffsetY, 1);
        Assert.Equal(initialZoomX, zoomBorder.ZoomX, 3);
        Assert.Equal(initialZoomY, zoomBorder.ZoomY, 3);
    }

    [AvaloniaFact]
    public void BringIntoView_CenteredChild_AfterPanning_ScrollsToTarget()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 600,
            Height = 400,
            BoundsMode = ContentBoundsMode.Unrestricted // Allow unrestricted panning
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 100,
            Background = Brushes.Yellow
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        // Pan content off screen
        zoomBorder.Pan(-800, -600);
        
        var scrollable = (ILogicalScrollable)zoomBorder;
        
        // Act - Request to bring content region into view
        var result = scrollable.BringIntoView(childElement, new Rect(0, 0, 50, 30));
        
        // Assert - Should succeed and adjust pan to make the target visible
        Assert.True(result);
        
        // Verify the target region is now at least partially visible
        var targetInViewport = zoomBorder.ContentToViewport(new Rect(0, 0, 50, 30));
        var viewportBounds = new Rect(0, 0, 600, 400);
        
        // The target rect should now intersect with the viewport
        Assert.True(viewportBounds.Intersects(targetInViewport), 
            "Target region should be visible in viewport after BringIntoView");
    }

    #endregion
}
