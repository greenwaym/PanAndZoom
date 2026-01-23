// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Xunit;

namespace Avalonia.Controls.PanAndZoom.UnitTests;

/// <summary>
/// Tests for mouse wheel behavior functionality.
/// </summary>
public class ZoomBorderWheelBehaviorTests
{
    [AvaloniaFact]
    public void WheelBehavior_DefaultValue_IsZoom()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder();

        // Assert
        Assert.Equal(WheelBehaviorMode.Zoom, zoomBorder.WheelBehavior);
    }

    [AvaloniaFact]
    public void WheelBehavior_CanBeSetToZoom()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.WheelBehavior = WheelBehaviorMode.Zoom;

        // Assert
        Assert.Equal(WheelBehaviorMode.Zoom, zoomBorder.WheelBehavior);
    }

    [AvaloniaFact]
    public void WheelBehavior_CanBeSetToPanVertical()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.WheelBehavior = WheelBehaviorMode.PanVertical;

        // Assert
        Assert.Equal(WheelBehaviorMode.PanVertical, zoomBorder.WheelBehavior);
    }

    [AvaloniaFact]
    public void WheelBehavior_CanBeSetToPanHorizontal()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.WheelBehavior = WheelBehaviorMode.PanHorizontal;

        // Assert
        Assert.Equal(WheelBehaviorMode.PanHorizontal, zoomBorder.WheelBehavior);
    }

    [AvaloniaFact]
    public void WheelBehavior_CanBeSetToNone()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.WheelBehavior = WheelBehaviorMode.None;

        // Assert
        Assert.Equal(WheelBehaviorMode.None, zoomBorder.WheelBehavior);
    }

    [AvaloniaFact]
    public void WheelWithCtrl_DefaultValue_IsZoom()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder();

        // Assert
        Assert.Equal(WheelBehaviorMode.Zoom, zoomBorder.WheelWithCtrl);
    }

    [AvaloniaFact]
    public void WheelWithCtrl_CanBeChanged()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.WheelWithCtrl = WheelBehaviorMode.PanVertical;

        // Assert
        Assert.Equal(WheelBehaviorMode.PanVertical, zoomBorder.WheelWithCtrl);
    }

    [AvaloniaFact]
    public void WheelWithShift_DefaultValue_IsPanHorizontal()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder();

        // Assert
        Assert.Equal(WheelBehaviorMode.PanHorizontal, zoomBorder.WheelWithShift);
    }

    [AvaloniaFact]
    public void WheelWithShift_CanBeChanged()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.WheelWithShift = WheelBehaviorMode.Zoom;

        // Assert
        Assert.Equal(WheelBehaviorMode.Zoom, zoomBorder.WheelWithShift);
    }

    [AvaloniaFact]
    public void WheelZoomSensitivity_DefaultValue_IsOne()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder();

        // Assert
        Assert.Equal(1.0, zoomBorder.WheelZoomSensitivity);
    }

    [AvaloniaFact]
    public void WheelZoomSensitivity_CanBeSetToCustomValue()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.WheelZoomSensitivity = 2.5;

        // Assert
        Assert.Equal(2.5, zoomBorder.WheelZoomSensitivity);
    }

    [AvaloniaFact]
    public void WheelPanSensitivity_DefaultValue_IsOne()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder();

        // Assert
        Assert.Equal(1.0, zoomBorder.WheelPanSensitivity);
    }

    [AvaloniaFact]
    public void WheelPanSensitivity_CanBeSetToCustomValue()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.WheelPanSensitivity = 1.5;

        // Assert
        Assert.Equal(1.5, zoomBorder.WheelPanSensitivity);
    }

    [AvaloniaFact]
    public void WheelBehavior_AllPropertiesCanBeSetTogether()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder
        {
            WheelBehavior = WheelBehaviorMode.PanVertical,
            WheelWithCtrl = WheelBehaviorMode.Zoom,
            WheelWithShift = WheelBehaviorMode.PanHorizontal,
            WheelZoomSensitivity = 2.0,
            WheelPanSensitivity = 1.5
        };

        // Assert
        Assert.Equal(WheelBehaviorMode.PanVertical, zoomBorder.WheelBehavior);
        Assert.Equal(WheelBehaviorMode.Zoom, zoomBorder.WheelWithCtrl);
        Assert.Equal(WheelBehaviorMode.PanHorizontal, zoomBorder.WheelWithShift);
        Assert.Equal(2.0, zoomBorder.WheelZoomSensitivity);
        Assert.Equal(1.5, zoomBorder.WheelPanSensitivity);
    }

    [AvaloniaFact]
    public void WheelBehavior_Zoom_ZoomsInOnWheelScroll()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            WheelBehavior = WheelBehaviorMode.Zoom,
            EnableZoom = true
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 150,
            Background = Brushes.Red
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialZoom = zoomBorder.ZoomX;

        // Act - Simulate wheel scroll up (zoom in)
        var wheelEventArgs = new PointerWheelEventArgs(
            zoomBorder,
            new Pointer(1, PointerType.Mouse, true),
            window,
            new Point(200, 150),
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
            KeyModifiers.None,
            new Vector(0, 1) // Scroll up to zoom in
        )
        {
            RoutedEvent = InputElement.PointerWheelChangedEvent
        };

        zoomBorder.RaiseEvent(wheelEventArgs);

        // Assert
        Assert.True(zoomBorder.ZoomX > initialZoom, "Zoom should increase on wheel scroll up");
    }

    [AvaloniaFact]
    public void WheelBehavior_PanVertical_PansVerticallyOnWheelScroll()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            WheelBehavior = WheelBehaviorMode.PanVertical,
            EnablePan = true
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 150,
            Background = Brushes.Red
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialOffsetY = zoomBorder.OffsetY;

        // Act - Simulate wheel scroll
        var wheelEventArgs = new PointerWheelEventArgs(
            zoomBorder,
            new Pointer(1, PointerType.Mouse, true),
            window,
            new Point(200, 150),
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
            KeyModifiers.None,
            new Vector(0, 1)
        )
        {
            RoutedEvent = InputElement.PointerWheelChangedEvent
        };

        zoomBorder.RaiseEvent(wheelEventArgs);

        // Assert
        Assert.True(zoomBorder.OffsetY != initialOffsetY, "Should pan vertically on wheel scroll");
    }

    [AvaloniaFact]
    public void WheelBehavior_WithCtrlModifier_UsesCtrlBehaviorInsteadOfDefault()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            WheelBehavior = WheelBehaviorMode.PanVertical,
            WheelWithCtrl = WheelBehaviorMode.Zoom,
            EnableZoom = true,
            EnablePan = true
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 150,
            Background = Brushes.Red
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialZoom = zoomBorder.ZoomX;

        // Act - Wheel with Ctrl modifier
        var wheelEventArgs = new PointerWheelEventArgs(
            zoomBorder,
            new Pointer(1, PointerType.Mouse, true),
            window,
            new Point(200, 150),
            0,
            new PointerPointProperties(RawInputModifiers.Control, PointerUpdateKind.Other),
            KeyModifiers.Control,
            new Vector(0, 1)
        )
        {
            RoutedEvent = InputElement.PointerWheelChangedEvent
        };

        zoomBorder.RaiseEvent(wheelEventArgs);

        // Assert - Should zoom (WheelWithCtrl) instead of pan (WheelBehavior)
        Assert.True(zoomBorder.ZoomX > initialZoom, "Should zoom when Ctrl is held");
    }

    [AvaloniaFact]
    public void WheelBehavior_WithShiftModifier_UsesShiftBehaviorInsteadOfDefault()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            WheelBehavior = WheelBehaviorMode.Zoom,
            WheelWithShift = WheelBehaviorMode.PanHorizontal,
            EnableZoom = true,
            EnablePan = true
        };

        var childElement = new Border
        {
            Width = 200,
            Height = 150,
            Background = Brushes.Red
        };

        zoomBorder.Child = childElement;
        var window = new Window { Content = zoomBorder };
        window.Show();

        var initialOffsetX = zoomBorder.OffsetX;

        // Act - Wheel with Shift modifier
        var wheelEventArgs = new PointerWheelEventArgs(
            zoomBorder,
            new Pointer(1, PointerType.Mouse, true),
            window,
            new Point(200, 150),
            0,
            new PointerPointProperties(RawInputModifiers.Shift, PointerUpdateKind.Other),
            KeyModifiers.Shift,
            new Vector(0, 1)
        )
        {
            RoutedEvent = InputElement.PointerWheelChangedEvent
        };

        zoomBorder.RaiseEvent(wheelEventArgs);

        // Assert - Should pan horizontally (WheelWithShift) instead of zoom (WheelBehavior)
        Assert.True(zoomBorder.OffsetX != initialOffsetX, "Should pan horizontally when Shift is held");
    }

    [AvaloniaFact]
    public void WheelZoomSensitivity_HigherValue_ProducesMoreZoom()
    {
        // Arrange
        var zoomBorder1 = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            WheelBehavior = WheelBehaviorMode.Zoom,
            WheelZoomSensitivity = 1.0,
            EnableZoom = true
        };

        var zoomBorder2 = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            WheelBehavior = WheelBehaviorMode.Zoom,
            WheelZoomSensitivity = 2.0, // Double sensitivity
            EnableZoom = true
        };

        var child1 = new Border { Width = 200, Height = 150, Background = Brushes.Red };
        var child2 = new Border { Width = 200, Height = 150, Background = Brushes.Blue };

        zoomBorder1.Child = child1;
        zoomBorder2.Child = child2;

        var window1 = new Window { Content = zoomBorder1 };
        var window2 = new Window { Content = zoomBorder2 };
        window1.Show();
        window2.Show();

        // Act - Same wheel event to both
        var wheelEventArgs1 = new PointerWheelEventArgs(
            zoomBorder1,
            new Pointer(1, PointerType.Mouse, true),
            window1,
            new Point(200, 150),
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
            KeyModifiers.None,
            new Vector(0, 1)
        )
        {
            RoutedEvent = InputElement.PointerWheelChangedEvent
        };

        var wheelEventArgs2 = new PointerWheelEventArgs(
            zoomBorder2,
            new Pointer(1, PointerType.Mouse, true),
            window2,
            new Point(200, 150),
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
            KeyModifiers.None,
            new Vector(0, 1)
        )
        {
            RoutedEvent = InputElement.PointerWheelChangedEvent
        };

        zoomBorder1.RaiseEvent(wheelEventArgs1);
        zoomBorder2.RaiseEvent(wheelEventArgs2);

        // Assert - Higher sensitivity should result in more zoom
        Assert.True(zoomBorder2.ZoomX > zoomBorder1.ZoomX, "Higher sensitivity should zoom more");
    }
}
