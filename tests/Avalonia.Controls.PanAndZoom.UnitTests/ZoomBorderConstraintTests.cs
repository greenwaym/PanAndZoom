using System;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Xunit;

namespace Avalonia.Controls.PanAndZoom.UnitTests;

public class ZoomBorderConstraintTests
{
    [AvaloniaFact]
    public void PointerWheel_ExceedsMaxZoom_ClampedToMaximum()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            MaxZoomX = 2.0,
            MaxZoomY = 2.0
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
        
        // Act - Try to zoom beyond maximum with large wheel delta
        var wheelEventArgs = new PointerWheelEventArgs(
            zoomBorder,
            new Pointer(1, PointerType.Mouse, true),
            zoomBorder,
            new Point(200, 150),
            0,
            new PointerPointProperties(),
            KeyModifiers.None,
            new Vector(0, 10))
        {
            RoutedEvent = InputElement.PointerWheelChangedEvent
        };
        
        zoomBorder.RaiseEvent(wheelEventArgs);
        
        // Assert
        Assert.True(zoomBorder.ZoomX <= 2.0, "ZoomX should not exceed maximum");
        Assert.True(zoomBorder.ZoomY <= 2.0, "ZoomY should not exceed maximum");
        Assert.True(wheelEventArgs.Handled, "Wheel event should be handled");
    }
    
    [AvaloniaFact]
    public void PointerWheel_BelowMinZoom_ClampedToMinimum()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            MinZoomX = 0.5,
            MinZoomY = 0.5
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
        
        // Act - Try to zoom below minimum with large negative wheel delta
        var wheelEventArgs = new PointerWheelEventArgs(
            zoomBorder,
            new Pointer(1, PointerType.Mouse, true),
            zoomBorder,
            new Point(200, 150),
            0,
            new PointerPointProperties(),
            KeyModifiers.None,
            new Vector(0, -10))
        {
            RoutedEvent = InputElement.PointerWheelChangedEvent
        };
        
        zoomBorder.RaiseEvent(wheelEventArgs);
        
        // Assert
        Assert.True(zoomBorder.ZoomX >= 0.5, "ZoomX should not go below minimum");
        Assert.True(zoomBorder.ZoomY >= 0.5, "ZoomY should not go below minimum");
        Assert.True(wheelEventArgs.Handled, "Wheel event should be handled");
    }
    
    [AvaloniaFact]
    public void PinchGesture_ExceedsMaxZoom_ClampedToMaximum()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestures = true,
            EnableGestureZoom = true,
            MaxZoomX = 3.0,
            MaxZoomY = 3.0
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
        
        // First zoom close to maximum
        zoomBorder.ZoomTo(2.8, 100, 75);
        
        // Act - Try to zoom beyond maximum with pinch gesture
        var pinchEventArgs = new PinchEventArgs(2.0, new Point(0.5, 0.5), 0.0, 0.0)
        {
            RoutedEvent = InputElement.PinchEvent,
            Source = zoomBorder
        };
        
        zoomBorder.RaiseEvent(pinchEventArgs);
        
        // Assert
        Assert.True(zoomBorder.ZoomX <= 3.0, "ZoomX should not exceed maximum");
        Assert.True(zoomBorder.ZoomY <= 3.0, "ZoomY should not exceed maximum");
        Assert.True(pinchEventArgs.Handled, "Pinch event should be handled");
    }
    
    [AvaloniaFact]
    public void PinchGesture_BelowMinZoom_ClampedToMinimum()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestures = true,
            EnableGestureZoom = true,
            MinZoomX = 0.2,
            MinZoomY = 0.2
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
        
        // First zoom close to minimum
        zoomBorder.ZoomTo(0.3, 100, 75);
        
        // Act - Try to zoom below minimum with pinch gesture
        var pinchEventArgs = new PinchEventArgs(0.1, new Point(0.5, 0.5), 0.0, 0.0)
        {
            RoutedEvent = InputElement.PinchEvent,
            Source = zoomBorder
        };
        
        zoomBorder.RaiseEvent(pinchEventArgs);
        
        // Assert
        Assert.True(zoomBorder.ZoomX >= 0.2, "ZoomX should not go below minimum");
        Assert.True(zoomBorder.ZoomY >= 0.2, "ZoomY should not go below minimum");
        Assert.True(pinchEventArgs.Handled, "Pinch event should be handled");
    }
    
    [AvaloniaFact]
    public void Pan_WithOffsetLimits_RespectsBoundaries()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnablePan = true,
            PanButton = ButtonName.Left,
            MinOffsetX = -100,
            MaxOffsetX = 100,
            MinOffsetY = -75,
            MaxOffsetY = 75
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
        
        // Set offset close to maximum
        zoomBorder.Pan(95, 70);
        
        // Act - Try to pan beyond limits
        var pointerPressedEventArgs = new PointerPressedEventArgs(
            zoomBorder,
            new Pointer(1, PointerType.Mouse, true),
            zoomBorder,
            new Point(200, 150),
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None)
        {
            RoutedEvent = InputElement.PointerPressedEvent
        };
        
        zoomBorder.RaiseEvent(pointerPressedEventArgs);
        
        // Simulate large movement that would exceed limits
        var pointerMovedEventArgs = new PointerEventArgs(
            InputElement.PointerMovedEvent,
            zoomBorder,
            new Pointer(1, PointerType.Mouse, true),
            zoomBorder,
            new Point(300, 200),
            0,
            new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.Other),
            KeyModifiers.None);
        
        zoomBorder.RaiseEvent(pointerMovedEventArgs);
        
        // Assert
        Assert.True(zoomBorder.OffsetX <= 100, "OffsetX should not exceed maximum");
        Assert.True(zoomBorder.OffsetY <= 75, "OffsetY should not exceed maximum");
        Assert.True(zoomBorder.OffsetX >= -100, "OffsetX should not go below minimum");
        Assert.True(zoomBorder.OffsetY >= -75, "OffsetY should not go below minimum");
    }
    
    [AvaloniaFact]
    public void ScrollGesture_WithOffsetLimits_RespectsBoundaries()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestures = true,
            EnableGestureTranslation = true,
            MinOffsetX = -50,
            MaxOffsetX = 50,
            MinOffsetY = -50,
            MaxOffsetY = 50
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
        
        // Set offset close to limits
        zoomBorder.Pan(45, 45);
        
        // Act - Try to scroll beyond limits
        var scrollEventArgs = new ScrollGestureEventArgs(1, new Vector(100, 100))
        {
            RoutedEvent = InputElement.ScrollGestureEvent,
            Source = zoomBorder
        };
        
        zoomBorder.RaiseEvent(scrollEventArgs);
        
        // Assert
        Assert.True(zoomBorder.OffsetX <= 50, "OffsetX should not exceed maximum");
        Assert.True(zoomBorder.OffsetY <= 50, "OffsetY should not exceed maximum");
        Assert.True(scrollEventArgs.Handled, "Scroll gesture should be handled");
    }
    
    [AvaloniaFact]
    public void ZoomConstraints_DifferentXAndY_HandledIndependently()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            MinZoomX = 0.5,
            MaxZoomX = 2.0,
            MinZoomY = 1.0,
            MaxZoomY = 4.0
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
        
        // Act - Try to zoom beyond different limits for X and Y
        var wheelEventArgs = new PointerWheelEventArgs(
            zoomBorder,
            new Pointer(1, PointerType.Mouse, true),
            zoomBorder,
            new Point(200, 150),
            0,
            new PointerPointProperties(),
            KeyModifiers.None,
            new Vector(0, 10))
        {
            RoutedEvent = InputElement.PointerWheelChangedEvent
        };
        
        zoomBorder.RaiseEvent(wheelEventArgs);
        
        // Assert
        Assert.True(zoomBorder.ZoomX <= 2.0, "ZoomX should respect its maximum");
        Assert.True(zoomBorder.ZoomY <= 4.0, "ZoomY should respect its maximum");
        Assert.True(zoomBorder.ZoomX >= 0.5, "ZoomX should respect its minimum");
        Assert.True(zoomBorder.ZoomY >= 1.0, "ZoomY should respect its minimum");
    }
    
    [AvaloniaFact]
    public void OffsetConstraints_DifferentXAndY_HandledIndependently()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnablePan = true,
            PanButton = ButtonName.Left,
            MinOffsetX = -200,
            MaxOffsetX = 200,
            MinOffsetY = -50,
            MaxOffsetY = 50
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
        
        // Act - Set offsets that test different limits
        zoomBorder.Pan(250, 100); // Should be clamped to 200, 50
        
        // Assert
        Assert.True(zoomBorder.OffsetX <= 200, "OffsetX should respect its maximum");
        Assert.True(zoomBorder.OffsetY <= 50, "OffsetY should respect its maximum");
        
        // Test minimum limits
        zoomBorder.Pan(-250, -100); // Should be clamped to -200, -50
        
        Assert.True(zoomBorder.OffsetX >= -200, "OffsetX should respect its minimum");
        Assert.True(zoomBorder.OffsetY >= -50, "OffsetY should respect its minimum");
    }
    
    [AvaloniaFact]
    public void ConstraintValidation_WithNegativeValues_HandlesCorrectly()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            EnablePan = true,
            MinZoomX = 0.1,
            MaxZoomX = 10.0,
            MinZoomY = 0.1,
            MaxZoomY = 10.0,
            MinOffsetX = -1000,
            MaxOffsetX = 1000,
            MinOffsetY = -1000,
            MaxOffsetY = 1000
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
        
        // Act - Test with extreme values
        zoomBorder.Zoom(0.05, 100, 75); // Below minimum zoom
        zoomBorder.Pan(-2000, -2000); // Below minimum offset
        
        // Assert
        Assert.True(zoomBorder.ZoomX >= 0.1, "ZoomX should be clamped to minimum");
        Assert.True(zoomBorder.ZoomY >= 0.1, "ZoomY should be clamped to minimum");
        Assert.True(zoomBorder.OffsetX >= -1000, "OffsetX should be clamped to minimum");
        Assert.True(zoomBorder.OffsetY >= -1000, "OffsetY should be clamped to minimum");
        
        // Test with values above maximum
        zoomBorder.Zoom(15.0, 100, 75); // Above maximum zoom
        zoomBorder.Pan(2000, 2000); // Above maximum offset
        
        Assert.True(zoomBorder.ZoomX <= 10.0, "ZoomX should be clamped to maximum");
        Assert.True(zoomBorder.ZoomY <= 10.0, "ZoomY should be clamped to maximum");
        Assert.True(zoomBorder.OffsetX <= 1000, "OffsetX should be clamped to maximum");
        Assert.True(zoomBorder.OffsetY <= 1000, "OffsetY should be clamped to maximum");
    }
    
    [AvaloniaFact]
    public void ConstraintValidation_DuringInteraction_MaintainsConsistency()
    {
        // Arrange
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            EnablePan = true,
            EnableGestures = true,
            PanButton = ButtonName.Left,
            MinZoomX = 0.5,
            MaxZoomX = 3.0,
            MinOffsetX = -100,
            MaxOffsetX = 100
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
        
        // Act - Perform multiple interactions that could violate constraints
        for (int i = 0; i < 10; i++)
        {
            // Zoom
            var wheelEventArgs = new PointerWheelEventArgs(
                zoomBorder,
                new Pointer(i + 1, PointerType.Mouse, true),
                zoomBorder,
                new Point(200, 150),
                0,
                new PointerPointProperties(),
                KeyModifiers.None,
                new Vector(0, i % 2 == 0 ? 1 : -1))
            {
                RoutedEvent = InputElement.PointerWheelChangedEvent
            };
            
            zoomBorder.RaiseEvent(wheelEventArgs);
            
            // Pan
            var scrollEventArgs = new ScrollGestureEventArgs(i + 1, new Vector(i % 2 == 0 ? 20 : -20, 0))
            {
                RoutedEvent = InputElement.ScrollGestureEvent,
                Source = zoomBorder
            };
            
            zoomBorder.RaiseEvent(scrollEventArgs);
            
            // Assert constraints are maintained after each interaction
            Assert.True(zoomBorder.ZoomX >= 0.5 && zoomBorder.ZoomX <= 3.0, 
                $"ZoomX constraint violated at iteration {i}: {zoomBorder.ZoomX}");
            Assert.True(zoomBorder.OffsetX >= -100 && zoomBorder.OffsetX <= 100, 
                $"OffsetX constraint violated at iteration {i}: {zoomBorder.OffsetX}");
        }
    }
    
    [AvaloniaFact]
    public void AutoCalculateMaxZoom_DoesNotCauseOffsetDrift()
    {
        // Arrange - Issue #124: MaxZoom and AutoCalculateMaxZoom unexpected behavior
        // When zoom reaches the upper bound, offset should not drift
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            AutoCalculateMaxZoom = true,
            MaxZoomPixelSize = 2.0 // Max zoom is 2x
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
        
        // Zoom to max first
        for (int i = 0; i < 10; i++)
        {
            zoomBorder.ZoomIn();
        }
        
        // Record initial offset at max zoom
        var initialOffsetX = zoomBorder.OffsetX;
        var initialOffsetY = zoomBorder.OffsetY;
        var initialZoomX = zoomBorder.ZoomX;
        var initialZoomY = zoomBorder.ZoomY;
        
        // Act - Try to zoom further beyond max multiple times
        for (int i = 0; i < 20; i++)
        {
            var wheelEventArgs = new PointerWheelEventArgs(
                zoomBorder,
                new Pointer(1, PointerType.Mouse, true),
                zoomBorder,
                new Point(200, 150),
                0,
                new PointerPointProperties(),
                KeyModifiers.None,
                new Vector(0, 5))
            {
                RoutedEvent = InputElement.PointerWheelChangedEvent
            };
            
            zoomBorder.RaiseEvent(wheelEventArgs);
        }
        
        // Assert - Offset should not have drifted when already at max zoom
        Assert.Equal(initialZoomX, zoomBorder.ZoomX, 4); // Zoom should stay at max
        Assert.Equal(initialZoomY, zoomBorder.ZoomY, 4);
        Assert.Equal(initialOffsetX, zoomBorder.OffsetX, 4); // Offset should not drift
        Assert.Equal(initialOffsetY, zoomBorder.OffsetY, 4);
    }
    
    [AvaloniaFact]
    public void ManualMaxZoom_DoesNotCauseOffsetDrift()
    {
        // Arrange - When zoom reaches the manual upper bound, offset should not drift
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            MaxZoomX = 2.0,
            MaxZoomY = 2.0
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
        
        // Zoom to max first
        for (int i = 0; i < 10; i++)
        {
            zoomBorder.ZoomIn();
        }
        
        // Record initial offset at max zoom
        var initialOffsetX = zoomBorder.OffsetX;
        var initialOffsetY = zoomBorder.OffsetY;
        var initialZoomX = zoomBorder.ZoomX;
        var initialZoomY = zoomBorder.ZoomY;
        
        // Act - Try to zoom further beyond max multiple times
        for (int i = 0; i < 20; i++)
        {
            var wheelEventArgs = new PointerWheelEventArgs(
                zoomBorder,
                new Pointer(1, PointerType.Mouse, true),
                zoomBorder,
                new Point(200, 150),
                0,
                new PointerPointProperties(),
                KeyModifiers.None,
                new Vector(0, 5))
            {
                RoutedEvent = InputElement.PointerWheelChangedEvent
            };
            
            zoomBorder.RaiseEvent(wheelEventArgs);
        }
        
        // Assert - Offset should not have drifted when already at max zoom
        Assert.Equal(initialZoomX, zoomBorder.ZoomX, 4); // Zoom should stay at max
        Assert.Equal(initialZoomY, zoomBorder.ZoomY, 4);
        Assert.Equal(initialOffsetX, zoomBorder.OffsetX, 4); // Offset should not drift
        Assert.Equal(initialOffsetY, zoomBorder.OffsetY, 4);
    }
    
    [AvaloniaFact]
    public void ZoomTo_WhenExceedingMaxLimit_ClampsRatioToPreventTranslationJump()
    {
        // Arrange - This tests the core fix: when ZoomTo would exceed max zoom,
        // the ratio should be clamped BEFORE applying ScaleAtPrepend to prevent
        // the translation jump bug.
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            MaxZoomX = 2.0,
            MaxZoomY = 2.0,
            EnableConstrains = true,
            Stretch = StretchMode.None
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
        
        // Use the same center point for all zoom operations
        var zoomCenterX = 100.0;
        var zoomCenterY = 75.0;
        
        // Set initial zoom to 1.5 using absolute Zoom() method
        zoomBorder.Zoom(1.5, zoomCenterX, zoomCenterY, skipTransitions: true);
        
        // Verify we're at 1.5
        Assert.Equal(1.5, zoomBorder.ZoomX, 2);
        
        // Record state before the zoom that will exceed limits
        var initialOffsetX = zoomBorder.OffsetX;
        var initialOffsetY = zoomBorder.OffsetY;
        var initialZoom = zoomBorder.ZoomX;
        
        // Calculate where the zoom center point is in content space
        var contentPointX = (zoomCenterX - initialOffsetX) / initialZoom;
        var contentPointY = (zoomCenterY - initialOffsetY) / initialZoom;
        
        // Act - Try to zoom with ratio 1.5 (would go to 2.25, exceeds max 2.0)
        zoomBorder.ZoomTo(1.5, zoomCenterX, zoomCenterY, skipTransitions: true);
        
        // Assert - Zoom should be clamped to exactly 2.0
        Assert.Equal(2.0, zoomBorder.ZoomX, 4);
        Assert.Equal(2.0, zoomBorder.ZoomY, 4);
        
        // The content point under the zoom center should still map to the same screen position
        var finalOffsetX = zoomBorder.OffsetX;
        var finalOffsetY = zoomBorder.OffsetY;
        var finalZoom = zoomBorder.ZoomX;
        
        // Screen point = content point * zoom + offset
        var screenPointX = contentPointX * finalZoom + finalOffsetX;
        var screenPointY = contentPointY * finalZoom + finalOffsetY;
        
        // The screen point should be very close to the original zoom center
        Assert.Equal(zoomCenterX, screenPointX, 1); // Allow small floating point tolerance
        Assert.Equal(zoomCenterY, screenPointY, 1);
    }
    
    [AvaloniaFact]
    public void ZoomTo_WhenExceedingMinLimit_ClampsRatioToPreventTranslationJump()
    {
        // Arrange - Same test but for minimum zoom limit
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            MinZoomX = 0.5,
            MinZoomY = 0.5,
            EnableConstrains = true,
            Stretch = StretchMode.None
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
        
        var zoomCenterX = 100.0;
        var zoomCenterY = 75.0;
        
        // Set initial zoom to 0.7 using absolute Zoom() method
        zoomBorder.Zoom(0.7, zoomCenterX, zoomCenterY, skipTransitions: true);
        
        // Verify we're at 0.7
        Assert.Equal(0.7, zoomBorder.ZoomX, 2);
        
        var initialOffsetX = zoomBorder.OffsetX;
        var initialOffsetY = zoomBorder.OffsetY;
        var initialZoom = zoomBorder.ZoomX;
        
        var contentPointX = (zoomCenterX - initialOffsetX) / initialZoom;
        var contentPointY = (zoomCenterY - initialOffsetY) / initialZoom;
        
        // Act - Try to zoom with ratio 0.6 (would go to 0.42, below min 0.5)
        zoomBorder.ZoomTo(0.6, zoomCenterX, zoomCenterY, skipTransitions: true);
        
        // Assert - Zoom should be clamped to exactly 0.5
        Assert.Equal(0.5, zoomBorder.ZoomX, 4);
        Assert.Equal(0.5, zoomBorder.ZoomY, 4);
        
        var finalOffsetX = zoomBorder.OffsetX;
        var finalOffsetY = zoomBorder.OffsetY;
        var finalZoom = zoomBorder.ZoomX;
        
        var screenPointX = contentPointX * finalZoom + finalOffsetX;
        var screenPointY = contentPointY * finalZoom + finalOffsetY;
        
        Assert.Equal(zoomCenterX, screenPointX, 1);
        Assert.Equal(zoomCenterY, screenPointY, 1);
    }
    
    [AvaloniaFact]
    public void Zoom_WhenExceedingMaxLimit_ClampsValueToPreventTranslationJump()
    {
        // Arrange - Test the Zoom() method (absolute zoom value) clamping
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            MaxZoomX = 2.0,
            MaxZoomY = 2.0,
            EnableConstrains = true,
            Stretch = StretchMode.None
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
        
        var zoomCenterX = 150.0;
        var zoomCenterY = 100.0;
        
        // Act - Try to zoom to 3.0 (exceeds max 2.0)
        zoomBorder.Zoom(3.0, zoomCenterX, zoomCenterY, skipTransitions: true);
        
        // Assert - Zoom should be clamped to exactly 2.0
        Assert.Equal(2.0, zoomBorder.ZoomX, 4);
        Assert.Equal(2.0, zoomBorder.ZoomY, 4);
        
        // Verify the zoom center point maps correctly
        // For Zoom() with center (cx, cy), the formula is:
        // offset = center - (zoom * center) = center * (1 - zoom)
        var expectedOffsetX = zoomCenterX - (2.0 * zoomCenterX);
        var expectedOffsetY = zoomCenterY - (2.0 * zoomCenterY);
        
        Assert.Equal(expectedOffsetX, zoomBorder.OffsetX, 2);
        Assert.Equal(expectedOffsetY, zoomBorder.OffsetY, 2);
    }
    
    [AvaloniaFact]
    public void ZoomTo_RepeatedZoomAtLimit_NoAccumulatedTranslationDrift()
    {
        // Arrange - Test that repeated zoom attempts at the limit don't cause drift
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            MaxZoomX = 2.0,
            MaxZoomY = 2.0,
            EnableConstrains = true,
            Stretch = StretchMode.None
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
        
        // Use consistent zoom center for all operations
        var zoomCenterX = 100.0;
        var zoomCenterY = 75.0;
        
        // Get to exactly max zoom
        zoomBorder.Zoom(2.0, zoomCenterX, zoomCenterY, skipTransitions: true);
        
        var initialOffsetX = zoomBorder.OffsetX;
        var initialOffsetY = zoomBorder.OffsetY;
        
        // Act - Try to zoom in many times while already at max (using same center point)
        for (int i = 0; i < 50; i++)
        {
            zoomBorder.ZoomTo(1.2, zoomCenterX, zoomCenterY, skipTransitions: true);
        }
        
        // Assert - No drift should have occurred
        Assert.Equal(2.0, zoomBorder.ZoomX, 4);
        Assert.Equal(2.0, zoomBorder.ZoomY, 4);
        Assert.Equal(initialOffsetX, zoomBorder.OffsetX, 4);
        Assert.Equal(initialOffsetY, zoomBorder.OffsetY, 4);
    }
    
    [AvaloniaFact]
    public void ZoomTo_ApproachingLimit_GradualTransitionWithoutJump()
    {
        // Arrange - Test smooth transition as we approach the limit
        // The point under the zoom center should stay stationary throughout
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            MaxZoomX = 2.0,
            MaxZoomY = 2.0,
            EnableConstrains = true,
            Stretch = StretchMode.None
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
        
        // Use the same zoom center for all operations
        var zoomCenterX = 100.0;
        var zoomCenterY = 75.0;
        
        // Start at 1.0 (default), then zoom to 1.5x
        zoomBorder.ZoomTo(1.5, zoomCenterX, zoomCenterY, skipTransitions: true);
        Assert.Equal(1.5, zoomBorder.ZoomX, 2);
        
        // Track the content point under the zoom center
        var initialOffsetX = zoomBorder.OffsetX;
        var initialOffsetY = zoomBorder.OffsetY;
        var initialZoom = zoomBorder.ZoomX;
        var contentPointX = (zoomCenterX - initialOffsetX) / initialZoom;
        var contentPointY = (zoomCenterY - initialOffsetY) / initialZoom;
        
        // Act - Zoom in small increments, some will hit the limit
        for (int i = 0; i < 10; i++)
        {
            zoomBorder.ZoomTo(1.1, zoomCenterX, zoomCenterY, skipTransitions: true);
            
            var currentOffsetX = zoomBorder.OffsetX;
            var currentOffsetY = zoomBorder.OffsetY;
            var currentZoom = zoomBorder.ZoomX;
            
            // The content point under zoom center should map back to zoom center
            var currentScreenX = contentPointX * currentZoom + currentOffsetX;
            var currentScreenY = contentPointY * currentZoom + currentOffsetY;
            
            // Assert - The zoom center point should stay stationary (within tolerance)
            Assert.True(Math.Abs(currentScreenX - zoomCenterX) < 0.5,
                $"Iteration {i}: X drifted from {zoomCenterX} to {currentScreenX}");
            Assert.True(Math.Abs(currentScreenY - zoomCenterY) < 0.5,
                $"Iteration {i}: Y drifted from {zoomCenterY} to {currentScreenY}");
        }
        
        // Final zoom should be at max
        Assert.Equal(2.0, zoomBorder.ZoomX, 4);
    }

    [AvaloniaFact]
    public void Zoom_WhenConstrainsDisabled_IgnoresZoomLimits()
    {
        // Arrange - Zoom limits are set, but EnableConstrains is false,
        // so the Zoom() method should NOT clamp the value.
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            MaxZoomX = 2.0,
            MaxZoomY = 2.0,
            MinZoomX = 0.5,
            MinZoomY = 0.5,
            EnableConstrains = false,
            Stretch = StretchMode.None
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

        // Act - Zoom to 3.0, which exceeds MaxZoom but constraints are disabled
        zoomBorder.Zoom(3.0, 100.0, 75.0, skipTransitions: true);

        // Assert - Should reach 3.0 since constraints are disabled
        Assert.Equal(3.0, zoomBorder.ZoomX, 4);
        Assert.Equal(3.0, zoomBorder.ZoomY, 4);
    }

    [AvaloniaFact]
    public void ZoomTo_WhenConstrainsDisabled_IgnoresZoomLimits()
    {
        // Arrange - Zoom limits are set, but EnableConstrains is false,
        // so the ZoomTo() method should NOT clamp the ratio.
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableZoom = true,
            MaxZoomX = 2.0,
            MaxZoomY = 2.0,
            MinZoomX = 0.5,
            MinZoomY = 0.5,
            EnableConstrains = false,
            Stretch = StretchMode.None
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

        // Set initial zoom to 1.5
        zoomBorder.Zoom(1.5, 100.0, 75.0, skipTransitions: true);

        // Act - ZoomTo with ratio 3.0 (would go to 4.5, exceeds max but constraints disabled)
        zoomBorder.ZoomTo(3.0, 100.0, 75.0, skipTransitions: true);

        // Assert - Should reach 4.5 since constraints are disabled
        Assert.Equal(4.5, zoomBorder.ZoomX, 4);
        Assert.Equal(4.5, zoomBorder.ZoomY, 4);
    }
}
