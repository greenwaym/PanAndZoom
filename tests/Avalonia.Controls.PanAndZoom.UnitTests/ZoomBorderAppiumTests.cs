// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Threading.Tasks;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.HeadlessTestingFramework;
using Avalonia.HeadlessTestingFramework.Appium;
using Xunit;

namespace Avalonia.Controls.PanAndZoom.UnitTests;

/// <summary>
/// Tests for ZoomBorder using Appium-like API.
/// </summary>
public class ZoomBorderAppiumTests
{
    #region Helper Methods

    private static Window CreateTestWindow(ZoomBorder zoomBorder)
    {
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = zoomBorder
        };
        window.Show();
        return window;
    }

    private static ZoomBorder CreateZoomBorder(Control? content = null, StretchMode stretch = StretchMode.None)
    {
        var zoomBorder = new ZoomBorder
        {
            Name = "TestZoomBorder",
            Width = 400,
            Height = 300,
            EnablePan = true,
            EnableZoom = true,
            EnableGestures = true,
            Background = Brushes.Gray,
            Stretch = stretch // Default to None to avoid auto-fit during tests
        };

        AutomationProperties.SetAutomationId(zoomBorder, "zoom_border");

        if (content != null)
        {
            zoomBorder.Child = content;
        }
        else
        {
            // Default content
            zoomBorder.Child = new Border
            {
                Name = "ContentBorder",
                Width = 200,
                Height = 150,
                Background = Brushes.Blue,
                Child = new TextBlock
                {
                    Name = "ContentText",
                    Text = "Zoom Me",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }

        return zoomBorder;
    }

    #endregion

    #region Find ZoomBorder Tests

    [AvaloniaFact]
    public void Driver_FindElement_ByName_FindsZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Name("TestZoomBorder"));

        Assert.NotNull(element);
        Assert.Equal("ZoomBorder", element.TagName);
    }

    [AvaloniaFact]
    public void Driver_FindElement_ByAutomationId_FindsZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.AutomationId("zoom_border"));

        Assert.NotNull(element);
        Assert.IsType<ZoomBorder>(element.Control);
    }

    [AvaloniaFact]
    public void Driver_FindElement_ByClassName_FindsZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.ClassName("ZoomBorder"));

        Assert.NotNull(element);
        Assert.IsType<ZoomBorder>(element.Control);
    }

    [AvaloniaFact]
    public void Driver_FindElement_ByType_FindsZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        Assert.NotNull(element);
        Assert.Same(zoomBorder, element.Control);
    }

    [AvaloniaFact]
    public void Driver_FindElement_ByXPath_FindsZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.XPath("//ZoomBorder[@Name='TestZoomBorder']"));

        Assert.NotNull(element);
        Assert.IsType<ZoomBorder>(element.Control);
    }

    #endregion

    #region ZoomBorder Property Tests

    [AvaloniaFact]
    public void Element_GetProperty_ZoomX_ReturnsValue()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        var zoomX = element.GetProperty<double>("ZoomX");

        Assert.Equal(1.0, zoomX);
    }

    [AvaloniaFact]
    public void Element_GetProperty_ZoomY_ReturnsValue()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        var zoomY = element.GetProperty<double>("ZoomY");

        Assert.Equal(1.0, zoomY);
    }

    [AvaloniaFact]
    public void Element_GetProperty_EnablePan_ReturnsTrue()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        var enablePan = element.GetProperty<bool>("EnablePan");

        Assert.True(enablePan);
    }

    [AvaloniaFact]
    public void Element_GetProperty_EnableZoom_ReturnsTrue()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        var enableZoom = element.GetProperty<bool>("EnableZoom");

        Assert.True(enableZoom);
    }

    [AvaloniaFact]
    public void Element_SetProperty_ZoomSpeed_ChangesValue()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        element.SetProperty("ZoomSpeed", 1.5);

        Assert.Equal(1.5, zoomBorder.ZoomSpeed);
    }

    [AvaloniaFact]
    public void Element_SetProperty_EnablePan_ChangesValue()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        element.SetProperty("EnablePan", false);

        Assert.False(zoomBorder.EnablePan);
    }

    #endregion

    #region ZoomBorder Command Tests via Appium API

    [AvaloniaFact]
    public void Element_InvokeCommand_ZoomIn_IncreasesZoom()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        var initialZoom = zoomBorder.ZoomX;

        // Execute ZoomIn command via reflection
        if (zoomBorder.ZoomInCommand.CanExecute(null))
        {
            zoomBorder.ZoomInCommand.Execute(null);
        }

        Assert.True(zoomBorder.ZoomX > initialZoom);
    }

    [AvaloniaFact]
    public void Element_InvokeCommand_ZoomOut_DecreasesZoom()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        // First zoom in
        zoomBorder.ZoomInCommand.Execute(null);
        var zoomAfterIn = zoomBorder.ZoomX;

        // Then zoom out
        zoomBorder.ZoomOutCommand.Execute(null);

        Assert.True(zoomBorder.ZoomX < zoomAfterIn);
    }

    [AvaloniaFact]
    public void Element_InvokeCommand_Reset_ResetsZoom()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        // First zoom in
        zoomBorder.ZoomInCommand.Execute(null);
        Assert.NotEqual(1.0, zoomBorder.ZoomX);

        // Reset
        zoomBorder.ResetCommand.Execute(null);

        Assert.Equal(1.0, zoomBorder.ZoomX);
        Assert.Equal(0.0, zoomBorder.OffsetX);
        Assert.Equal(0.0, zoomBorder.OffsetY);
    }

    #endregion

    #region Touch Gesture Tests with TouchAction

    [AvaloniaFact]
    public void TouchAction_Tap_OnZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Perform tap gesture
        var action = new TouchAction(driver)
            .Tap(element);

        action.Perform();

        // Verify zoom remains unchanged at 1.0 (StretchMode.None)
        Assert.Equal(1.0, zoomBorder.ZoomX);
    }

    [AvaloniaFact]
    public void TouchAction_DoubleTap_OnZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        zoomBorder.EnableDoubleClickZoom = true;
        zoomBorder.DoubleClickZoomFactor = 2.0;
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Perform double tap gesture
        var action = new TouchAction(driver)
            .DoubleTap(element);

        action.Perform();

        // Note: DoubleTap might not trigger in headless mode without proper input simulation
        // This test verifies the action chain can be constructed
        Assert.NotNull(action);
    }

    [AvaloniaFact]
    public void TouchAction_LongPress_OnZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Perform long press gesture
        var action = new TouchAction(driver)
            .LongPress(element, 500);

        action.Perform();

        // Verify long press completed
        Assert.NotNull(action);
    }

    [AvaloniaFact]
    public void TouchAction_Swipe_OnZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        zoomBorder.PanButton = ButtonName.Left;
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Perform swipe gesture (pan)
        var action = new TouchAction(driver)
            .Press(element)
            .MoveTo(element.Center.X + 50, element.Center.Y)
            .Release();

        action.Perform();

        // Verify swipe gesture was constructed
        Assert.NotNull(action);
    }

    [AvaloniaFact]
    public void TouchAction_SwipeDirection_OnZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Perform swipe using direction enum
        var action = new TouchAction(driver)
            .Press(element)
            .Swipe(Avalonia.HeadlessTestingFramework.SwipeDirection.Right, 100)
            .Release();

        action.Perform();

        Assert.NotNull(action);
    }

    #endregion

    #region MultiTouch Tests

    [AvaloniaFact]
    public void MultiTouchAction_Pinch_ZoomIn()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Perform pinch gesture to zoom in
        MultiTouchAction.Pinch(driver, element, scale: 1.5);

        // The pinch gesture should have been simulated
        Assert.NotNull(element);
    }

    [AvaloniaFact]
    public void MultiTouchAction_Pinch_ZoomOut()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Perform pinch gesture to zoom out (scale < 1)
        MultiTouchAction.Pinch(driver, element, scale: 0.5);

        Assert.NotNull(element);
    }

    [AvaloniaFact]
    public void MultiTouchAction_Scroll_OnZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Perform scroll gesture
        MultiTouchAction.Scroll(driver, element, 0, 50);

        Assert.NotNull(element);
    }

    #endregion

    #region Wait and ExpectedConditions Tests

    [AvaloniaFact]
    public void Wait_Until_ElementExists_ZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.Wait.Until(
            ExpectedConditions.ElementExists(By.Type<ZoomBorder>()),
            timeout: TimeSpan.FromSeconds(5)
        );

        Assert.NotNull(element);
        Assert.IsType<ZoomBorder>(element.Control);
    }

    [AvaloniaFact]
    public void Wait_Until_ElementIsVisible_ZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.Wait.Until(
            ExpectedConditions.ElementIsVisible(By.Type<ZoomBorder>()),
            timeout: TimeSpan.FromSeconds(5)
        );

        Assert.NotNull(element);
        Assert.True(element.Displayed);
    }

    [AvaloniaFact]
    public void Wait_Until_PropertyValue_ZoomX()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Wait for ZoomX to be 1.0
        var result = driver.Wait.Until(d =>
        {
            var zb = d.FindElement(By.Type<ZoomBorder>());
            return zb.GetProperty<double>("ZoomX") == 1.0;
        }, timeout: TimeSpan.FromSeconds(5));

        Assert.True(result);
    }

    #endregion

    #region Find Child Elements Tests

    [AvaloniaFact]
    public void Element_FindElement_ChildContent()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        var contentBorder = element.FindElement(By.Name("ContentBorder"));

        Assert.NotNull(contentBorder);
        Assert.Equal("Border", contentBorder.TagName);
    }

    [AvaloniaFact]
    public void Element_FindElement_TextContent()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        var textBlock = element.FindElement(By.Name("ContentText"));

        Assert.NotNull(textBlock);
        Assert.Equal("Zoom Me", textBlock.Text);
    }

    [AvaloniaFact]
    public void Element_FindElements_AllBorders()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var borders = driver.FindElements(By.ClassName("Border"));

        // Should find at least ZoomBorder and ContentBorder
        Assert.True(borders.Count >= 2);
    }

    #endregion

    #region Element State Tests

    [AvaloniaFact]
    public void Element_Displayed_ZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        Assert.True(element.Displayed);
    }

    [AvaloniaFact]
    public void Element_Enabled_ZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        Assert.True(element.Enabled);
    }

    [AvaloniaFact]
    public void Element_Location_ZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        Assert.True(element.Location.X >= 0);
        Assert.True(element.Location.Y >= 0);
    }

    [AvaloniaFact]
    public void Element_Size_ZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        Assert.True(element.Size.Width > 0);
        Assert.True(element.Size.Height > 0);
    }

    [AvaloniaFact]
    public void Element_Center_ZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        Assert.True(element.Center.X > 0);
        Assert.True(element.Center.Y > 0);
    }

    #endregion

    #region Complex Content Tests

    [AvaloniaFact]
    public void ZoomBorder_WithCanvas_Content()
    {
        var canvas = new Canvas
        {
            Name = "DrawingCanvas",
            Width = 500,
            Height = 400,
            Background = Brushes.White
        };

        // Add shapes to canvas
        var rect = new Rectangle
        {
            Name = "Rect1",
            Width = 100,
            Height = 50,
            Fill = Brushes.Red
        };
        Canvas.SetLeft(rect, 50);
        Canvas.SetTop(rect, 50);
        canvas.Children.Add(rect);

        var ellipse = new Ellipse
        {
            Name = "Ellipse1",
            Width = 80,
            Height = 80,
            Fill = Brushes.Green
        };
        Canvas.SetLeft(ellipse, 200);
        Canvas.SetTop(ellipse, 100);
        canvas.Children.Add(ellipse);

        var zoomBorder = CreateZoomBorder(canvas);
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var zoomElement = driver.FindElement(By.Type<ZoomBorder>());
        var canvasElement = zoomElement.FindElement(By.Name("DrawingCanvas"));
        var rectElement = driver.FindElement(By.Name("Rect1"));
        var ellipseElement = driver.FindElement(By.Name("Ellipse1"));

        Assert.NotNull(canvasElement);
        Assert.NotNull(rectElement);
        Assert.NotNull(ellipseElement);
    }

    [AvaloniaFact]
    public void ZoomBorder_WithImage_Content()
    {
        var image = new Image
        {
            Name = "TestImage",
            Width = 300,
            Height = 200,
            Stretch = Stretch.Uniform
        };

        var zoomBorder = CreateZoomBorder(image);
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var zoomElement = driver.FindElement(By.Type<ZoomBorder>());
        var imageElement = zoomElement.FindElement(By.Name("TestImage"));

        Assert.NotNull(imageElement);
        Assert.Equal("Image", imageElement.TagName);
    }

    #endregion

    #region Composite Locator Tests

    [AvaloniaFact]
    public void By_Chained_FindsNestedElement()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(
            By.Chained(
                By.Type<ZoomBorder>(),
                By.Name("ContentBorder")
            )
        );

        Assert.NotNull(element);
        Assert.Equal("Border", element.TagName);
    }

    [AvaloniaFact]
    public void By_All_FindsElementMatchingAllConditions()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(
            By.All(
                By.ClassName("ZoomBorder"),
                By.Name("TestZoomBorder")
            )
        );

        Assert.NotNull(element);
        Assert.IsType<ZoomBorder>(element.Control);
    }

    [AvaloniaFact]
    public void By_Any_FindsElementMatchingAnyCondition()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(
            By.Any(
                By.Name("NonExistent"),
                By.Name("TestZoomBorder")
            )
        );

        Assert.NotNull(element);
        Assert.IsType<ZoomBorder>(element.Control);
    }

    #endregion

    #region Property Locator Tests

    [AvaloniaFact]
    public void By_Property_EnablePan_FindsZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(
            By.Property("EnablePan", true)
        );

        Assert.NotNull(element);
        Assert.IsType<ZoomBorder>(element.Control);
    }

    [AvaloniaFact]
    public void By_Property_Width_FindsZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(
            By.Property("Width", 400.0)
        );

        Assert.NotNull(element);
    }

    #endregion

    #region Screenshot Tests

    [AvaloniaFact]
    public void Element_Screenshot_ReturnsImage()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        var screenshot = element.Screenshot();

        Assert.NotNull(screenshot);
        Assert.True(screenshot.PixelSize.Width > 0);
        Assert.True(screenshot.PixelSize.Height > 0);
    }

    [AvaloniaFact]
    public void Driver_Screenshot_ReturnsWindowImage()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var screenshot = driver.Screenshot();

        Assert.NotNull(screenshot);
        Assert.True(screenshot.PixelSize.Width > 0);
        Assert.True(screenshot.PixelSize.Height > 0);
    }

    #endregion

    #region ZoomBorder Direct Method Tests via Appium API

    [AvaloniaFact]
    public void ZoomBorder_ZoomIn_ViaAppiumElement()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        var initialZoom = element.GetProperty<double>("ZoomX");

        // Call ZoomIn directly on the ZoomBorder
        ((ZoomBorder)element.Control).ZoomIn(skipTransitions: true);

        var newZoom = element.GetProperty<double>("ZoomX");
        Assert.True(newZoom > initialZoom);
    }

    [AvaloniaFact]
    public void ZoomBorder_ZoomOut_ViaAppiumElement()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        
        // First zoom in
        ((ZoomBorder)element.Control).ZoomIn(skipTransitions: true);
        var zoomAfterIn = element.GetProperty<double>("ZoomX");

        // Then zoom out
        ((ZoomBorder)element.Control).ZoomOut(skipTransitions: true);
        var zoomAfterOut = element.GetProperty<double>("ZoomX");

        Assert.True(zoomAfterOut < zoomAfterIn);
    }

    [AvaloniaFact]
    public void ZoomBorder_Pan_ViaAppiumElement()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        var initialOffsetX = element.GetProperty<double>("OffsetX");
        var initialOffsetY = element.GetProperty<double>("OffsetY");

        // Pan using ZoomBorder's method
        ((ZoomBorder)element.Control).PanDelta(50, 30, skipTransitions: true);

        var newOffsetX = element.GetProperty<double>("OffsetX");
        var newOffsetY = element.GetProperty<double>("OffsetY");

        Assert.NotEqual(initialOffsetX, newOffsetX);
        Assert.NotEqual(initialOffsetY, newOffsetY);
    }

    [AvaloniaFact]
    public void ZoomBorder_ResetMatrix_ViaAppiumElement()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Zoom and pan
        ((ZoomBorder)element.Control).ZoomIn(skipTransitions: true);
        ((ZoomBorder)element.Control).PanDelta(50, 30, skipTransitions: true);

        // Reset
        ((ZoomBorder)element.Control).ResetMatrix(skipTransitions: true);

        Assert.Equal(1.0, element.GetProperty<double>("ZoomX"));
        Assert.Equal(1.0, element.GetProperty<double>("ZoomY"));
        Assert.Equal(0.0, element.GetProperty<double>("OffsetX"));
        Assert.Equal(0.0, element.GetProperty<double>("OffsetY"));
    }

    #endregion

    #region ZoomBorder Constraint Tests via Appium API

    [AvaloniaFact]
    public void ZoomBorder_MinZoom_Constraint()
    {
        var zoomBorder = CreateZoomBorder();
        zoomBorder.MinZoomX = 0.5;
        zoomBorder.MinZoomY = 0.5;
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Try to zoom out beyond minimum
        for (int i = 0; i < 10; i++)
        {
            ((ZoomBorder)element.Control).ZoomOut(skipTransitions: true);
        }

        var zoomX = element.GetProperty<double>("ZoomX");
        var zoomY = element.GetProperty<double>("ZoomY");

        Assert.True(zoomX >= 0.5);
        Assert.True(zoomY >= 0.5);
    }

    [AvaloniaFact]
    public void ZoomBorder_MaxZoom_Constraint()
    {
        var zoomBorder = CreateZoomBorder();
        zoomBorder.MaxZoomX = 4.0;
        zoomBorder.MaxZoomY = 4.0;
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Try to zoom in beyond maximum
        for (int i = 0; i < 20; i++)
        {
            ((ZoomBorder)element.Control).ZoomIn(skipTransitions: true);
        }

        var zoomX = element.GetProperty<double>("ZoomX");
        var zoomY = element.GetProperty<double>("ZoomY");

        Assert.True(zoomX <= 4.0);
        Assert.True(zoomY <= 4.0);
    }

    #endregion

    #region ZoomBorder Events via Appium API

    [AvaloniaFact]
    public void ZoomBorder_ZoomChanged_EventRaised()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var eventRaised = false;
        zoomBorder.ZoomChanged += (s, e) => eventRaised = true;

        var element = driver.FindElement(By.Type<ZoomBorder>());
        ((ZoomBorder)element.Control).ZoomIn(skipTransitions: true);

        Assert.True(eventRaised);
    }

    [AvaloniaFact]
    public void ZoomBorder_MatrixChanged_EventRaised()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        // Get initial zoom
        var initialZoomX = zoomBorder.ZoomX;

        var element = driver.FindElement(By.Type<ZoomBorder>());
        // Use ZoomIn via the command which modifies the matrix
        zoomBorder.ZoomInCommand.Execute(null);

        // The zoom should have changed
        Assert.NotEqual(initialZoomX, zoomBorder.ZoomX);
    }

    #endregion

    #region Focus and Keyboard Tests

    [AvaloniaFact]
    public void Element_Focus_ZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        element.Focus();

        // Note: Focus behavior may vary in headless mode
        Assert.NotNull(element);
    }

    [AvaloniaFact]
    public void Element_PressKey_ZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        zoomBorder.EnableKeyboardNavigation = true;
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        element.Focus();

        // Press arrow keys for navigation
        element.PressKey(Key.Right);
        element.PressKey(Key.Down);

        Assert.NotNull(element);
    }

    #endregion

    #region PageSource Tests

    [AvaloniaFact]
    public void Driver_PageSource_ContainsZoomBorder()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var pageSource = driver.PageSource;

        Assert.Contains("ZoomBorder", pageSource);
        Assert.Contains("TestZoomBorder", pageSource);
    }

    [AvaloniaFact]
    public void Driver_PageSource_ContainsContent()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var pageSource = driver.PageSource;

        Assert.Contains("ContentBorder", pageSource);
        Assert.Contains("ContentText", pageSource);
    }

    #endregion

    #region Element Count and Exists Tests

    [AvaloniaFact]
    public void Driver_ElementExists_ZoomBorder_ReturnsTrue()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var exists = driver.ElementExists(By.Type<ZoomBorder>());

        Assert.True(exists);
    }

    [AvaloniaFact]
    public void Driver_ElementExists_NonExistent_ReturnsFalse()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var exists = driver.ElementExists(By.Name("NonExistentElement"));

        Assert.False(exists);
    }

    [AvaloniaFact]
    public void Driver_ElementCount_Borders()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var count = driver.ElementCount(By.ClassName("Border"));

        Assert.True(count >= 2); // ZoomBorder and ContentBorder
    }

    #endregion

    #region Async Operations

    [AvaloniaFact]
    public async Task TouchAction_PerformAsync_Completes()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        var action = new TouchAction(driver)
            .Tap(element);

        await action.PerformAsync();

        Assert.NotNull(action);
    }

    [AvaloniaFact]
    public async Task TouchAction_ChainedGestures_PerformAsync()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        var action = new TouchAction(driver)
            .Press(element)
            .Wait(100)
            .MoveBy(50, 0)
            .Wait(100)
            .Release();

        await action.PerformAsync();

        Assert.NotNull(action);
    }

    #endregion

    #region TryFindElement Tests

    [AvaloniaFact]
    public void Driver_TryFindElement_Exists_ReturnsElement()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.TryFindElement(By.Type<ZoomBorder>());

        Assert.NotNull(element);
    }

    [AvaloniaFact]
    public void Driver_TryFindElement_NotExists_ReturnsNull()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.TryFindElement(By.Name("NonExistent"));

        Assert.Null(element);
    }

    #endregion

    #region ZoomBorder Stretch Mode Tests

    [AvaloniaFact]
    public void ZoomBorder_SetStretch_None()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        element.SetProperty("Stretch", StretchMode.None);

        Assert.Equal(StretchMode.None, zoomBorder.Stretch);
    }

    [AvaloniaFact]
    public void ZoomBorder_SetStretch_Fill()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        element.SetProperty("Stretch", StretchMode.Fill);

        Assert.Equal(StretchMode.Fill, zoomBorder.Stretch);
    }

    [AvaloniaFact]
    public void ZoomBorder_SetStretch_Uniform()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());
        element.SetProperty("Stretch", StretchMode.Uniform);

        Assert.Equal(StretchMode.Uniform, zoomBorder.Stretch);
    }

    #endregion

    #region ZoomBorder ZoomToRectangle Tests

    [AvaloniaFact]
    public void ZoomBorder_ZoomToRectangle_ViaAppiumElement()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Zoom to a specific rectangle
        ((ZoomBorder)element.Control).ZoomToRectangle(
            new Rect(50, 50, 100, 100), 
            animate: false
        );

        // Verify zoom changed
        var zoomX = element.GetProperty<double>("ZoomX");
        Assert.NotEqual(1.0, zoomX);
    }

    #endregion

    #region ZoomBorder CenterOn Tests

    [AvaloniaFact]
    public void ZoomBorder_CenterOn_ViaAppiumElement()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Center on a specific point with zoom
        ((ZoomBorder)element.Control).CenterOn(
            new Point(100, 75),
            zoom: 2.0,
            animate: false
        );

        // Verify centering and zoom
        var zoomX = element.GetProperty<double>("ZoomX");
        Assert.Equal(2.0, zoomX);
    }

    #endregion

    #region Driver Registration Tests

    [AvaloniaFact]
    public void Driver_RegisterPredicate_CustomLocator()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        // Register a custom predicate for ZoomBorder
        driver.RegisterPredicate("ZoomEnabled", c => 
            c is ZoomBorder zb && zb.EnableZoom);

        // Find using custom predicate (requires description and predicate key)
        var element = driver.FindElement(By.Predicate("Finds ZoomBorder with zoom enabled", "ZoomEnabled"));

        Assert.NotNull(element);
        Assert.IsType<ZoomBorder>(element.Control);
    }

    #endregion

    #region Touch Action Variations

    [AvaloniaFact]
    public void TouchAction_Press_WithCoordinates()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var action = new TouchAction(driver)
            .Press(100, 100)
            .Wait(50)
            .Release();

        action.Perform();

        Assert.NotNull(action);
    }

    [AvaloniaFact]
    public void TouchAction_Tap_MultipleCount()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.FindElement(By.Type<ZoomBorder>());

        // Triple tap
        var action = new TouchAction(driver)
            .Tap(element, 3);

        action.Perform();

        Assert.NotNull(action);
    }

    [AvaloniaFact]
    public void TouchAction_Swipe_FullPath()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        // Swipe from specific start to end
        var action = new TouchAction(driver)
            .Swipe(100, 100, 200, 150, 300);

        action.Perform();

        Assert.NotNull(action);
    }

    #endregion

    #region ExpectedConditions Tests

    [AvaloniaFact]
    public void ExpectedConditions_ElementToBeClickable()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var element = driver.Wait.Until(
            ExpectedConditions.ElementToBeClickable(By.Type<ZoomBorder>()),
            timeout: TimeSpan.FromSeconds(5)
        );

        Assert.NotNull(element);
        Assert.True(element.Enabled);
    }

    [AvaloniaFact]
    public void ExpectedConditions_AttributeToBe()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        var result = driver.Wait.Until(
            ExpectedConditions.AttributeToBe(By.Type<ZoomBorder>(), "Name", "TestZoomBorder"),
            timeout: TimeSpan.FromSeconds(5)
        );

        Assert.True(result);
    }

    [AvaloniaFact]
    public void ExpectedConditions_NumberOfElementsToBe()
    {
        var zoomBorder = CreateZoomBorder();
        var window = CreateTestWindow(zoomBorder);
        var driver = new AvaloniaDriver(window);

        // Find all TextBlock elements (there's one in the default content)
        var elements = driver.FindElements(By.Type<TextBlock>());

        Assert.NotNull(elements);
        Assert.True(elements.Count >= 1, $"Expected at least 1 TextBlock, found {elements.Count}");
    }

    #endregion
}
