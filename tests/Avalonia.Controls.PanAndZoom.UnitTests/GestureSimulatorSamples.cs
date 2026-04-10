// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.HeadlessTestingFramework;
using Xunit;

namespace Avalonia.Controls.PanAndZoom.UnitTests;

/// <summary>
/// Sample code demonstrating gesture simulation usage with ZoomBorder control.
/// These tests serve as examples for common gesture interaction patterns.
/// </summary>
public class GestureSimulatorSamples
{
    #region Basic Tap Gestures

    /// <summary>
    /// Sample: Basic tap gesture to select an element
    /// </summary>
    [AvaloniaFact]
    public void Sample_TapToSelect()
    {
        // Arrange - Create a simple control
        var button = new Button { Content = "Click Me", Width = 100, Height = 50 };
        var window = new Window { Content = button };
        window.Show();

        var simulator = new GestureSimulator();
        var wasClicked = false;

        button.Click += (s, e) => wasClicked = true;

        // Act - Tap the button
        simulator.Tap(button, new Point(50, 25));

        // Note: Tapped events don't automatically trigger Click
        // For click simulation, use mouse simulator
    }

    /// <summary>
    /// Sample: Double-tap to zoom in on a ZoomBorder
    /// </summary>
    [AvaloniaFact]
    public void Sample_DoubleTapToZoom()
    {
        // Arrange
        var content = new Border 
        { 
            Width = 200, 
            Height = 200, 
            Background = Brushes.LightBlue 
        };
        var zoomBorder = new ZoomBorder 
        { 
            Width = 400, 
            Height = 300,
            Child = content
        };
        
        // Register handlers BEFORE showing window
        var doubleTapCount = 0;
        zoomBorder.AddHandler(InputElement.DoubleTappedEvent, (s, e) => doubleTapCount++);

        var window = new Window { Content = zoomBorder };
        window.Show();

        var simulator = new GestureSimulator();

        // Act - Double tap at center
        simulator.DoubleTap(zoomBorder, new Point(200, 150));

        // Assert
        Assert.Equal(1, doubleTapCount);
    }

    /// <summary>
    /// Sample: Right-tap to open context menu
    /// </summary>
    [AvaloniaFact]
    public void Sample_RightTapForContextMenu()
    {
        // Arrange
        var target = new Border 
        { 
            Width = 200, 
            Height = 200, 
            Background = Brushes.LightGreen 
        };
        
        var rightTapPosition = default(Point);
        target.AddHandler(InputElement.RightTappedEvent, (s, e) =>
        {
            if (e is TappedEventArgs tapped)
            {
                rightTapPosition = tapped.GetPosition(target);
            }
        });

        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();

        // Act - Right tap to trigger context menu
        simulator.RightTap(target, new Point(100, 100));

        // Assert - Event was raised (context menu would appear here)
        // In real usage, you'd show a ContextMenu
    }

    #endregion

    #region Pinch-to-Zoom Samples

    /// <summary>
    /// Sample: Pinch gesture to zoom in
    /// </summary>
    [AvaloniaFact]
    public void Sample_PinchToZoomIn()
    {
        // Arrange
        var zoomBorder = new ZoomBorder 
        { 
            Width = 400, 
            Height = 300,
            Child = new Border { Background = Brushes.Coral }
        };

        var scales = new List<double>();
        zoomBorder.AddHandler(InputElement.PinchEvent, (s, e) =>
        {
            if (e is PinchEventArgs pinch)
            {
                scales.Add(pinch.Scale);
            }
        });

        var window = new Window { Content = zoomBorder };
        window.Show();

        var simulator = new GestureSimulator();

        // Act - Pinch zoom from 1x to 2x (spread fingers apart)
        simulator.PinchZoom(zoomBorder, 
            origin: new Point(200, 150), 
            startScale: 1.0, 
            endScale: 2.0, 
            steps: 5);

        // Assert
        Assert.NotEmpty(scales);
        Assert.True(scales[^1] >= 1.9); // Final scale near 2.0
    }

    /// <summary>
    /// Sample: Pinch gesture to zoom out
    /// </summary>
    [AvaloniaFact]
    public void Sample_PinchToZoomOut()
    {
        // Arrange
        var zoomBorder = new ZoomBorder 
        { 
            Width = 400, 
            Height = 300,
            Child = new Border { Background = Brushes.Coral }
        };

        var scales = new List<double>();
        zoomBorder.AddHandler(InputElement.PinchEvent, (s, e) =>
        {
            if (e is PinchEventArgs pinch)
            {
                scales.Add(pinch.Scale);
            }
        });

        var window = new Window { Content = zoomBorder };
        window.Show();

        var simulator = new GestureSimulator();

        // Act - Pinch zoom from 2x to 1x (pinch fingers together)
        simulator.PinchZoom(zoomBorder, 
            origin: new Point(200, 150), 
            startScale: 2.0, 
            endScale: 1.0, 
            steps: 5);

        // Assert
        Assert.NotEmpty(scales);
        Assert.True(scales[^1] <= 1.1); // Final scale near 1.0
    }

    /// <summary>
    /// Sample: Rotation gesture with pinch
    /// </summary>
    [AvaloniaFact]
    public void Sample_PinchToRotate()
    {
        // Arrange
        var target = new Border 
        { 
            Width = 200, 
            Height = 200, 
            Background = Brushes.Purple 
        };

        var pinchCount = 0;
        target.AddHandler(InputElement.PinchEvent, (s, e) => pinchCount++);

        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();

        // Act - Rotate 90 degrees
        simulator.PinchRotate(target, 
            origin: new Point(100, 100), 
            startAngle: 0, 
            endAngle: 90, 
            steps: 10);

        // Assert
        Assert.True(pinchCount > 0);
    }

    #endregion

    #region Scroll Gesture Samples

    /// <summary>
    /// Sample: Scroll gesture in a ScrollViewer
    /// </summary>
    [AvaloniaFact]
    public void Sample_ScrollGesture()
    {
        // Arrange
        var content = new StackPanel();
        for (int i = 0; i < 50; i++)
        {
            content.Children.Add(new TextBlock { Text = $"Item {i}" });
        }

        var scrollViewer = new ScrollViewer { Content = content };
        
        var scrollDelta = Vector.Zero;
        scrollViewer.AddHandler(InputElement.ScrollGestureEvent, (s, e) =>
        {
            if (e is ScrollGestureEventArgs scroll)
            {
                scrollDelta += scroll.Delta;
            }
        });

        var window = new Window { Content = scrollViewer };
        window.Show();

        var simulator = new GestureSimulator();

        // Act - Scroll down 200 pixels
        simulator.ScrollSequence(scrollViewer, new Vector(0, 200), steps: 10);

        // Assert
        Assert.True(Math.Abs(scrollDelta.Y) > 100);
    }

    /// <summary>
    /// Sample: Flick gesture with inertia
    /// </summary>
    [AvaloniaFact]
    public void Sample_FlickGesture()
    {
        // Arrange
        var target = new Border 
        { 
            Width = 200, 
            Height = 200, 
            Background = Brushes.Orange 
        };

        var scrollEventCount = 0;
        target.AddHandler(InputElement.ScrollGestureEvent, (s, e) => scrollEventCount++);

        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();

        // Act - Flick upward
        simulator.Flick(target, Avalonia.HeadlessTestingFramework.SwipeDirection.Up, distance: 50, velocity: 500);

        // Assert
        Assert.True(scrollEventCount > 0);
    }

    #endregion

    #region Pull-to-Refresh Samples

    /// <summary>
    /// Sample: Pull-to-refresh gesture
    /// </summary>
    [AvaloniaFact]
    public void Sample_PullToRefresh()
    {
        // Arrange
        var refreshList = new Border 
        { 
            Width = 300, 
            Height = 400, 
            Background = Brushes.LightCyan 
        };

        var pullDistance = 0.0;
        var pullEnded = false;

        refreshList.AddHandler(InputElement.PullGestureEvent, (s, e) =>
        {
            if (e is PullGestureEventArgs pull)
            {
                pullDistance += pull.Delta.Y;
            }
        });

        refreshList.AddHandler(InputElement.PullGestureEndedEvent, (s, e) =>
        {
            pullEnded = true;
            // In real app: trigger data refresh here
        });

        var window = new Window { Content = refreshList };
        window.Show();

        var simulator = new GestureSimulator();

        // Act - Pull down to refresh
        simulator.PullToRefresh(refreshList, PullDirection.TopToBottom, distance: 100);

        // Assert
        Assert.True(pullDistance > 0);
        Assert.True(pullEnded);
    }

    #endregion

    #region Touchpad Gesture Samples (macOS)

    /// <summary>
    /// Sample: Touchpad magnify gesture (trackpad pinch)
    /// </summary>
    [AvaloniaFact]
    public void Sample_TouchpadMagnify()
    {
        // Arrange
        var target = new Border 
        { 
            Width = 200, 
            Height = 200, 
            Background = Brushes.Teal 
        };

        var magnifyDelta = 0.0;
        target.AddHandler(InputElement.PointerTouchPadGestureMagnifyEvent, (s, e) =>
        {
            if (e is PointerDeltaEventArgs delta)
            {
                magnifyDelta += delta.Delta.X;
            }
        });

        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();

        // Act - Magnify gesture (zoom in)
        simulator.TouchpadMagnifySequence(target, new Point(100, 100), 
            totalMagnification: 0.5, steps: 5);

        // Assert
        Assert.True(Math.Abs(magnifyDelta) > 0);
    }

    /// <summary>
    /// Sample: Touchpad swipe for navigation
    /// </summary>
    [AvaloniaFact]
    public void Sample_TouchpadSwipe()
    {
        // Arrange
        var navigationHost = new Border 
        { 
            Width = 400, 
            Height = 300, 
            Background = Brushes.SlateGray 
        };

        var swipeCount = 0;
        navigationHost.AddHandler(InputElement.PointerTouchPadGestureSwipeEvent, (s, e) =>
        {
            swipeCount++;
            // In real app: navigate between pages
        });

        var window = new Window { Content = navigationHost };
        window.Show();

        var simulator = new GestureSimulator();

        // Act - Swipe left to navigate forward
        simulator.TouchpadSwipeSequence(navigationHost, new Point(200, 150), 
            Avalonia.HeadlessTestingFramework.SwipeDirection.Left, distance: 100, steps: 5);

        // Assert
        Assert.True(swipeCount > 0);
    }

    #endregion

    #region Compound Gesture Samples

    /// <summary>
    /// Sample: Double-tap zoom (common mobile pattern)
    /// </summary>
    [AvaloniaFact]
    public void Sample_DoubleTapZoomPattern()
    {
        // Arrange
        var zoomBorder = new ZoomBorder 
        { 
            Width = 400, 
            Height = 300,
            Child = new Border { Background = Brushes.MediumPurple }
        };

        var doubleTapRaised = false;
        var pinchEvents = new List<double>();

        zoomBorder.AddHandler(InputElement.DoubleTappedEvent, (s, e) => doubleTapRaised = true);
        zoomBorder.AddHandler(InputElement.PinchEvent, (s, e) =>
        {
            if (e is PinchEventArgs pinch)
            {
                pinchEvents.Add(pinch.Scale);
            }
        });

        var window = new Window { Content = zoomBorder };
        window.Show();

        var simulator = new GestureSimulator();

        // Act - Double-tap to zoom (triggers DoubleTap + animated zoom)
        simulator.DoubleTapZoom(zoomBorder, new Point(200, 150), zoomScale: 2.0, steps: 5);

        // Assert
        Assert.True(doubleTapRaised);
        Assert.NotEmpty(pinchEvents);
    }

    /// <summary>
    /// Sample: Press and hold for context actions
    /// </summary>
    [AvaloniaFact]
    public void Sample_PressAndHold()
    {
        // Arrange
        var target = new Border 
        { 
            Width = 100, 
            Height = 100, 
            Background = Brushes.DarkOrange 
        };

        var holdingStates = new List<HoldingState>();
        target.AddHandler(InputElement.HoldingEvent, (s, e) =>
        {
            if (e is HoldingRoutedEventArgs holding)
            {
                holdingStates.Add(holding.HoldingState);
            }
        });

        var window = new Window { Content = target };
        window.Show();

        var simulator = new GestureSimulator();

        // Act - Press and hold for 500ms
        simulator.Hold(target, new Point(50, 50), holdDuration: 500);

        // Assert
        Assert.Equal(2, holdingStates.Count);
        Assert.Equal(HoldingState.Started, holdingStates[0]);
        Assert.Equal(HoldingState.Completed, holdingStates[1]);
    }

    #endregion

    #region Integration with ZoomBorder

    /// <summary>
    /// Sample: Complete pan and zoom gesture workflow with event monitoring
    /// Demonstrates simulating the full gesture sequence for a ZoomBorder-like component
    /// </summary>
    [AvaloniaFact]
    public void Sample_CompleteZoomBorderWorkflow()
    {
        // Arrange - use a generic control to demonstrate gesture flow
        var control = new Border
        {
            Width = 400,
            Height = 300,
            Background = Brushes.DeepSkyBlue
        };

        var window = new Window { Content = control };
        window.Show();

        var simulator = new GestureSimulator();

        // Track all gesture events
        var eventsRaised = new List<string>();
        
        control.AddHandler(InputElement.PinchEvent, (s, e) => eventsRaised.Add("Pinch"));
        control.AddHandler(InputElement.PinchEndedEvent, (s, e) => eventsRaised.Add("PinchEnded"));
        control.AddHandler(InputElement.ScrollGestureEvent, (s, e) => eventsRaised.Add("Scroll"));
        control.AddHandler(InputElement.ScrollGestureEndedEvent, (s, e) => eventsRaised.Add("ScrollEnded"));
        control.AddHandler(InputElement.DoubleTappedEvent, (s, e) => eventsRaised.Add("DoubleTap"));

        // Step 1: Zoom in with pinch gesture (typically used for zoom)
        simulator.PinchZoom(control, new Point(200, 150), 1.0, 1.5, steps: 5);
        simulator.PinchEnded(control);
        
        // Step 2: Double-tap to toggle zoom level
        simulator.DoubleTap(control, new Point(200, 150));

        // Step 3: Scroll/pan the content
        simulator.ScrollSequence(control, new Vector(50, 50), steps: 5);

        // Step 4: Zoom out with pinch
        simulator.PinchZoom(control, new Point(200, 150), 1.5, 0.8, steps: 5);
        simulator.PinchEnded(control);

        // Verify all gestures were raised in sequence
        Assert.Contains("Pinch", eventsRaised);
        Assert.Contains("PinchEnded", eventsRaised);
        Assert.Contains("DoubleTap", eventsRaised);
        Assert.Contains("Scroll", eventsRaised);
        Assert.Contains("ScrollEnded", eventsRaised);
        
        // Verify event order (pinch before double-tap before scroll)
        var firstPinchIndex = eventsRaised.IndexOf("Pinch");
        var doubleTapIndex = eventsRaised.IndexOf("DoubleTap");
        var firstScrollIndex = eventsRaised.IndexOf("Scroll");
        
        Assert.True(firstPinchIndex < doubleTapIndex, "Pinch should occur before DoubleTap");
        Assert.True(doubleTapIndex < firstScrollIndex, "DoubleTap should occur before Scroll");
    }

    #endregion
}
