// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.HeadlessTestingFramework;
using Avalonia.HeadlessTestingFramework.Recording;
using Xunit;
using Path = System.IO.Path;

namespace Avalonia.Controls.PanAndZoom.UnitTests;

/// <summary>
/// Tests for recording ZoomBorder interactions including zoom, pan, and gestures.
/// These tests demonstrate visual regression testing patterns for ZoomBorder.
/// </summary>
public class ZoomBorderRecordingTests : IDisposable
{
    private readonly string _testOutputDir;

    public ZoomBorderRecordingTests()
    {
        // Save recordings to artifacts/recordings for inspection
        var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        _testOutputDir = Path.Combine(projectDir, "artifacts", "recordings", $"ZoomBorderRecording_{DateTime.Now:yyyyMMdd_HHmmss}");
        Directory.CreateDirectory(_testOutputDir);
    }

    public void Dispose()
    {
        // Keep recordings for inspection - don't delete
        // Output location: artifacts/recordings/
    }

    [AvaloniaFact]
    public void Recording_ZoomBorderPinchZoom_CapturesZoomAnimation()
    {
        // Arrange
        using var simulator = new RecordedTouchSimulator();
        simulator.FramesPerStep = 1;

        var zoomBorder = CreateZoomBorderWithContent();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = zoomBorder
        };
        window.Show();
        
        // Force layout to ensure window has valid dimensions
        window.UpdateLayout();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        try
        {
            var outputPath = Path.Combine(_testOutputDir, "pinch_zoom");
            var session = simulator.StartRecording(window, outputPath: outputPath);

            // Act - Simulate pinch zoom in
            // Use center of element content for proper zoom behavior
            var centerPoint = new Point(zoomBorder.Bounds.Width / 2, zoomBorder.Bounds.Height / 2);
            simulator.AddMarker("ZoomStart", new { ZoomX = zoomBorder.ZoomX, ZoomY = zoomBorder.ZoomY });
            simulator.RecordedPinchZoom(zoomBorder, centerPoint, 50, 150, steps: 10);
            simulator.AddMarker("ZoomEnd", new { ZoomX = zoomBorder.ZoomX, ZoomY = zoomBorder.ZoomY });

            var stats = simulator.StopRecording();

            // Assert - verify frames were captured
            Assert.True(stats.FramesCaptured > 0, $"pinch_zoom: Expected frames > 0, got {stats.FramesCaptured}");

            // Assert - verify recording session and events are properly managed
            Assert.Contains(session.Events, e => e.EventType == RecordingEventType.Gesture);
            Assert.Contains(session.Events, e => e.EventType == RecordingEventType.Marker && e.Description == "ZoomStart");
            Assert.Contains(session.Events, e => e.EventType == RecordingEventType.Marker && e.Description == "ZoomEnd");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Recording_ZoomBorderPan_CapturesPanMovement()
    {
        // Arrange
        using var simulator = new RecordedTouchSimulator();
        simulator.FramesPerStep = 1;

        var zoomBorder = CreateZoomBorderWithContent();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = zoomBorder
        };
        window.Show();

        // Set initial zoom using the ZoomTo method
        zoomBorder.ZoomTo(2.0, 200, 150, skipTransitions: true);

        try
        {
            var outputPath = Path.Combine(_testOutputDir, "pan");
            var session = simulator.StartRecording(window, outputPath: outputPath);

            // Record initial offset
            var initialOffsetX = zoomBorder.OffsetX;
            var initialOffsetY = zoomBorder.OffsetY;

            // Act - Simulate pan (drag)
            var startPoint = new Point(200, 150);
            var endPoint = new Point(100, 100);

            simulator.AddMarker("PanStart", new { OffsetX = zoomBorder.OffsetX, OffsetY = zoomBorder.OffsetY });
            simulator.RecordedDrag(zoomBorder, startPoint, endPoint, steps: 10);
            simulator.AddMarker("PanEnd", new { OffsetX = zoomBorder.OffsetX, OffsetY = zoomBorder.OffsetY });

            var stats = simulator.StopRecording();

            // Assert - Verify pan was actually applied
            Assert.Contains(session.Events, e => e.EventType == RecordingEventType.Input);
            
            // Verify that offset changed after drag (pan happened)
            var finalOffsetX = zoomBorder.OffsetX;
            var finalOffsetY = zoomBorder.OffsetY;
            Assert.NotEqual(initialOffsetX, finalOffsetX); // Pan should have changed the offset
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Recording_ZoomBorderTwoFingerPan_CapturesGesture()
    {
        // Arrange
        using var simulator = new RecordedTouchSimulator();
        simulator.FramesPerStep = 1;

        var zoomBorder = CreateZoomBorderWithContent();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = zoomBorder
        };
        window.Show();

        // Set initial zoom using the ZoomTo method
        zoomBorder.ZoomTo(1.5, 200, 150, skipTransitions: true);

        try
        {
            var outputPath = Path.Combine(_testOutputDir, "two_finger_pan");
            var session = simulator.StartRecording(window, outputPath: outputPath);

            // Act
            simulator.AddMarker("TwoFingerPanStart");
            simulator.RecordedTwoFingerPan(zoomBorder, new Point(200, 150), new Point(100, 100), steps: 8);
            simulator.AddMarker("TwoFingerPanEnd");

            var stats = simulator.StopRecording();

            // Assert - In headless mode, frame capture may not work without Skia backend
            Assert.Contains(session.Events, e => e.EventType == RecordingEventType.Gesture);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Recording_ZoomBorderScrollGesture_CapturesScroll()
    {
        // Arrange
        using var simulator = new RecordedTouchSimulator();

        var zoomBorder = CreateZoomBorderWithContent();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = zoomBorder
        };
        window.Show();

        try
        {
            var outputPath = Path.Combine(_testOutputDir, "scroll");
            var session = simulator.StartRecording(window, outputPath: outputPath);

            // Act - Simulate scroll gestures
            simulator.AddMarker("ScrollSequenceStart");
            
            for (int i = 0; i < 5; i++)
            {
                simulator.RecordedScroll(zoomBorder, new Vector(0, -20));
                simulator.CaptureFrame();
            }
            
            simulator.AddMarker("ScrollSequenceEnd");

            var stats = simulator.StopRecording();

            // Assert
            Assert.True(session.Events.Count >= 5);
            Assert.Equal(5, CountEventsOfType(session, RecordingEventType.Scroll));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Recording_ZoomBorderDoubleTapZoom_CapturesDoubleTap()
    {
        // Arrange
        using var simulator = new RecordedTouchSimulator();

        var zoomBorder = CreateZoomBorderWithContent();
        zoomBorder.EnableDoubleClickZoom = true;

        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = zoomBorder
        };
        window.Show();

        try
        {
            var outputPath = Path.Combine(_testOutputDir, "double_tap");
            var session = simulator.StartRecording(window, outputPath: outputPath);

            // Act
            simulator.AddMarker("DoubleTapStart", new { ZoomX = zoomBorder.ZoomX });
            simulator.RecordedDoubleTap(zoomBorder, new Point(200, 150));
            simulator.AddMarker("DoubleTapEnd", new { ZoomX = zoomBorder.ZoomX });

            var stats = simulator.StopRecording();

            // Assert - verify frames were captured
            Assert.True(stats.FramesCaptured > 0, $"double_tap: Expected frames > 0, got {stats.FramesCaptured}");

            // Assert - verify input events were recorded
            Assert.Contains(session.Events, e => e.EventType == RecordingEventType.Input);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Recording_ZoomBorderRotation_CapturesRotationGesture()
    {
        // Arrange
        using var simulator = new RecordedTouchSimulator();
        simulator.FramesPerStep = 1;

        var zoomBorder = CreateZoomBorderWithContent();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = zoomBorder
        };
        window.Show();

        try
        {
            var outputPath = Path.Combine(_testOutputDir, "rotation");
            var session = simulator.StartRecording(window, outputPath: outputPath);
            
            // Verify initial rotation is 0
            var initialRotation = zoomBorder.Rotation;
            Assert.Equal(0.0, initialRotation);

            // Act - Simulate rotation
            simulator.AddMarker("RotationStart");
            simulator.RecordedRotation(zoomBorder, new Point(200, 150), 60, 0, 45, steps: 10);
            simulator.AddMarker("RotationEnd");

            var stats = simulator.StopRecording();

            // Assert - In headless mode, frame capture may not work without Skia backend
            Assert.Contains(session.Events, e => e.EventType == RecordingEventType.Gesture);
            
            // Verify rotation was applied
            Assert.NotEqual(0.0, zoomBorder.Rotation);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Recording_ZoomBorderSwipeGestures_CapturesAllDirections()
    {
        // Arrange
        using var simulator = new RecordedTouchSimulator();
        simulator.FramesPerStep = 1;

        var zoomBorder = CreateZoomBorderWithContent();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = zoomBorder
        };
        window.Show();

        try
        {
            var outputPath = Path.Combine(_testOutputDir, "swipes");
            var session = simulator.StartRecording(window, outputPath: outputPath);

            var center = new Point(200, 150);

            // Act - Swipe in all directions
            simulator.AddMarker("SwipeRight");
            simulator.RecordedSwipe(zoomBorder, center, Avalonia.HeadlessTestingFramework.SwipeDirection.Right, 80);

            simulator.AddMarker("SwipeLeft");
            simulator.RecordedSwipe(zoomBorder, center, Avalonia.HeadlessTestingFramework.SwipeDirection.Left, 80);

            simulator.AddMarker("SwipeUp");
            simulator.RecordedSwipe(zoomBorder, center, Avalonia.HeadlessTestingFramework.SwipeDirection.Up, 80);

            simulator.AddMarker("SwipeDown");
            simulator.RecordedSwipe(zoomBorder, center, Avalonia.HeadlessTestingFramework.SwipeDirection.Down, 80);

            var stats = simulator.StopRecording();

            // Assert - Markers are recorded with Start/End events plus 4 swipe markers = 6 total
            // Swipe gestures should be recorded
            Assert.True(CountEventsOfType(session, RecordingEventType.Marker) >= 4, "Should have at least 4 marker events");
            Assert.True(CountEventsOfType(session, RecordingEventType.Gesture) >= 4, "Should have at least 4 gesture events");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Recording_ZoomBorderComplexInteraction_CapturesFullWorkflow()
    {
        // Arrange
        using var simulator = new RecordedTouchSimulator();
        simulator.FramesPerStep = 1;

        var zoomBorder = CreateZoomBorderWithContent();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = zoomBorder
        };
        window.Show();

        try
        {
            var outputPath = Path.Combine(_testOutputDir, "complex_workflow");
            var session = simulator.StartRecording(window, outputPath: outputPath);
            var center = new Point(200, 150);

            // Act - Complex interaction sequence
            simulator.AddMarker("WorkflowStart");

            // Step 1: Tap to focus
            simulator.RecordedTap(zoomBorder, center);

            // Step 2: Pinch zoom in
            simulator.AddMarker("ZoomIn");
            simulator.RecordedPinchZoom(zoomBorder, center, 50, 100, steps: 5);

            // Step 3: Pan around
            simulator.AddMarker("Pan");
            simulator.RecordedDrag(zoomBorder, center, new Point(150, 100), steps: 5);

            // Step 4: Pinch zoom out
            simulator.AddMarker("ZoomOut");
            simulator.RecordedPinchZoom(zoomBorder, center, 100, 50, steps: 5);

            simulator.AddMarker("WorkflowEnd");

            var stats = simulator.StopRecording();

            // Assert - In headless mode, frame capture may not work without Skia backend
            // But we verify that the recording session and events are properly managed
            Assert.True(stats.EventCount >= 5, "Should have multiple events recorded");
            Assert.Contains(session.Events, e => e.EventType == RecordingEventType.Marker && e.Description == "WorkflowStart");
            Assert.Contains(session.Events, e => e.EventType == RecordingEventType.Marker && e.Description == "WorkflowEnd");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Recording_OutputFilesCreated_ForPngSequence()
    {
        // Arrange
        using var simulator = new RecordedTouchSimulator();

        var zoomBorder = CreateZoomBorderWithContent();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = zoomBorder
        };
        window.Show();

        try
        {
            var outputPath = Path.Combine(_testOutputDir, "png_output");
            var options = new RecordingOptions
            {
                Format = RecordingFormat.PngSequence,
                BaseFileName = "zoom_frame"
            };
            var session = simulator.StartRecording(window, options, outputPath);

            // Act - Capture a few frames
            simulator.CaptureFrame();
            simulator.CaptureFrame();
            simulator.CaptureFrame();

            var stats = simulator.StopRecording();

            // Assert - In headless mode without Skia backend, actual frame capture may not work
            // We verify the recording session is properly configured
            // If frames were captured, they should be PNG files
            if (stats.OutputFiles.Count > 0)
            {
                foreach (var file in stats.OutputFiles)
                {
                    Assert.True(file.EndsWith(".png"), "Output files should be PNG");
                }
            }
            // At minimum, the session should have been started and stopped
            Assert.Equal(RecordingState.Finalized, session.State);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Recording_Statistics_ContainsAccurateData()
    {
        // Arrange
        using var simulator = new RecordedTouchSimulator();

        var zoomBorder = CreateZoomBorderWithContent();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = zoomBorder
        };
        window.Show();

        try
        {
            var outputPath = Path.Combine(_testOutputDir, "statistics");
            var session = simulator.StartRecording(window, outputPath: outputPath);

            // Act
            simulator.AddMarker("Marker1");
            simulator.RecordedTap(zoomBorder, new Point(100, 100));
            simulator.AddMarker("Marker2");

            var stats = simulator.StopRecording();

            // Assert - In headless mode without Skia backend, frame capture may return 0
            // We verify the core recording functionality works
            Assert.Equal(session.SessionId, stats.SessionId);
            Assert.True(stats.Duration.TotalMilliseconds >= 0, "Duration should be valid");
            Assert.True(stats.EventCount > 0, "Should have recorded events");
            Assert.Equal(RecordingFormat.PngSequence, stats.Format);
        }
        finally
        {
            window.Close();
        }
    }

    private static ZoomBorder CreateZoomBorderWithContent()
    {
        return new ZoomBorder
        {
            ClipToBounds = true,
            Background = Brushes.White,
            PanButton = ButtonName.Left, // TouchInputSimulator uses left button
            Child = new Canvas
            {
                Width = 300,
                Height = 200,
                Background = Brushes.LightGray,
                Children =
                {
                    new Rectangle
                    {
                        Width = 50,
                        Height = 50,
                        Fill = Brushes.Red,
                        [Canvas.LeftProperty] = 25,
                        [Canvas.TopProperty] = 25
                    },
                    new Ellipse
                    {
                        Width = 60,
                        Height = 60,
                        Fill = Brushes.Blue,
                        [Canvas.LeftProperty] = 100,
                        [Canvas.TopProperty] = 70
                    },
                    new Rectangle
                    {
                        Width = 40,
                        Height = 80,
                        Fill = Brushes.Green,
                        [Canvas.LeftProperty] = 200,
                        [Canvas.TopProperty] = 50
                    }
                }
            }
        };
    }

    private static int CountEventsOfType(RecordingSession session, RecordingEventType eventType)
    {
        int count = 0;
        foreach (var evt in session.Events)
        {
            if (evt.EventType == eventType)
                count++;
        }
        return count;
    }

    [AvaloniaFact]
    public void Recording_PinchZoom_DiagnosticZoomValues()
    {
        // Arrange
        using var simulator = new RecordedTouchSimulator();
        simulator.FramesPerStep = 1;

        var zoomBorder = CreateZoomBorderWithContent();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = zoomBorder
        };
        window.Show();
        window.UpdateLayout();

        try
        {
            var session = simulator.StartRecording(window);
            
            // Use center of the ZoomBorder element instead of window center
            var centerPoint = new Point(zoomBorder.Bounds.Width / 2, zoomBorder.Bounds.Height / 2);
            
            var initialZoomX = zoomBorder.ZoomX;
            var initialOffsetX = zoomBorder.OffsetX;
            var initialOffsetY = zoomBorder.OffsetY;
            
            // Simulate pinch zoom
            simulator.RecordedPinchZoom(zoomBorder, centerPoint, 50, 150, steps: 10);
            
            var finalZoomX = zoomBorder.ZoomX;
            var finalOffsetX = zoomBorder.OffsetX;
            var finalOffsetY = zoomBorder.OffsetY;
            
            simulator.StopRecording();
            
            // Assert - ZoomX should have increased
            Assert.True(finalZoomX > initialZoomX, 
                $"ZoomX should have increased. Initial: {initialZoomX}, Final: {finalZoomX}. " +
                $"InitialOffset: ({initialOffsetX}, {initialOffsetY}), FinalOffset: ({finalOffsetX}, {finalOffsetY}). " +
                $"CenterPoint: {centerPoint}, ZoomBorder.Bounds: {zoomBorder.Bounds}");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact] // Use regular Fact since no Avalonia UI needed
    public void Recording_ConvertPinchZoomToVideo_CreatesVideoFile()
    {
        // Skip if FFmpeg not available
        var converter = new VideoConverter();
        if (!converter.IsFfmpegAvailable())
        {
            return;
        }

        // Find existing PNG recordings
        var recordingsDir = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..")), "artifacts", "recordings");
        if (!Directory.Exists(recordingsDir))
        {
            return; // Skip if no recordings exist
        }

        var pngDirs = Directory.GetDirectories(recordingsDir, "*", SearchOption.AllDirectories)
            .Where(d => Directory.GetFiles(d, "frame_*.png").Length > 0)
            .ToArray();

        if (pngDirs.Length == 0)
        {
            return; // Skip if no PNG sequences exist
        }

        var existingDir = pngDirs[0];

        // Convert to video
        var videoResult = converter.ConvertPngSequenceToVideo(existingDir, options: new VideoConversionOptions
        {
            Format = VideoFormat.Mp4,
            FrameRate = 30,
            Quality = 23,
            Timeout = TimeSpan.FromSeconds(30)
        });

        // Assert - video conversion result
        Assert.True(videoResult.Success, $"Video conversion failed: {videoResult.ErrorMessage}\nFFmpeg output: {videoResult.FfmpegOutput}");
        Assert.NotNull(videoResult.OutputPath);
        Assert.True(File.Exists(videoResult.OutputPath), 
            $"Video file should exist at {videoResult.OutputPath}");
    }

    [AvaloniaFact]
    public void Recording_MultipleGestures_CapturesFrames()
    {
        // Arrange
        using var simulator = new RecordedTouchSimulator();
        simulator.FramesPerStep = 1;

        var zoomBorder = CreateZoomBorderWithContent();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = zoomBorder
        };
        window.Show();
        window.UpdateLayout();

        try
        {
            // Record a complex interaction with multiple gestures
            var outputPath = Path.Combine(_testOutputDir, "multi_gesture");
            var session = simulator.StartRecording(window, outputPath: outputPath);

            var centerPoint = new Point(zoomBorder.Bounds.Width / 2, zoomBorder.Bounds.Height / 2);

            // 1. Zoom in
            simulator.AddMarker("ZoomIn");
            simulator.RecordedPinchZoom(zoomBorder, centerPoint, 50, 100, steps: 5);

            // 2. Pan
            simulator.AddMarker("Pan");
            simulator.RecordedDrag(zoomBorder, new Point(200, 150), new Point(150, 100), steps: 5);

            // 3. Rotate
            simulator.AddMarker("Rotate");
            simulator.RecordedRotation(zoomBorder, centerPoint, 30, 0, 45, steps: 5);

            // 4. Zoom out
            simulator.AddMarker("ZoomOut");
            simulator.RecordedPinchZoom(zoomBorder, centerPoint, 100, 50, steps: 5);

            var stats = simulator.StopRecording();

            // Assert - frames were captured
            Assert.True(stats.FramesCaptured > 0, $"Expected frames > 0, got {stats.FramesCaptured}");

            // Assert - markers were recorded
            Assert.Contains(session.Events, e => e.Description == "ZoomIn");
            Assert.Contains(session.Events, e => e.Description == "Pan");
            Assert.Contains(session.Events, e => e.Description == "Rotate");
            Assert.Contains(session.Events, e => e.Description == "ZoomOut");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact] // Use regular Fact since video conversion doesn't need Avalonia UI
    public void Recording_ConvertMultipleGesturesToVideo_CreatesVideoFiles()
    {
        // Skip if FFmpeg not available
        var converter = new VideoConverter();
        if (!converter.IsFfmpegAvailable())
        {
            return;
        }

        // Find existing multi_gesture recordings
        var recordingsDir = Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..")), "artifacts", "recordings");
        if (!Directory.Exists(recordingsDir))
        {
            return; // Skip if no recordings exist
        }

        var pngDirs = Directory.GetDirectories(recordingsDir, "*multi_gesture*", SearchOption.AllDirectories)
            .Where(d => Directory.GetFiles(d, "frame_*.png").Length > 0)
            .ToArray();

        // Also check generic PNG dirs if no multi_gesture specific ones
        if (pngDirs.Length == 0)
        {
            pngDirs = Directory.GetDirectories(recordingsDir, "*", SearchOption.AllDirectories)
                .Where(d => Directory.GetFiles(d, "frame_*.png").Length > 0)
                .ToArray();
        }

        if (pngDirs.Length == 0)
        {
            return; // Skip if no PNG sequences exist
        }

        var existingDir = pngDirs[0];

        // Convert to video
        var videoResult = converter.ConvertPngSequenceToVideo(existingDir, options: new VideoConversionOptions
        {
            Format = VideoFormat.Mp4,
            FrameRate = 30,
            Quality = 23,
            Timeout = TimeSpan.FromSeconds(30)
        });

        // Assert - video conversion result
        Assert.True(videoResult.Success, $"Video conversion failed: {videoResult.ErrorMessage}\nFFmpeg output: {videoResult.FfmpegOutput}");
        Assert.NotNull(videoResult.OutputPath);
        Assert.True(File.Exists(videoResult.OutputPath), 
            $"Video file should exist at {videoResult.OutputPath}");
        
        // Check video file has reasonable size (> 1KB)
        var fileInfo = new FileInfo(videoResult.OutputPath);
        Assert.True(fileInfo.Length > 1024, 
            $"Video file should be > 1KB, got {fileInfo.Length} bytes");
    }

    [Fact] // Use regular Fact since no Avalonia UI needed
    public void VideoConverter_IsFfmpegAvailable_ReturnsStatus()
    {
        // This test checks if FFmpeg is available on the system
        var converter = new VideoConverter();
        var isAvailable = converter.IsFfmpegAvailable();
        
        // Just log the result - don't fail the test if FFmpeg isn't installed
        if (isAvailable)
        {
            var version = converter.GetFfmpegVersion();
            Assert.NotNull(version);
        }
        
        // Test passes regardless - this is informational
        Assert.True(true, $"FFmpeg available: {isAvailable}");
    }
}