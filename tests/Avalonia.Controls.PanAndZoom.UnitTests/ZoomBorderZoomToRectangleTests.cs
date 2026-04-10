// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Xunit;

namespace Avalonia.Controls.PanAndZoom.UnitTests;

/// <summary>
/// Tests for ZoomBorder zoom to rectangle functionality.
/// </summary>
public class ZoomBorderZoomToRectangleTests
{
    [AvaloniaFact]
    public void ZoomToRectangle_ZoomsToFitRectangle()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300
        };

        var childElement = new Border
        {
            Width = 800,
            Height = 600,
            Background = Brushes.Red
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        var rect = new Rect(100, 100, 200, 150);

        // Act
        zoomBorder.ZoomToRectangle(rect);

        // Assert - Should zoom to fit the rectangle
        Assert.True(zoomBorder.ZoomX >= 1.0, "Should zoom in to fit rectangle");
    }

    [AvaloniaFact]
    public void ZoomToRectangleExact_ZoomsToSpecificViewportRect()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300
        };

        var childElement = new Border
        {
            Width = 800,
            Height = 600,
            Background = Brushes.Red
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        var contentRect = new Rect(100, 100, 200, 150);
        var viewportRect = new Rect(50, 50, 300, 200);

        // Act
        zoomBorder.ZoomToRectangleExact(contentRect, viewportRect);

        // Assert
        Assert.True(zoomBorder.ZoomX > 1.0);
    }

    [AvaloniaFact]
    public void ZoomToRectangle_CentersRectangleInViewport()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 600,
            Height = 400,
            Stretch = StretchMode.None
        };

        var childElement = new Canvas
        {
            Width = 800,
            Height = 600,
            Background = Brushes.LightGray
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        // Red region coordinates
        var rect = new Rect(50, 100, 200, 150);
        
        // Expected calculations:
        // Layout offset: Border always centers its child, so layoutOffset = ((600-800)/2, (400-600)/2) = (-100, -100)
        // zoom = min(600/200, 400/150) = min(3, 2.667) = 2.667
        // rectCenterX = 50 + 100 = 150
        // rectCenterY = 100 + 75 = 175
        // viewportCenterX = 300
        // viewportCenterY = 200
        // The visual position formula is: visualPos = layoutOffset + matrixOffset + contentPoint * zoom
        // So: matrixOffset = viewportCenter - layoutOffset - contentPoint * zoom
        // offsetX = 300 - (-100) - 150 * 2.667 = 400 - 400 = 0
        // offsetY = 200 - (-100) - 175 * 2.667 = 300 - 466.67 = -166.67

        // Act
        zoomBorder.ZoomToRectangle(rect);

        // Assert - verify the rectangle center maps to viewport center
        var expectedZoom = Math.Min(600.0 / 200.0, 400.0 / 150.0);
        var rectCenterX = rect.X + rect.Width / 2.0;
        var rectCenterY = rect.Y + rect.Height / 2.0;
        
        // Layout offset where Avalonia centers the child within the ZoomBorder
        var layoutOffsetX = (600.0 - 800.0) / 2.0;  // -100
        var layoutOffsetY = (400.0 - 600.0) / 2.0;  // -100
        
        // Matrix offset = viewportCenter - layoutOffset - contentCenter * zoom
        var expectedOffsetX = 300.0 - layoutOffsetX - rectCenterX * expectedZoom;
        var expectedOffsetY = 200.0 - layoutOffsetY - rectCenterY * expectedZoom;

        Assert.Equal(expectedZoom, zoomBorder.ZoomX, 2);
        Assert.Equal(expectedOffsetX, zoomBorder.OffsetX, 2);
        Assert.Equal(expectedOffsetY, zoomBorder.OffsetY, 2);
        
        // Verify that the rectangle center point transforms to viewport center
        // Visual position = layoutOffset + matrixOffset + contentPoint * zoom
        var transformedCenterX = layoutOffsetX + rectCenterX * zoomBorder.ZoomX + zoomBorder.OffsetX;
        var transformedCenterY = layoutOffsetY + rectCenterY * zoomBorder.ZoomY + zoomBorder.OffsetY;
        
        Assert.Equal(300.0, transformedCenterX, 1);
        Assert.Equal(200.0, transformedCenterY, 1);
    }
}
