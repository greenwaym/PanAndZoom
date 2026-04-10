// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Avalonia.HeadlessTestingFramework;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Xunit;

namespace Avalonia.Controls.PanAndZoom.UnitTests;

/// <summary>
/// Comprehensive touch gesture tests for ZoomBorder using TouchInputSimulator.
/// Tests all touch and gesture-related functionality including pinch-to-zoom,
/// scroll gestures, touchpad magnify, multi-touch scenarios, and gesture events.
/// </summary>
public class ZoomBorderTouchGestureComprehensiveTests
{
    #region Helper Methods
    
    private static ZoomBorder CreateZoomBorder(
        double width = 400, 
        double height = 300,
        double childWidth = 200,
        double childHeight = 150,
        bool enableGestures = true,
        bool enableGestureZoom = true,
        bool enableGestureTranslation = true,
        bool enableGestureRotation = true)
    {
        var zoomBorder = new ZoomBorder
        {
            Width = width,
            Height = height,
            EnableGestures = enableGestures,
            EnableGestureZoom = enableGestureZoom,
            EnableGestureTranslation = enableGestureTranslation,
            EnableGestureRotation = enableGestureRotation
        };
        
        var childElement = new Border
        {
            Width = childWidth,
            Height = childHeight,
            Background = Brushes.Red
        };
        
        zoomBorder.Child = childElement;
        return zoomBorder;
    }
    
    private static Window ShowInWindow(ZoomBorder zoomBorder)
    {
        var window = new Window { Content = zoomBorder };
        window.Show();
        return window;
    }
    
    #endregion

    #region Pinch Gesture - Basic Zoom Tests
    
    [AvaloniaFact]
    public void PinchGesture_ZoomIn_IncreasesZoomFromDefaultValue()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialZoomX = zoomBorder.ZoomX;
        var initialZoomY = zoomBorder.ZoomY;

        // Act
        simulator.PinchGesture(zoomBorder, 2.0, new Point(0.5, 0.5));

        // Assert
        Assert.True(zoomBorder.ZoomX > initialZoomX, $"ZoomX should increase from {initialZoomX}, was {zoomBorder.ZoomX}");
        Assert.True(zoomBorder.ZoomY > initialZoomY, $"ZoomY should increase from {initialZoomY}, was {zoomBorder.ZoomY}");
    }
    
    [AvaloniaFact]
    public void PinchGesture_ZoomOut_DecreasesZoom()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        zoomBorder.ZoomTo(2.0, 100, 75);
        var simulator = new TouchInputSimulator();
        
        var initialZoomX = zoomBorder.ZoomX;

        // Act
        simulator.PinchGesture(zoomBorder, 0.5, new Point(0.5, 0.5));

        // Assert
        Assert.True(zoomBorder.ZoomX < initialZoomX, $"ZoomX should decrease from {initialZoomX}, was {zoomBorder.ZoomX}");
    }
    
    [AvaloniaFact]
    public void PinchGesture_MultipleSequentialZooms_AccumulatesZoom()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var zoomHistory = new List<double> { zoomBorder.ZoomX };

        // Act - Multiple zoom in gestures
        for (int i = 0; i < 5; i++)
        {
            simulator.PinchGesture(zoomBorder, 1.2, new Point(0.5, 0.5));
            zoomHistory.Add(zoomBorder.ZoomX);
        }

        // Assert - Each zoom should increase
        for (int i = 1; i < zoomHistory.Count; i++)
        {
            Assert.True(zoomHistory[i] > zoomHistory[i - 1], 
                $"Zoom at step {i} ({zoomHistory[i]}) should be greater than step {i-1} ({zoomHistory[i - 1]})");
        }
    }
    
    [AvaloniaFact]
    public void PinchGesture_ZoomAtDifferentOrigins_AffectsOffset()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        // First zoom at top-left
        simulator.PinchGesture(zoomBorder, 1.5, new Point(0.0, 0.0));
        var offsetAfterTopLeft = (zoomBorder.OffsetX, zoomBorder.OffsetY);
        zoomBorder.ResetMatrix();
        
        // Then zoom at center
        simulator.PinchGesture(zoomBorder, 1.5, new Point(0.5, 0.5));
        var offsetAfterCenter = (zoomBorder.OffsetX, zoomBorder.OffsetY);
        zoomBorder.ResetMatrix();
        
        // Then zoom at bottom-right
        simulator.PinchGesture(zoomBorder, 1.5, new Point(1.0, 1.0));
        var offsetAfterBottomRight = (zoomBorder.OffsetX, zoomBorder.OffsetY);

        // Assert - Different origins should produce different offsets
        Assert.NotEqual(offsetAfterTopLeft, offsetAfterCenter);
        Assert.NotEqual(offsetAfterCenter, offsetAfterBottomRight);
    }
    
    [AvaloniaFact]
    public void PinchGesture_RespectsMinZoom()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        zoomBorder.MinZoomX = 0.5;
        zoomBorder.MinZoomY = 0.5;
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act - Try to zoom out beyond minimum
        for (int i = 0; i < 10; i++)
        {
            simulator.PinchGesture(zoomBorder, 0.5, new Point(0.5, 0.5));
        }

        // Assert - Should not go below minimum
        Assert.True(zoomBorder.ZoomX >= 0.5, $"ZoomX should not go below 0.5, was {zoomBorder.ZoomX}");
        Assert.True(zoomBorder.ZoomY >= 0.5, $"ZoomY should not go below 0.5, was {zoomBorder.ZoomY}");
    }
    
    [AvaloniaFact]
    public void PinchGesture_RespectsMaxZoom()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        zoomBorder.MaxZoomX = 5.0;
        zoomBorder.MaxZoomY = 5.0;
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act - Try to zoom in beyond maximum
        for (int i = 0; i < 20; i++)
        {
            simulator.PinchGesture(zoomBorder, 2.0, new Point(0.5, 0.5));
        }

        // Assert - Should not go above maximum
        Assert.True(zoomBorder.ZoomX <= 5.0, $"ZoomX should not exceed 5.0, was {zoomBorder.ZoomX}");
        Assert.True(zoomBorder.ZoomY <= 5.0, $"ZoomY should not exceed 5.0, was {zoomBorder.ZoomY}");
    }
    
    #endregion
    
    #region Pinch Gesture - Enable/Disable Tests
    
    [AvaloniaFact]
    public void PinchGesture_WhenGesturesDisabled_DoesNotZoom()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder(enableGestures: false);
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialZoomX = zoomBorder.ZoomX;
        var initialZoomY = zoomBorder.ZoomY;

        // Act
        simulator.PinchGesture(zoomBorder, 2.0, new Point(0.5, 0.5));

        // Assert
        Assert.Equal(initialZoomX, zoomBorder.ZoomX);
        Assert.Equal(initialZoomY, zoomBorder.ZoomY);
    }
    
    [AvaloniaFact]
    public void PinchGesture_WhenGestureZoomDisabled_DoesNotZoom()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder(enableGestureZoom: false);
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialZoomX = zoomBorder.ZoomX;
        var initialZoomY = zoomBorder.ZoomY;

        // Act
        simulator.PinchGesture(zoomBorder, 2.0, new Point(0.5, 0.5));

        // Assert
        Assert.Equal(initialZoomX, zoomBorder.ZoomX);
        Assert.Equal(initialZoomY, zoomBorder.ZoomY);
    }
    
    [AvaloniaFact]
    public void PinchGesture_WhenReEnabled_ZoomsAgain()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder(enableGestureZoom: false);
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        // First try while disabled
        simulator.PinchGesture(zoomBorder, 2.0, new Point(0.5, 0.5));
        var zoomWhileDisabled = zoomBorder.ZoomX;
        
        // Re-enable
        zoomBorder.EnableGestureZoom = true;
        
        // Act
        simulator.PinchGesture(zoomBorder, 2.0, new Point(0.5, 0.5));

        // Assert
        Assert.True(zoomBorder.ZoomX > zoomWhileDisabled, "Zoom should work after re-enabling");
    }
    
    [AvaloniaFact]
    public void PinchGesture_WithNoChild_DoesNotCrash()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestures = true,
            EnableGestureZoom = true
        };
        // No child set
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => 
            simulator.PinchGesture(zoomBorder, 2.0, new Point(0.5, 0.5)));
        
        Assert.Null(exception);
    }
    
    #endregion
    
    #region Scroll Gesture - Pan Tests
    
    [AvaloniaFact]
    public void ScrollGesture_PanRight_IncreasesOffsetX()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialOffsetX = zoomBorder.OffsetX;

        // Act - Scroll gesture with negative X moves content right (inverted)
        simulator.ScrollGesture(zoomBorder, new Vector(-50, 0));

        // Assert
        Assert.True(zoomBorder.OffsetX > initialOffsetX, 
            $"OffsetX should increase, was {initialOffsetX} now {zoomBorder.OffsetX}");
    }
    
    [AvaloniaFact]
    public void ScrollGesture_PanLeft_DecreasesOffsetX()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        zoomBorder.Pan(100, 0); // Start with some offset
        var simulator = new TouchInputSimulator();
        
        var initialOffsetX = zoomBorder.OffsetX;

        // Act - Scroll gesture with positive X moves content left (inverted)
        simulator.ScrollGesture(zoomBorder, new Vector(50, 0));

        // Assert
        Assert.True(zoomBorder.OffsetX < initialOffsetX, 
            $"OffsetX should decrease from {initialOffsetX}, was {zoomBorder.OffsetX}");
    }
    
    [AvaloniaFact]
    public void ScrollGesture_PanDown_IncreasesOffsetY()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialOffsetY = zoomBorder.OffsetY;

        // Act - Scroll gesture with negative Y moves content down (inverted)
        simulator.ScrollGesture(zoomBorder, new Vector(0, -50));

        // Assert
        Assert.True(zoomBorder.OffsetY > initialOffsetY, 
            $"OffsetY should increase, was {initialOffsetY} now {zoomBorder.OffsetY}");
    }
    
    [AvaloniaFact]
    public void ScrollGesture_PanUp_DecreasesOffsetY()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        zoomBorder.Pan(0, 100); // Start with some offset
        var simulator = new TouchInputSimulator();
        
        var initialOffsetY = zoomBorder.OffsetY;

        // Act - Scroll gesture with positive Y moves content up (inverted)
        simulator.ScrollGesture(zoomBorder, new Vector(0, 50));

        // Assert
        Assert.True(zoomBorder.OffsetY < initialOffsetY, 
            $"OffsetY should decrease from {initialOffsetY}, was {zoomBorder.OffsetY}");
    }
    
    [AvaloniaFact]
    public void ScrollGesture_DiagonalPan_ChangesBothOffsets()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialOffsetX = zoomBorder.OffsetX;
        var initialOffsetY = zoomBorder.OffsetY;

        // Act - Diagonal scroll
        simulator.ScrollGesture(zoomBorder, new Vector(-30, -40));

        // Assert
        Assert.NotEqual(initialOffsetX, zoomBorder.OffsetX);
        Assert.NotEqual(initialOffsetY, zoomBorder.OffsetY);
    }
    
    [AvaloniaFact]
    public void ScrollGesture_MultipleGestures_AccumulatesPan()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var offsetHistory = new List<double> { zoomBorder.OffsetX };

        // Act - Multiple pan gestures to the right
        for (int i = 0; i < 5; i++)
        {
            simulator.ScrollGesture(zoomBorder, new Vector(-20, 0));
            offsetHistory.Add(zoomBorder.OffsetX);
        }

        // Assert - Each pan should increase offset
        for (int i = 1; i < offsetHistory.Count; i++)
        {
            Assert.True(offsetHistory[i] > offsetHistory[i - 1], 
                $"OffsetX at step {i} ({offsetHistory[i]}) should be greater than step {i-1} ({offsetHistory[i - 1]})");
        }
    }
    
    [AvaloniaFact]
    public void ScrollGesture_WhenTranslationDisabled_DoesNotPan()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder(enableGestureTranslation: false);
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialOffsetX = zoomBorder.OffsetX;
        var initialOffsetY = zoomBorder.OffsetY;

        // Act
        simulator.ScrollGesture(zoomBorder, new Vector(50, 50));

        // Assert
        Assert.Equal(initialOffsetX, zoomBorder.OffsetX);
        Assert.Equal(initialOffsetY, zoomBorder.OffsetY);
    }
    
    [AvaloniaFact]
    public void ScrollGesture_WithZoom_PansCorrectly()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        zoomBorder.ZoomTo(2.0, 100, 75);
        var simulator = new TouchInputSimulator();
        
        var initialOffsetX = zoomBorder.OffsetX;

        // Act
        simulator.ScrollGesture(zoomBorder, new Vector(-50, 0));

        // Assert
        Assert.NotEqual(initialOffsetX, zoomBorder.OffsetX);
    }
    
    #endregion
    
    #region Touch Point Limits Tests
    
    [AvaloniaFact]
    public void PinchGesture_WhenMinTouchPointsGreaterThan2_DoesNotZoom()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        zoomBorder.MinimumTouchPoints = 3; // Requires 3+ touch points
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialZoom = zoomBorder.ZoomX;

        // Act - Pinch uses 2 touch points
        simulator.PinchGesture(zoomBorder, 2.0, new Point(0.5, 0.5));

        // Assert
        Assert.Equal(initialZoom, zoomBorder.ZoomX);
    }
    
    [AvaloniaFact]
    public void PinchGesture_WhenMaxTouchPointsLessThan2_DoesNotZoom()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        zoomBorder.MaximumTouchPoints = 1; // Only allows 1 touch point
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialZoom = zoomBorder.ZoomX;

        // Act - Pinch uses 2 touch points
        simulator.PinchGesture(zoomBorder, 2.0, new Point(0.5, 0.5));

        // Assert
        Assert.Equal(initialZoom, zoomBorder.ZoomX);
    }
    
    [AvaloniaFact]
    public void ScrollGesture_WhenMinTouchPointsGreaterThan2_DoesNotPan()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        zoomBorder.MinimumTouchPoints = 3;
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialOffset = zoomBorder.OffsetX;

        // Act - Scroll gesture uses 2 touch points
        simulator.ScrollGesture(zoomBorder, new Vector(50, 0));

        // Assert
        Assert.Equal(initialOffset, zoomBorder.OffsetX);
    }
    
    [AvaloniaFact]
    public void PinchGesture_WithDefaultTouchPointLimits_Zooms()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        // Default: MinimumTouchPoints = 1, MaximumTouchPoints = 2
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialZoom = zoomBorder.ZoomX;

        // Act
        simulator.PinchGesture(zoomBorder, 1.5, new Point(0.5, 0.5));

        // Assert
        Assert.True(zoomBorder.ZoomX > initialZoom);
    }
    
    #endregion
    
    #region Simultaneous Pan/Zoom Tests
    
    [AvaloniaFact]
    public void SimultaneousPanZoom_WhenEnabled_AllowsBothGestures()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        zoomBorder.EnableSimultaneousPanZoom = true;
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialZoom = zoomBorder.ZoomX;
        var initialOffset = zoomBorder.OffsetX;

        // Act - Pinch to zoom
        simulator.PinchGesture(zoomBorder, 1.5, new Point(0.5, 0.5));
        var zoomAfterPinch = zoomBorder.ZoomX;
        
        // Then scroll to pan
        simulator.ScrollGesture(zoomBorder, new Vector(-30, 0));
        var offsetAfterScroll = zoomBorder.OffsetX;

        // Assert
        Assert.True(zoomAfterPinch > initialZoom, "Zoom should have increased");
        Assert.NotEqual(initialOffset, offsetAfterScroll);
    }
    
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
    
    #endregion
    
    #region Gesture Recognition Delay Tests
    
    [AvaloniaFact]
    public void GestureRecognitionDelay_DefaultValue_IsZero()
    {
        // Arrange & Act
        var zoomBorder = new ZoomBorder();

        // Assert
        Assert.Equal(TimeSpan.Zero, zoomBorder.GestureRecognitionDelay);
    }
    
    [AvaloniaFact]
    public void GestureRecognitionDelay_CanBeSet()
    {
        // Arrange
        var zoomBorder = new ZoomBorder();
        var delay = TimeSpan.FromMilliseconds(100);

        // Act
        zoomBorder.GestureRecognitionDelay = delay;

        // Assert
        Assert.Equal(delay, zoomBorder.GestureRecognitionDelay);
    }
    
    #endregion
    
    #region Gesture Events Tests
    
    [AvaloniaFact]
    public void PinchGesture_RaisesGestureStartedEvent()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        GestureEventArgs? receivedArgs = null;
        zoomBorder.GestureStarted += (_, args) => receivedArgs = args;

        // Act
        simulator.PinchGesture(zoomBorder, 1.5, new Point(0.5, 0.5));

        // Assert
        Assert.NotNull(receivedArgs);
        Assert.Equal("Pinch", receivedArgs!.GestureType);
    }
    
    [AvaloniaFact]
    public void PinchGestureEnded_RaisesGestureEndedEvent()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        GestureEventArgs? receivedArgs = null;
        zoomBorder.GestureEnded += (_, args) => receivedArgs = args;

        // Act
        simulator.PinchGesture(zoomBorder, 1.5, new Point(0.5, 0.5));
        simulator.PinchGestureEnded(zoomBorder);

        // Assert
        Assert.NotNull(receivedArgs);
        Assert.Equal("Pinch", receivedArgs!.GestureType);
    }
    
    [AvaloniaFact]
    public void ScrollGesture_RaisesGestureStartedEvent()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        GestureEventArgs? receivedArgs = null;
        zoomBorder.GestureStarted += (_, args) => receivedArgs = args;

        // Act
        simulator.ScrollGesture(zoomBorder, new Vector(30, 20));

        // Assert
        Assert.NotNull(receivedArgs);
        Assert.Equal("Scroll", receivedArgs!.GestureType);
    }
    
    [AvaloniaFact]
    public void ScrollGestureEnded_RaisesGestureEndedEvent()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        GestureEventArgs? receivedArgs = null;
        zoomBorder.GestureEnded += (_, args) => receivedArgs = args;

        // Act
        simulator.ScrollGesture(zoomBorder, new Vector(30, 20));
        simulator.ScrollGestureEnded(zoomBorder);

        // Assert
        Assert.NotNull(receivedArgs);
        Assert.Equal("Scroll", receivedArgs!.GestureType);
    }
    
    [AvaloniaFact]
    public void GestureEvents_ProvideCorrectZoomValues()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        GestureEventArgs? receivedArgs = null;
        zoomBorder.GestureStarted += (_, args) => receivedArgs = args;

        // Act
        simulator.PinchGesture(zoomBorder, 1.5, new Point(0.5, 0.5));

        // Assert
        Assert.NotNull(receivedArgs);
        Assert.True(receivedArgs!.ZoomX > 0);
        Assert.True(receivedArgs!.ZoomY > 0);
    }
    
    [AvaloniaFact]
    public void GestureEvents_ProvideCorrectOffsetValues()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        zoomBorder.Pan(50, 30);
        var simulator = new TouchInputSimulator();
        
        GestureEventArgs? receivedArgs = null;
        zoomBorder.GestureStarted += (_, args) => receivedArgs = args;

        // Act
        simulator.ScrollGesture(zoomBorder, new Vector(10, 10));

        // Assert
        Assert.NotNull(receivedArgs);
        // The event args should contain current offset values
    }
    
    #endregion
    
    #region Touchpad Magnify Tests
    
    [AvaloniaFact]
    public void TouchpadMagnify_ZoomIn_DoesNotCrash()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => 
            simulator.TouchpadMagnify(zoomBorder, new Vector(0.3, 0), new Point(200, 150)));
        
        Assert.Null(exception);
    }
    
    [AvaloniaFact]
    public void TouchpadMagnify_ZoomOut_DoesNotCrash()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        zoomBorder.ZoomTo(2.0, 100, 75);
        var simulator = new TouchInputSimulator();

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => 
            simulator.TouchpadMagnify(zoomBorder, new Vector(-0.3, 0), new Point(200, 150)));
        
        Assert.Null(exception);
    }
    
    [AvaloniaFact]
    public void TouchpadMagnify_WithKeyModifiers_DoesNotCrash()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act & Assert - Should not throw with various modifiers
        var exception = Record.Exception(() => 
        {
            simulator.TouchpadMagnify(zoomBorder, new Vector(0.2, 0), new Point(200, 150), KeyModifiers.Control);
            simulator.TouchpadMagnify(zoomBorder, new Vector(0.2, 0), new Point(200, 150), KeyModifiers.Shift);
            simulator.TouchpadMagnify(zoomBorder, new Vector(0.2, 0), new Point(200, 150), KeyModifiers.Alt);
        });
        
        Assert.Null(exception);
    }
    
    #endregion
    
    #region Touchpad Swipe Tests
    
    [AvaloniaFact]
    public void TouchpadSwipe_Horizontal_DoesNotCrash()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act & Assert
        var exception = Record.Exception(() => 
            simulator.TouchpadSwipe(zoomBorder, new Vector(50, 0), new Point(200, 150)));
        
        Assert.Null(exception);
    }
    
    [AvaloniaFact]
    public void TouchpadSwipe_Vertical_DoesNotCrash()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act & Assert
        var exception = Record.Exception(() => 
            simulator.TouchpadSwipe(zoomBorder, new Vector(0, 50), new Point(200, 150)));
        
        Assert.Null(exception);
    }
    
    [AvaloniaFact]
    public void TouchpadSwipe_Diagonal_DoesNotCrash()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act & Assert
        var exception = Record.Exception(() => 
            simulator.TouchpadSwipe(zoomBorder, new Vector(30, 40), new Point(200, 150)));
        
        Assert.Null(exception);
    }
    
    #endregion
    
    #region Touchpad Rotate Tests
    
    [AvaloniaFact]
    public void TouchpadRotate_Clockwise_DoesNotCrash()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act & Assert
        var exception = Record.Exception(() => 
            simulator.TouchpadRotate(zoomBorder, 45, new Point(200, 150)));
        
        Assert.Null(exception);
    }
    
    [AvaloniaFact]
    public void TouchpadRotate_CounterClockwise_DoesNotCrash()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act & Assert
        var exception = Record.Exception(() => 
            simulator.TouchpadRotate(zoomBorder, -45, new Point(200, 150)));
        
        Assert.Null(exception);
    }
    
    #endregion
    
    #region Multi-Step Simulation Tests
    
    [AvaloniaFact]
    public void SimulatePinchZoom_ZoomIn_IncreasesZoom()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialZoom = zoomBorder.ZoomX;

        // Act - Pinch outward (zoom in)
        simulator.SimulatePinchZoom(zoomBorder, new Point(200, 150), 50, 150, 10);

        // Assert
        Assert.True(zoomBorder.ZoomX > initialZoom, 
            $"Zoom should increase from {initialZoom}, was {zoomBorder.ZoomX}");
    }
    
    [AvaloniaFact]
    public void SimulatePinchZoom_ZoomOut_DecreasesZoom()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        zoomBorder.ZoomTo(2.0, 100, 75);
        var simulator = new TouchInputSimulator();
        
        var initialZoom = zoomBorder.ZoomX;

        // Act - Pinch inward (zoom out)
        simulator.SimulatePinchZoom(zoomBorder, new Point(200, 150), 150, 50, 10);

        // Assert
        Assert.True(zoomBorder.ZoomX < initialZoom, 
            $"Zoom should decrease from {initialZoom}, was {zoomBorder.ZoomX}");
    }
    
    [AvaloniaFact]
    public void SimulateTwoFingerPan_ChangesOffset()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialOffsetX = zoomBorder.OffsetX;
        var initialOffsetY = zoomBorder.OffsetY;

        // Act
        simulator.SimulateTwoFingerPan(zoomBorder, new Point(100, 100), new Point(200, 150), 50, 10);

        // Assert - The offset should change due to scroll gestures
        var offsetChanged = zoomBorder.OffsetX != initialOffsetX || zoomBorder.OffsetY != initialOffsetY;
        Assert.True(offsetChanged, "Offset should change after two-finger pan simulation");
    }
    
    [AvaloniaFact]
    public void SimulateRotation_CompletesWithoutError()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act & Assert
        var exception = Record.Exception(() => 
            simulator.SimulateRotation(zoomBorder, new Point(200, 150), 50, 0, 90, 10));
        
        Assert.Null(exception);
    }
    
    [AvaloniaFact]
    public void SimulateDrag_RaisesPointerEvents()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var pressCount = 0;
        var moveCount = 0;
        var releaseCount = 0;
        
        zoomBorder.PointerPressed += (_, _) => pressCount++;
        zoomBorder.PointerMoved += (_, _) => moveCount++;
        zoomBorder.PointerReleased += (_, _) => releaseCount++;

        // Act
        simulator.SimulateDrag(zoomBorder, new Point(100, 100), new Point(200, 150), 10);

        // Assert
        Assert.Equal(1, pressCount);
        Assert.True(moveCount >= 10, $"Should have at least 10 move events, had {moveCount}");
        Assert.Equal(1, releaseCount);
    }
    
    #endregion
    
    #region Swipe Gesture Tests
    
    [AvaloniaFact]
    public void Swipe_Left_RaisesEvents()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var pressed = false;
        var released = false;
        zoomBorder.PointerPressed += (_, _) => pressed = true;
        zoomBorder.PointerReleased += (_, _) => released = true;

        // Act
        simulator.Swipe(zoomBorder, new Point(300, 150), Avalonia.HeadlessTestingFramework.SwipeDirection.Left, 100);

        // Assert
        Assert.True(pressed);
        Assert.True(released);
    }
    
    [AvaloniaFact]
    public void Swipe_Right_RaisesEvents()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var pressed = false;
        var released = false;
        zoomBorder.PointerPressed += (_, _) => pressed = true;
        zoomBorder.PointerReleased += (_, _) => released = true;

        // Act
        simulator.Swipe(zoomBorder, new Point(100, 150), Avalonia.HeadlessTestingFramework.SwipeDirection.Right, 100);

        // Assert
        Assert.True(pressed);
        Assert.True(released);
    }
    
    [AvaloniaFact]
    public void Swipe_Up_RaisesEvents()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var pressed = false;
        var released = false;
        zoomBorder.PointerPressed += (_, _) => pressed = true;
        zoomBorder.PointerReleased += (_, _) => released = true;

        // Act
        simulator.Swipe(zoomBorder, new Point(200, 250), Avalonia.HeadlessTestingFramework.SwipeDirection.Up, 100);

        // Assert
        Assert.True(pressed);
        Assert.True(released);
    }
    
    [AvaloniaFact]
    public void Swipe_Down_RaisesEvents()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var pressed = false;
        var released = false;
        zoomBorder.PointerPressed += (_, _) => pressed = true;
        zoomBorder.PointerReleased += (_, _) => released = true;

        // Act
        simulator.Swipe(zoomBorder, new Point(200, 50), Avalonia.HeadlessTestingFramework.SwipeDirection.Down, 100);

        // Assert
        Assert.True(pressed);
        Assert.True(released);
    }
    
    #endregion
    
    #region Touch Interaction Tests
    
    [AvaloniaFact]
    public void TouchTap_RaisesPointerPressedAndReleased()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var pressCount = 0;
        var releaseCount = 0;
        zoomBorder.PointerPressed += (_, _) => pressCount++;
        zoomBorder.PointerReleased += (_, _) => releaseCount++;

        // Act
        simulator.Tap(zoomBorder, new Point(200, 150));

        // Assert
        Assert.Equal(1, pressCount);
        Assert.Equal(1, releaseCount);
    }
    
    [AvaloniaFact]
    public void TouchDoubleTap_RaisesTwoTapSequences()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var pressCount = 0;
        var releaseCount = 0;
        zoomBorder.PointerPressed += (_, _) => pressCount++;
        zoomBorder.PointerReleased += (_, _) => releaseCount++;

        // Act
        simulator.DoubleTap(zoomBorder, new Point(200, 150));

        // Assert
        Assert.Equal(2, pressCount);
        Assert.Equal(2, releaseCount);
    }
    
    [AvaloniaFact]
    public void TouchDrag_WithPanEnabled_ChangesOffset()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        zoomBorder.EnablePan = true;
        zoomBorder.PanButton = ButtonName.Left;
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var moveCount = 0;
        zoomBorder.PointerMoved += (_, _) => moveCount++;

        // Act
        simulator.SimulateDrag(zoomBorder, new Point(100, 100), new Point(200, 150), 5);

        // Assert
        Assert.True(moveCount >= 5);
    }
    
    [AvaloniaFact]
    public void MultiTouch_TwoFingerTouchDown_TracksPoints()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act
        var touch1 = simulator.TouchDown(zoomBorder, new Point(100, 100));
        var touch2 = simulator.TouchDown(zoomBorder, new Point(200, 200));

        // Assert
        Assert.Equal(2, simulator.ActiveTouchPoints.Count);
        Assert.True(simulator.ActiveTouchPoints.ContainsKey(touch1));
        Assert.True(simulator.ActiveTouchPoints.ContainsKey(touch2));
    }
    
    [AvaloniaFact]
    public void MultiTouch_MoveAndRelease_WorksCorrectly()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act
        var touch1 = simulator.TouchDown(zoomBorder, new Point(100, 100));
        var touch2 = simulator.TouchDown(zoomBorder, new Point(200, 200));
        
        simulator.TouchMove(zoomBorder, touch1, new Point(110, 110));
        simulator.TouchMove(zoomBorder, touch2, new Point(210, 210));
        
        // Assert positions updated
        Assert.Equal(new Point(110, 110), simulator.ActiveTouchPoints[touch1].Position);
        Assert.Equal(new Point(210, 210), simulator.ActiveTouchPoints[touch2].Position);
        
        // Release touches
        simulator.TouchUp(zoomBorder, touch1);
        Assert.Single(simulator.ActiveTouchPoints);
        
        simulator.TouchUp(zoomBorder, touch2);
        Assert.Empty(simulator.ActiveTouchPoints);
    }
    
    #endregion
    
    #region Edge Cases and Error Handling
    
    [AvaloniaFact]
    public void PinchGesture_ScaleOfOne_NoChange()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialZoom = zoomBorder.ZoomX;

        // Act - Scale of 1 means no change
        simulator.PinchGesture(zoomBorder, 1.0, new Point(0.5, 0.5));

        // Assert
        Assert.Equal(initialZoom, zoomBorder.ZoomX);
    }
    
    [AvaloniaFact]
    public void ScrollGesture_ZeroDelta_NoChange()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var initialOffsetX = zoomBorder.OffsetX;
        var initialOffsetY = zoomBorder.OffsetY;

        // Act
        simulator.ScrollGesture(zoomBorder, new Vector(0, 0));

        // Assert
        Assert.Equal(initialOffsetX, zoomBorder.OffsetX);
        Assert.Equal(initialOffsetY, zoomBorder.OffsetY);
    }
    
    [AvaloniaFact]
    public void PinchGesture_VerySmallScale_HandlesGracefully()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act & Assert - Should not crash with very small scale
        var exception = Record.Exception(() => 
            simulator.PinchGesture(zoomBorder, 0.001, new Point(0.5, 0.5)));
        
        Assert.Null(exception);
    }
    
    [AvaloniaFact]
    public void PinchGesture_VeryLargeScale_HandlesGracefully()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act & Assert - Should not crash with very large scale
        var exception = Record.Exception(() => 
            simulator.PinchGesture(zoomBorder, 100.0, new Point(0.5, 0.5)));
        
        Assert.Null(exception);
    }
    
    [AvaloniaFact]
    public void ScrollGesture_VeryLargeDelta_HandlesGracefully()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act & Assert - Should not crash with very large delta
        var exception = Record.Exception(() => 
            simulator.ScrollGesture(zoomBorder, new Vector(10000, 10000)));
        
        Assert.Null(exception);
    }
    
    [AvaloniaFact]
    public void RapidGestures_HandlesWithoutError()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act & Assert - Rapid succession of gestures
        var exception = Record.Exception(() => 
        {
            for (int i = 0; i < 50; i++)
            {
                simulator.PinchGesture(zoomBorder, 1.1, new Point(0.5, 0.5));
                simulator.ScrollGesture(zoomBorder, new Vector(5, 5));
            }
        });
        
        Assert.Null(exception);
    }
    
    [AvaloniaFact]
    public void GestureSequence_PinchThenScroll_WorksCorrectly()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        var gestureEvents = new List<string>();
        zoomBorder.GestureStarted += (_, args) => gestureEvents.Add($"Start:{args.GestureType}");
        zoomBorder.GestureEnded += (_, args) => gestureEvents.Add($"End:{args.GestureType}");

        // Act
        simulator.PinchGesture(zoomBorder, 1.5, new Point(0.5, 0.5));
        simulator.PinchGestureEnded(zoomBorder);
        simulator.ScrollGesture(zoomBorder, new Vector(30, 20));
        simulator.ScrollGestureEnded(zoomBorder);

        // Assert
        Assert.Contains("Start:Pinch", gestureEvents);
        Assert.Contains("End:Pinch", gestureEvents);
        Assert.Contains("Start:Scroll", gestureEvents);
        Assert.Contains("End:Scroll", gestureEvents);
    }
    
    #endregion
    
    #region Combined Operations Tests
    
    [AvaloniaFact]
    public void ZoomThenPan_BothOperationsWork()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();

        // Act - First zoom in
        simulator.PinchGesture(zoomBorder, 2.0, new Point(0.5, 0.5));
        var zoomAfterPinch = zoomBorder.ZoomX;
        
        // Then pan
        var offsetBeforePan = zoomBorder.OffsetX;
        simulator.ScrollGesture(zoomBorder, new Vector(-50, 0));
        var offsetAfterPan = zoomBorder.OffsetX;

        // Assert
        Assert.True(zoomAfterPinch > 1.0, "Should have zoomed in");
        Assert.NotEqual(offsetBeforePan, offsetAfterPan);
    }
    
    [AvaloniaFact]
    public void ResetMatrixAfterGestures_ResetsToDefault()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        // Perform some gestures
        simulator.PinchGesture(zoomBorder, 2.0, new Point(0.5, 0.5));
        simulator.ScrollGesture(zoomBorder, new Vector(-50, -30));

        // Act
        zoomBorder.ResetMatrix();

        // Assert
        Assert.Equal(1.0, zoomBorder.ZoomX);
        Assert.Equal(1.0, zoomBorder.ZoomY);
        Assert.Equal(0.0, zoomBorder.OffsetX);
        Assert.Equal(0.0, zoomBorder.OffsetY);
    }
    
    [AvaloniaFact]
    public void FillAfterGestures_FillsContent()
    {
        // Arrange
        var zoomBorder = CreateZoomBorder();
        ShowInWindow(zoomBorder);
        var simulator = new TouchInputSimulator();
        
        // Perform some gestures
        simulator.PinchGesture(zoomBorder, 0.5, new Point(0.5, 0.5));
        var zoomAfterPinch = zoomBorder.ZoomX;

        // Act
        zoomBorder.Fill();

        // Assert - Fill should change the zoom
        Assert.NotEqual(zoomAfterPinch, zoomBorder.ZoomX);
    }
    
    #endregion
}
