// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Reflection;
using Avalonia.HeadlessTestingFramework;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Media;
using Xunit;

namespace Avalonia.Controls.PanAndZoom.UnitTests;

public class ReflectionDiagnosticTest
{
    [AvaloniaFact]
    public void TestPinchEventHandlerRegistration()
    {
        // Create a simple border
        var border = new Border
        {
            Width = 100,
            Height = 100,
            Background = Brushes.Red
        };
        
        // Register handler BEFORE window.Show()
        var pinchEventCount = 0;
        border.AddHandler(InputElement.PinchEvent, (sender, e) => 
        {
            pinchEventCount++;
        });
        
        var window = new Window { Content = border };
        window.Show();
        
        // Manually raise a pinch event
        var testPinchArgs = new PinchEventArgs(1.5, new Point(50, 50));
        border.RaiseEvent(testPinchArgs);
        
        Assert.True(pinchEventCount >= 1, $"Manual PinchEvent not received! pinchEventCount={pinchEventCount}");
    }
    
    [AvaloniaFact]
    public void TestGestureRecognizerWithSimpleBorder()
    {
        // Create a border with PinchGestureRecognizer
        var border = new Border
        {
            Width = 400,
            Height = 300,
            Background = Brushes.Red
        };
        border.GestureRecognizers.Add(new PinchGestureRecognizer());
        
        // Register handler
        var pinchEventCount = 0;
        border.AddHandler(InputElement.PinchEvent, (sender, e) => 
        {
            pinchEventCount++;
        });
        
        var window = new Window { Content = border };
        window.Show();
        
        // Simulate two-finger touch
        var firstTouch = new GestureRecognizerTestHelper();
        var secondTouch = new GestureRecognizerTestHelper();
        
        // First finger down
        firstTouch.Down(border, new Point(100, 100));
        
        // Second finger down
        secondTouch.Down(border, new Point(200, 100));
        
        // Check capture status
        var firstCaptured = firstTouch.CapturedGestureRecognizer != null;
        var secondCaptured = secondTouch.CapturedGestureRecognizer != null;
        
        // Move first finger to trigger pinch
        firstTouch.Move(border, new Point(110, 100));
        
        Assert.True(pinchEventCount >= 1, 
            $"PinchEvent not received! pinchEventCount={pinchEventCount}, " +
            $"firstCaptured={firstCaptured}, secondCaptured={secondCaptured}");
    }
    
    [AvaloniaFact]
    public void TestZoomBorderGestureRecognizer()
    {
        // Create ZoomBorder
        var zoomBorder = new ZoomBorder
        {
            Width = 400,
            Height = 300,
            EnableGestureZoom = true,
            EnableGestures = true
        };
        zoomBorder.Child = new Border { Width = 200, Height = 150, Background = Brushes.Red };
        
        // Register handler
        var pinchEventCount = 0;
        zoomBorder.AddHandler(InputElement.PinchEvent, (sender, e) => 
        {
            pinchEventCount++;
        });
        
        var window = new Window { Content = zoomBorder };
        window.Show();
        
        // Verify recognizers exist
        var hasPinchRecognizer = false;
        PinchGestureRecognizer? pinchRecognizer = null;
        foreach (var gr in zoomBorder.GestureRecognizers)
        {
            if (gr is PinchGestureRecognizer pgr)
            {
                hasPinchRecognizer = true;
                pinchRecognizer = pgr;
            }
        }
        Assert.True(hasPinchRecognizer, "ZoomBorder should have PinchGestureRecognizer");
        
        // Simulate two-finger touch
        var firstTouch = new GestureRecognizerTestHelper();
        var secondTouch = new GestureRecognizerTestHelper();
        
        // First finger down
        firstTouch.Down(zoomBorder, new Point(100, 100));
        
        // Second finger down  
        secondTouch.Down(zoomBorder, new Point(200, 100));
        
        // Get internal state for debugging
        var pinchType = typeof(PinchGestureRecognizer);
        var firstContactField = pinchType.GetField("_firstContact", BindingFlags.Instance | BindingFlags.NonPublic);
        var secondContactField = pinchType.GetField("_secondContact", BindingFlags.Instance | BindingFlags.NonPublic);
        var initialDistanceField = pinchType.GetField("_initialDistance", BindingFlags.Instance | BindingFlags.NonPublic);
        
        var firstContact = firstContactField?.GetValue(pinchRecognizer);
        var secondContact = secondContactField?.GetValue(pinchRecognizer);
        var initialDistance = initialDistanceField?.GetValue(pinchRecognizer);
        
        // Move first finger to trigger pinch
        firstTouch.Move(zoomBorder, new Point(110, 100));
        
        Assert.True(pinchEventCount >= 1, 
            $"PinchEvent not received! pinchEventCount={pinchEventCount}, " +
            $"firstContact={firstContact != null}, secondContact={secondContact != null}, " +
            $"initialDistance={initialDistance}, " +
            $"firstCaptured={firstTouch.CapturedGestureRecognizer?.GetType().Name}, " +
            $"secondCaptured={secondTouch.CapturedGestureRecognizer?.GetType().Name}");
    }
}
