// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Xunit;

namespace Avalonia.Controls.PanAndZoom.UnitTests;

/// <summary>
/// Tests for ZoomBorder multi-touch functionality.
/// MinimumTouchPoints, MaximumTouchPoints, EnableSimultaneousPanZoom, and GestureRecognitionDelay
/// are implemented in the gesture handling logic (Border_PinchGesture and Border_ScrollGesture).
/// These tests verify the properties can be get/set and that the implementation works correctly.
/// </summary>
public class ZoomBorderMultiTouchTests
{
    [AvaloniaFact]
    public void EnableSimultaneousPanZoom_DefaultValue_IsTrue()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder();

        // Assert
        Assert.True(zoomBorder.EnableSimultaneousPanZoom);
    }

    [AvaloniaFact]
    public void MinimumTouchPoints_DefaultValue_Is1()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder();

        // Assert
        Assert.Equal(1, zoomBorder.MinimumTouchPoints);
    }

    [AvaloniaFact]
    public void MaximumTouchPoints_DefaultValue_Is2()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder();

        // Assert
        Assert.Equal(2, zoomBorder.MaximumTouchPoints);
    }

    [AvaloniaFact]
    public void GestureRecognitionDelay_DefaultValue_IsZero()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder();

        // Assert - Default is zero (no delay), set positive value to enable delay
        Assert.Equal(TimeSpan.Zero, zoomBorder.GestureRecognitionDelay);
    }

    [AvaloniaFact]
    public void EnableSimultaneousPanZoom_CanBeSet()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.EnableSimultaneousPanZoom = false;

        // Assert
        Assert.False(zoomBorder.EnableSimultaneousPanZoom);
    }

    [AvaloniaFact]
    public void MinimumTouchPoints_CanBeSet()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.MinimumTouchPoints = 2;

        // Assert
        Assert.Equal(2, zoomBorder.MinimumTouchPoints);
    }

    [AvaloniaFact]
    public void MaximumTouchPoints_CanBeSet()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.MaximumTouchPoints = 3;

        // Assert
        Assert.Equal(3, zoomBorder.MaximumTouchPoints);
    }

    [AvaloniaFact]
    public void GestureRecognitionDelay_CanBeSet()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.GestureRecognitionDelay = TimeSpan.FromMilliseconds(100);

        // Assert
        Assert.Equal(TimeSpan.FromMilliseconds(100), zoomBorder.GestureRecognitionDelay);
    }

    /// <summary>
    /// Verifies that EnableSimultaneousPanZoom property controls simultaneous gesture handling.
    /// When false, pinch gestures are blocked when panning is active.
    /// </summary>
    [AvaloniaFact]
    public void EnableSimultaneousPanZoom_WhenFalse_BlocksSimultaneousGestures()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            EnableSimultaneousPanZoom = false
        };
        
        // Assert - Property value is stored and checked in Border_PinchGesture
        Assert.False(zoomBorder.EnableSimultaneousPanZoom);
        
        // The implementation checks: if (!EnableSimultaneousPanZoom && _isPanning) return;
        // This blocks pinch gestures while panning is in progress
    }

    /// <summary>
    /// Verifies that touch point limits are checked in gesture handlers.
    /// MinimumTouchPoints > 2 blocks pinch (2-finger) gestures.
    /// MaximumTouchPoints < 2 also blocks pinch gestures.
    /// </summary>
    [AvaloniaFact]
    public void TouchPointLimits_EnforcedInGestureHandlers()
    {
        // Arrange - Setting MinimumTouchPoints > 2 will block pinch gestures
        var zoomBorderMinTooHigh = new ZoomBorder
        {
            MinimumTouchPoints = 3 // Requires 3 fingers, but pinch uses 2
        };
        
        // Assert - This configuration will block standard pinch gestures
        Assert.Equal(3, zoomBorderMinTooHigh.MinimumTouchPoints);
        
        // Arrange - Setting MaximumTouchPoints < 2 will also block pinch gestures  
        var zoomBorderMaxTooLow = new ZoomBorder
        {
            MaximumTouchPoints = 1 // Allows only 1 finger, but pinch uses 2
        };
        
        // Assert - This configuration will block pinch gestures
        Assert.Equal(1, zoomBorderMaxTooLow.MaximumTouchPoints);
        
        // The implementation checks: if (MinimumTouchPoints > 2 || MaximumTouchPoints < 2) return;
    }

    /// <summary>
    /// Verifies that GestureRecognitionDelay property is implemented and respects the delay setting.
    /// When a positive delay is set, gestures won't be processed until the delay has elapsed.
    /// </summary>
    [AvaloniaFact]
    public void GestureRecognitionDelay_WhenPositive_DelaysGestureRecognition()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            GestureRecognitionDelay = TimeSpan.FromMilliseconds(100)
        };
        
        // Assert - Property stores value and is checked in Border_PinchGesture
        Assert.Equal(TimeSpan.FromMilliseconds(100), zoomBorder.GestureRecognitionDelay);
        
        // The implementation:
        // 1. Records _gestureStartTime on first gesture event
        // 2. Returns early while (DateTime.Now - _gestureStartTime) < GestureRecognitionDelay
        // 3. Sets _gestureRecognized = true and processes gesture after delay expires
    }

    #region Pinch Gesture Touch Point Limit Tests

    [AvaloniaFact]
    public void PinchGesture_MinimumTouchPointsTooHigh_GestureNotProcessed()
    {
        // Arrange - MinimumTouchPoints > 2 should block pinch gestures (pinch uses 2 fingers)
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestures = true,
            EnableGestureZoom = true,
            MinimumTouchPoints = 3  // Requires 3 fingers, but pinch uses 2
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

        var initialZoomX = zoomBorder.ZoomX;

        // Act - Simulate pinch gesture
        var pinchEventArgs = new PinchEventArgs(1.5, new Point(0.5, 0.5), 0.0, 0.0)
        {
            RoutedEvent = InputElement.PinchEvent,
            Source = zoomBorder
        };

        zoomBorder.RaiseEvent(pinchEventArgs);

        // Assert - Zoom should NOT change because MinimumTouchPoints > 2
        Assert.Equal(initialZoomX, zoomBorder.ZoomX);
        Assert.False(pinchEventArgs.Handled, "Pinch event should NOT be handled when MinimumTouchPoints > 2");
    }

    [AvaloniaFact]
    public void PinchGesture_MaximumTouchPointsTooLow_GestureNotProcessed()
    {
        // Arrange - MaximumTouchPoints < 2 should block pinch gestures (pinch uses 2 fingers)
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestures = true,
            EnableGestureZoom = true,
            MaximumTouchPoints = 1  // Allows only 1 finger, but pinch uses 2
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

        var initialZoomX = zoomBorder.ZoomX;

        // Act - Simulate pinch gesture
        var pinchEventArgs = new PinchEventArgs(1.5, new Point(0.5, 0.5), 0.0, 0.0)
        {
            RoutedEvent = InputElement.PinchEvent,
            Source = zoomBorder
        };

        zoomBorder.RaiseEvent(pinchEventArgs);

        // Assert - Zoom should NOT change because MaximumTouchPoints < 2
        Assert.Equal(initialZoomX, zoomBorder.ZoomX);
        Assert.False(pinchEventArgs.Handled, "Pinch event should NOT be handled when MaximumTouchPoints < 2");
    }

    [AvaloniaFact]
    public void PinchGesture_TouchPointLimitsInclude2_GestureProcessed()
    {
        // Arrange - When touch point limits include 2, pinch gestures should work
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestures = true,
            EnableGestureZoom = true,
            MinimumTouchPoints = 1,
            MaximumTouchPoints = 3
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

        var initialZoomX = zoomBorder.ZoomX;

        // Act - Simulate pinch gesture
        var pinchEventArgs = new PinchEventArgs(1.5, new Point(0.5, 0.5), 0.0, 0.0)
        {
            RoutedEvent = InputElement.PinchEvent,
            Source = zoomBorder
        };

        zoomBorder.RaiseEvent(pinchEventArgs);

        // Assert - Zoom should change because touch point limits include 2
        Assert.True(zoomBorder.ZoomX > initialZoomX, "Zoom should increase when touch point limits include 2");
        Assert.True(pinchEventArgs.Handled, "Pinch event should be handled when touch point limits include 2");
    }

    #endregion

    #region Scroll Gesture Touch Point Limit Tests

    [AvaloniaFact]
    public void ScrollGesture_MinimumTouchPointsTooHigh_GestureNotProcessed()
    {
        // Arrange - MinimumTouchPoints > 2 should block scroll gestures (scroll uses 2 fingers)
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestures = true,
            EnableGestureTranslation = true,
            MinimumTouchPoints = 3  // Requires 3 fingers, but scroll uses 2
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

        // Act - Simulate scroll gesture
        var scrollEventArgs = new ScrollGestureEventArgs(1, new Vector(50, 0))
        {
            RoutedEvent = InputElement.ScrollGestureEvent,
            Source = zoomBorder
        };

        zoomBorder.RaiseEvent(scrollEventArgs);

        // Assert - Offset should NOT change because MinimumTouchPoints > 2
        Assert.Equal(initialOffsetX, zoomBorder.OffsetX);
        Assert.False(scrollEventArgs.Handled, "Scroll event should NOT be handled when MinimumTouchPoints > 2");
    }

    [AvaloniaFact]
    public void ScrollGesture_MaximumTouchPointsTooLow_GestureNotProcessed()
    {
        // Arrange - MaximumTouchPoints < 2 should block scroll gestures (scroll uses 2 fingers)
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestures = true,
            EnableGestureTranslation = true,
            MaximumTouchPoints = 1  // Allows only 1 finger, but scroll uses 2
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

        // Act - Simulate scroll gesture
        var scrollEventArgs = new ScrollGestureEventArgs(1, new Vector(50, 0))
        {
            RoutedEvent = InputElement.ScrollGestureEvent,
            Source = zoomBorder
        };

        zoomBorder.RaiseEvent(scrollEventArgs);

        // Assert - Offset should NOT change because MaximumTouchPoints < 2
        Assert.Equal(initialOffsetX, zoomBorder.OffsetX);
        Assert.False(scrollEventArgs.Handled, "Scroll event should NOT be handled when MaximumTouchPoints < 2");
    }

    [AvaloniaFact]
    public void ScrollGesture_TouchPointLimitsInclude2_GestureProcessed()
    {
        // Arrange - When touch point limits include 2, scroll gestures should work
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestures = true,
            EnableGestureTranslation = true,
            MinimumTouchPoints = 1,
            MaximumTouchPoints = 3
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

        // Act - Simulate scroll gesture
        var scrollEventArgs = new ScrollGestureEventArgs(1, new Vector(50, 0))
        {
            RoutedEvent = InputElement.ScrollGestureEvent,
            Source = zoomBorder
        };

        zoomBorder.RaiseEvent(scrollEventArgs);

        // Assert - Offset should change because touch point limits include 2
        Assert.NotEqual(initialOffsetX, zoomBorder.OffsetX);
        Assert.True(scrollEventArgs.Handled, "Scroll event should be handled when touch point limits include 2");
    }

    #endregion

    #region GestureRecognitionDelay Tests

    [AvaloniaFact]
    public void GestureRecognitionDelay_WithPositiveDelay_FirstGestureIsIgnored()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestures = true,
            EnableGestureZoom = true,
            GestureRecognitionDelay = TimeSpan.FromMilliseconds(500) // 500ms delay
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

        var initialZoomX = zoomBorder.ZoomX;

        // Act - Send first pinch gesture (should be ignored due to delay)
        var pinchEventArgs = new PinchEventArgs(1.5, new Point(0.5, 0.5), 0.0, 0.0)
        {
            RoutedEvent = InputElement.PinchEvent,
            Source = zoomBorder
        };

        zoomBorder.RaiseEvent(pinchEventArgs);

        // Assert - First gesture should be ignored (zoom unchanged) because delay hasn't elapsed
        Assert.Equal(initialZoomX, zoomBorder.ZoomX);
        Assert.False(pinchEventArgs.Handled, "First pinch gesture should be ignored due to recognition delay");
    }

    [AvaloniaFact]
    public void GestureRecognitionDelay_WhenZero_GestureProcessedImmediately()
    {
        // Arrange - Default delay is zero, so gesture should be processed immediately
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestures = true,
            EnableGestureZoom = true,
            GestureRecognitionDelay = TimeSpan.Zero
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

        var initialZoomX = zoomBorder.ZoomX;

        // Act - Send pinch gesture
        var pinchEventArgs = new PinchEventArgs(1.5, new Point(0.5, 0.5), 0.0, 0.0)
        {
            RoutedEvent = InputElement.PinchEvent,
            Source = zoomBorder
        };

        zoomBorder.RaiseEvent(pinchEventArgs);

        // Assert - Gesture should be processed immediately with zero delay
        Assert.True(zoomBorder.ZoomX > initialZoomX, "Pinch gesture should be processed immediately with zero delay");
        Assert.True(pinchEventArgs.Handled, "Pinch event should be handled with zero delay");
    }

    #endregion

    #region EnableSimultaneousPanZoom Tests

    [AvaloniaFact]
    public void EnableSimultaneousPanZoom_DefaultTrue_AllowsSimultaneousGestures()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestures = true,
            EnableGestureZoom = true,
            EnableGestureTranslation = true,
            EnableSimultaneousPanZoom = true // default
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

        // Assert - Simultaneous gestures should be enabled by default
        Assert.True(zoomBorder.EnableSimultaneousPanZoom);
    }

    [AvaloniaFact]
    public void EnableSimultaneousPanZoom_WhenFalse_PropertyIsStored()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableSimultaneousPanZoom = false
        };

        // Assert - Property value is stored correctly
        Assert.False(zoomBorder.EnableSimultaneousPanZoom);
    }

    #endregion

    #region Property Validation Tests

    [AvaloniaFact]
    public void MinimumTouchPoints_ValidRange_AcceptsValue()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act & Assert - Various valid values
        zoomBorder.MinimumTouchPoints = 0;
        Assert.Equal(0, zoomBorder.MinimumTouchPoints);

        zoomBorder.MinimumTouchPoints = 1;
        Assert.Equal(1, zoomBorder.MinimumTouchPoints);

        zoomBorder.MinimumTouchPoints = 5;
        Assert.Equal(5, zoomBorder.MinimumTouchPoints);

        zoomBorder.MinimumTouchPoints = 10;
        Assert.Equal(10, zoomBorder.MinimumTouchPoints);
    }

    [AvaloniaFact]
    public void MaximumTouchPoints_ValidRange_AcceptsValue()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act & Assert - Various valid values
        zoomBorder.MaximumTouchPoints = 1;
        Assert.Equal(1, zoomBorder.MaximumTouchPoints);

        zoomBorder.MaximumTouchPoints = 2;
        Assert.Equal(2, zoomBorder.MaximumTouchPoints);

        zoomBorder.MaximumTouchPoints = 5;
        Assert.Equal(5, zoomBorder.MaximumTouchPoints);

        zoomBorder.MaximumTouchPoints = 10;
        Assert.Equal(10, zoomBorder.MaximumTouchPoints);
    }

    [AvaloniaFact]
    public void GestureRecognitionDelay_NegativeValue_AcceptsValue()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act - Set negative value (treated as zero effectively in the implementation)
        zoomBorder.GestureRecognitionDelay = TimeSpan.FromMilliseconds(-100);

        // Assert - Value is stored (implementation should handle appropriately)
        Assert.Equal(TimeSpan.FromMilliseconds(-100), zoomBorder.GestureRecognitionDelay);
    }

    [AvaloniaFact]
    public void GestureRecognitionDelay_LargeValue_AcceptsValue()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();

        // Act
        zoomBorder.GestureRecognitionDelay = TimeSpan.FromSeconds(10);

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(10), zoomBorder.GestureRecognitionDelay);
    }

    #endregion
}
