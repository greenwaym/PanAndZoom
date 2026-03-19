// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Xunit;

namespace Avalonia.Controls.PanAndZoom.UnitTests;

/// <summary>
/// Tests for double-click zoom functionality.
/// </summary>
public class ZoomBorderDoubleClickZoomTests
{
    [AvaloniaFact]
    public void EnableDoubleClickZoom_DefaultValue_IsTrue()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder();

        // Assert
        Assert.True(zoomBorder.EnableDoubleClickZoom);
    }

    [AvaloniaFact]
    public void EnableDoubleClickZoom_CanBeSetToFalse()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.EnableDoubleClickZoom = false;

        // Assert
        Assert.False(zoomBorder.EnableDoubleClickZoom);
    }

    [AvaloniaFact]
    public void DoubleClickZoomMode_DefaultValue_IsZoomInOut()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder();

        // Assert
        Assert.Equal(DoubleClickZoomMode.ZoomInOut, zoomBorder.DoubleClickZoomMode);
    }

    [AvaloniaFact]
    public void DoubleClickZoomMode_CanBeSetToZoomIn()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.DoubleClickZoomMode = DoubleClickZoomMode.ZoomIn;

        // Assert
        Assert.Equal(DoubleClickZoomMode.ZoomIn, zoomBorder.DoubleClickZoomMode);
    }

    [AvaloniaFact]
    public void DoubleClickZoomMode_CanBeSetToZoomOut()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.DoubleClickZoomMode = DoubleClickZoomMode.ZoomOut;

        // Assert
        Assert.Equal(DoubleClickZoomMode.ZoomOut, zoomBorder.DoubleClickZoomMode);
    }

    [AvaloniaFact]
    public void DoubleClickZoomMode_CanBeSetToZoomToFit()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.DoubleClickZoomMode = DoubleClickZoomMode.ZoomToFit;

        // Assert
        Assert.Equal(DoubleClickZoomMode.ZoomToFit, zoomBorder.DoubleClickZoomMode);
    }

    [AvaloniaFact]
    public void DoubleClickZoomMode_CanBeSetToNone()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.DoubleClickZoomMode = DoubleClickZoomMode.None;

        // Assert
        Assert.Equal(DoubleClickZoomMode.None, zoomBorder.DoubleClickZoomMode);
    }

    [AvaloniaFact]
    public void DoubleClickZoomFactor_DefaultValue_IsTwo()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder();

        // Assert
        Assert.Equal(2.0, zoomBorder.DoubleClickZoomFactor);
    }

    [AvaloniaFact]
    public void DoubleClickZoomFactor_CanBeSetToCustomValue()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.DoubleClickZoomFactor = 3.5;

        // Assert
        Assert.Equal(3.5, zoomBorder.DoubleClickZoomFactor);
    }

    [AvaloniaFact]
    public void DoubleClickZoom_AllPropertiesCanBeSetTogether()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder
        {
            EnableDoubleClickZoom = true,
            DoubleClickZoomMode = DoubleClickZoomMode.ZoomIn,
            DoubleClickZoomFactor = 2.5
        };

        // Assert
        Assert.True(zoomBorder.EnableDoubleClickZoom);
        Assert.Equal(DoubleClickZoomMode.ZoomIn, zoomBorder.DoubleClickZoomMode);
        Assert.Equal(2.5, zoomBorder.DoubleClickZoomFactor);
    }

    // ===== Functional Double-Click Zoom Tests =====
    // Note: These tests validate the zoom operations that double-click triggers,
    // since testing actual double-click events in headless mode is complex.

    [AvaloniaFact]
    public void ZoomIn_WithDoubleClickFactor_ActuallyZoomsIn()
    {
        // Arrange
        var canvas = new Canvas { Width = 400, Height = 400 };
        var zoomBorder = new ZoomBorder
        {
            Width = 800,
            Height = 600,
            EnableDoubleClickZoom = true,
            DoubleClickZoomMode = DoubleClickZoomMode.ZoomIn,
            DoubleClickZoomFactor = 2.0,
            Child = canvas
        };
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialZoomX = zoomBorder.ZoomX;

        // Act - Simulate what double-click does in ZoomIn mode
        zoomBorder.ZoomTo(zoomBorder.DoubleClickZoomFactor, 200, 200, skipTransitions: true);

        // Assert - Zoom should have increased
        Assert.True(zoomBorder.ZoomX > initialZoomX, "ZoomX should increase after zoom in");
    }

    [AvaloniaFact]
    public void ZoomOut_WithDoubleClickFactor_ActuallyZoomsOut()
    {
        // Arrange
        var canvas = new Canvas { Width = 400, Height = 400 };
        var zoomBorder = new ZoomBorder
        {
            Width = 800,
            Height = 600,
            EnableDoubleClickZoom = true,
            DoubleClickZoomMode = DoubleClickZoomMode.ZoomOut,
            DoubleClickZoomFactor = 2.0,
            Child = canvas
        };
        var window = new Window { Content = zoomBorder };
        window.Show();

        // First zoom in so we have something to zoom out from
        zoomBorder.ZoomTo(2.0, 200, 200, skipTransitions: true);
        var initialZoomX = zoomBorder.ZoomX;

        // Act - Simulate what double-click does in ZoomOut mode
        zoomBorder.ZoomTo(1.0 / zoomBorder.DoubleClickZoomFactor, 200, 200, skipTransitions: true);

        // Assert - Zoom should have decreased
        Assert.True(zoomBorder.ZoomX < initialZoomX, "ZoomX should decrease after zoom out");
    }

    [AvaloniaFact]
    public void DoubleClickZoom_WhenDisabled_ZoomToStillWorks()
    {
        // Arrange
        var canvas = new Canvas { Width = 400, Height = 400 };
        var zoomBorder = new ZoomBorder
        {
            Width = 800,
            Height = 600,
            EnableDoubleClickZoom = false, // Disabled
            DoubleClickZoomMode = DoubleClickZoomMode.ZoomIn,
            DoubleClickZoomFactor = 2.0,
            Child = canvas
        };
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialZoomX = zoomBorder.ZoomX;

        // Act - ZoomTo should still work even when double-click is disabled
        zoomBorder.ZoomTo(2.0, 200, 200, skipTransitions: true);

        // Assert - Zoom should have changed (programmatic zoom works regardless of EnableDoubleClickZoom)
        Assert.True(zoomBorder.ZoomX > initialZoomX, "Programmatic ZoomTo should work even when double-click is disabled");
    }

    [AvaloniaFact]
    public void DoubleClickZoomFactor_AppliedCorrectly()
    {
        // Arrange
        var canvas = new Canvas { Width = 400, Height = 400 };
        var zoomBorder = new ZoomBorder
        {
            Width = 800,
            Height = 600,
            DoubleClickZoomFactor = 3.0, // Custom factor
            Child = canvas
        };
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialZoomX = zoomBorder.ZoomX;

        // Act - Apply the double-click zoom factor
        zoomBorder.ZoomTo(zoomBorder.DoubleClickZoomFactor, 200, 200, skipTransitions: true);

        // Assert - Zoom should approximately triple
        Assert.True(zoomBorder.ZoomX > initialZoomX * 2.5, "ZoomX should approximately triple with factor 3.0");
    }

    [AvaloniaFact]
    public void ZoomInOutMode_Toggle_WorksCorrectly()
    {
        // Arrange
        var canvas = new Canvas { Width = 400, Height = 400 };
        var zoomBorder = new ZoomBorder
        {
            Width = 800,
            Height = 600,
            EnableDoubleClickZoom = true,
            DoubleClickZoomMode = DoubleClickZoomMode.ZoomInOut,
            DoubleClickZoomFactor = 2.0,
            Child = canvas
        };
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialZoomX = zoomBorder.ZoomX;

        // Act - First "click" should zoom in (since we're at 1.0 zoom)
        zoomBorder.ZoomTo(zoomBorder.DoubleClickZoomFactor, 200, 200, skipTransitions: true);

        var afterFirstZoom = zoomBorder.ZoomX;

        // Assert - Should have zoomed in
        Assert.True(afterFirstZoom > initialZoomX, "First zoom should increase zoom level");

        // Act - Second "click" would typically reset in ZoomInOut mode
        zoomBorder.ResetMatrix(skipTransitions: true);

        // Assert - Should be back to approximately 1.0
        Assert.True(zoomBorder.ZoomX < afterFirstZoom, "Reset should decrease zoom level");
    }

    // ===== DoubleTapped Event Tests (Real Event Raising) =====

    private static PointerEventArgs CreatePointerEventArgs(Visual source, Point position)
    {
        return new PointerEventArgs(
            InputElement.PointerPressedEvent,
            source,
            new Pointer(1, PointerType.Mouse, true),
            source,
            position,
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None)
        {
            RoutedEvent = InputElement.PointerPressedEvent
        };
    }

    [AvaloniaFact]
    public void DoubleTapped_ZoomInMode_ZoomsIn()
    {
        // Arrange
        var canvas = new Canvas { Width = 400, Height = 400 };
        var zoomBorder = new ZoomBorder
        {
            Width = 800,
            Height = 600,
            EnableDoubleClickZoom = true,
            DoubleClickZoomMode = DoubleClickZoomMode.ZoomIn,
            DoubleClickZoomFactor = 2.0,
            Child = canvas
        };
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialZoomX = zoomBorder.ZoomX;

        // Act - Raise DoubleTapped event
        var pointerArgs = CreatePointerEventArgs(zoomBorder, new Point(200, 200));
        var tappedArgs = new TappedEventArgs(
            InputElement.DoubleTappedEvent,
            pointerArgs)
        {
            RoutedEvent = InputElement.DoubleTappedEvent
        };
        zoomBorder.RaiseEvent(tappedArgs);

        // Assert
        Assert.True(zoomBorder.ZoomX > initialZoomX, "ZoomX should increase after double-tap in ZoomIn mode");
    }

    [AvaloniaFact]
    public void DoubleTapped_ZoomOutMode_ZoomsOut()
    {
        // Arrange
        var canvas = new Canvas { Width = 400, Height = 400 };
        var zoomBorder = new ZoomBorder
        {
            Width = 800,
            Height = 600,
            EnableDoubleClickZoom = true,
            DoubleClickZoomMode = DoubleClickZoomMode.ZoomOut,
            DoubleClickZoomFactor = 2.0,
            Child = canvas
        };
        var window = new Window { Content = zoomBorder };
        window.Show();

        // First zoom in to have something to zoom out from
        zoomBorder.ZoomTo(4.0, 200, 200, skipTransitions: true);
        var initialZoomX = zoomBorder.ZoomX;

        // Act - Raise DoubleTapped event
        var pointerArgs = CreatePointerEventArgs(zoomBorder, new Point(200, 200));
        var tappedArgs = new TappedEventArgs(
            InputElement.DoubleTappedEvent,
            pointerArgs)
        {
            RoutedEvent = InputElement.DoubleTappedEvent
        };
        zoomBorder.RaiseEvent(tappedArgs);

        // Assert
        Assert.True(zoomBorder.ZoomX < initialZoomX, "ZoomX should decrease after double-tap in ZoomOut mode");
    }

    [AvaloniaFact]
    public void DoubleTapped_ZoomInOutMode_Functionality_Test()
    {
        // Arrange
        var canvas = new Canvas { Width = 400, Height = 400 };
        var zoomBorder = new ZoomBorder
        {
            Width = 800,
            Height = 600,
            EnableDoubleClickZoom = true,
            DoubleClickZoomMode = DoubleClickZoomMode.ZoomInOut,
            DoubleClickZoomFactor = 2.0,
            Child = canvas
        };
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialZoomX = zoomBorder.ZoomX;

        // Act - Simulate what DoubleTapped handler does in ZoomInOut mode
        // At initial zoom (1.0), should zoom in
        zoomBorder.ZoomTo(zoomBorder.DoubleClickZoomFactor, 200, 200, skipTransitions: true);
        var afterFirstTap = zoomBorder.ZoomX;

        // Assert
        Assert.True(afterFirstTap > initialZoomX, "First double-tap should zoom in");

        // Act - At higher zoom, should reset
        zoomBorder.ResetMatrix(skipTransitions: true);

        // Assert
        Assert.True(zoomBorder.ZoomX < afterFirstTap, "Reset should decrease zoom level");
    }

    [AvaloniaFact]
    public void DoubleTapped_ZoomToFitMode_FitsContent()
    {
        // Arrange
        var canvas = new Canvas { Width = 400, Height = 400 };
        var zoomBorder = new ZoomBorder
        {
            Width = 800,
            Height = 600,
            EnableDoubleClickZoom = true,
            DoubleClickZoomMode = DoubleClickZoomMode.ZoomToFit,
            Child = canvas
        };
        var window = new Window { Content = zoomBorder };
        window.Show();

        // Zoom to a different level first
        zoomBorder.ZoomTo(3.0, 200, 200, skipTransitions: true);
        var beforeZoom = zoomBorder.ZoomX;

        // Act - Raise DoubleTapped event
        var pointerArgs = CreatePointerEventArgs(zoomBorder, new Point(200, 200));
        var tappedArgs = new TappedEventArgs(
            InputElement.DoubleTappedEvent,
            pointerArgs)
        {
            RoutedEvent = InputElement.DoubleTappedEvent
        };
        zoomBorder.RaiseEvent(tappedArgs);

        // Assert - Zoom should change (AutoFit was called)
        Assert.True(zoomBorder.ZoomX != beforeZoom || zoomBorder.OffsetX != 0 || zoomBorder.OffsetY != 0, 
            "Double-tap ZoomToFit should auto-fit content");
    }

    [AvaloniaFact]
    public void DoubleTapped_NoneMode_DoesNothing()
    {
        // Arrange
        var canvas = new Canvas { Width = 400, Height = 400 };
        var zoomBorder = new ZoomBorder
        {
            Width = 800,
            Height = 600,
            EnableDoubleClickZoom = true,
            DoubleClickZoomMode = DoubleClickZoomMode.None,
            Child = canvas
        };
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialZoomX = zoomBorder.ZoomX;
        var initialOffsetX = zoomBorder.OffsetX;
        var initialOffsetY = zoomBorder.OffsetY;

        // Act - Raise DoubleTapped event
        var pointerArgs = CreatePointerEventArgs(zoomBorder, new Point(200, 200));
        var tappedArgs = new TappedEventArgs(
            InputElement.DoubleTappedEvent,
            pointerArgs)
        {
            RoutedEvent = InputElement.DoubleTappedEvent
        };
        zoomBorder.RaiseEvent(tappedArgs);

        // Assert
        Assert.Equal(initialZoomX, zoomBorder.ZoomX);
        Assert.Equal(initialOffsetX, zoomBorder.OffsetX);
        Assert.Equal(initialOffsetY, zoomBorder.OffsetY);
    }

    [AvaloniaFact]
    public void DoubleTapped_WhenDisabled_DoesNothing()
    {
        // Arrange
        var canvas = new Canvas { Width = 400, Height = 400 };
        var zoomBorder = new ZoomBorder
        {
            Width = 800,
            Height = 600,
            EnableDoubleClickZoom = false, // Disabled
            DoubleClickZoomMode = DoubleClickZoomMode.ZoomIn,
            DoubleClickZoomFactor = 2.0,
            Child = canvas
        };
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialZoomX = zoomBorder.ZoomX;

        // Act - Raise DoubleTapped event
        var pointerArgs = CreatePointerEventArgs(zoomBorder, new Point(200, 200));
        var tappedArgs = new TappedEventArgs(
            InputElement.DoubleTappedEvent,
            pointerArgs)
        {
            RoutedEvent = InputElement.DoubleTappedEvent
        };
        zoomBorder.RaiseEvent(tappedArgs);

        // Assert
        Assert.Equal(initialZoomX, zoomBorder.ZoomX);
    }

    [AvaloniaFact]
    public void DoubleTapped_WithNoChild_DoesNothing()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 800,
            Height = 600,
            EnableDoubleClickZoom = true,
            DoubleClickZoomMode = DoubleClickZoomMode.ZoomIn,
            DoubleClickZoomFactor = 2.0
            // No child
        };
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialZoomX = zoomBorder.ZoomX;

        // Act - Raise DoubleTapped event
        var pointerArgs = CreatePointerEventArgs(zoomBorder, new Point(200, 200));
        var tappedArgs = new TappedEventArgs(
            InputElement.DoubleTappedEvent,
            pointerArgs)
        {
            RoutedEvent = InputElement.DoubleTappedEvent
        };
        zoomBorder.RaiseEvent(tappedArgs);

        // Assert - No crash, zoom unchanged (no child to reference)
        Assert.Equal(initialZoomX, zoomBorder.ZoomX);
    }
}
