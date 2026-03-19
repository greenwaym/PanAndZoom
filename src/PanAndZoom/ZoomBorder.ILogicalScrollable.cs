// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using static System.Math;

namespace Avalonia.Controls.PanAndZoom;

/// <summary>
/// Pan and zoom control for Avalonia.
/// </summary>
public partial class ZoomBorder : ILogicalScrollable
{
    private Size _extent;
    private Size _viewport;
    private Vector _offset;
    private bool _canHorizontallyScroll;
    private bool _canVerticallyScroll;
    private EventHandler? _scrollInvalidated;

    /// <summary>
    /// Calculate scrollable properties.
    /// </summary>
    /// <param name="source">The source bounds.</param>
    /// <param name="borderSize">The size of border (this control)</param>
    /// <param name="matrix">The transform matrix.</param>
    /// <param name="extent">The extent of the scrollable content.</param>
    /// <param name="viewport">The size of the viewport.</param>
    /// <param name="offset">The current scroll offset.</param>
    public static void CalculateScrollable(Rect source, Size borderSize, Matrix matrix, out Size extent, out Size viewport, out Vector offset)
    {
        var bounds = new Rect(0, 0, source.Width, source.Height);
            
        viewport = borderSize;

        var transformed = bounds.TransformToAABB(matrix);

        Log($"[CalculateScrollable] source: {source}, bounds: {bounds}, transformed: {transformed}");

        var width = transformed.Size.Width;
        var height = transformed.Size.Height;

        if (width < viewport.Width)
        {
            width = viewport.Width;

            if (transformed.Position.X < 0.0)
            {
                width += Abs(transformed.Position.X);
            }
            else
            {
                var widthTranslated = transformed.Size.Width + transformed.Position.X;
                if (widthTranslated > width)
                {
                    width += widthTranslated - width;
                }
            }
        }
        else if (!(width > viewport.Width))
        {
            width += Abs(transformed.Position.X);
        }
            
        if (height < viewport.Height)
        {
            height = viewport.Height;
                
            if (transformed.Position.Y < 0.0)
            {
                height += Abs(transformed.Position.Y);
            }
            else
            {
                var heightTranslated = transformed.Size.Height + transformed.Position.Y;
                if (heightTranslated > height)
                {
                    height += heightTranslated - height;
                }
            }
        }
        else if (!(height > viewport.Height))
        {
            height += Abs(transformed.Position.Y);
        }

        extent = new Size(width, height);

        var ox = transformed.Position.X;
        var oy = transformed.Position.Y;

        var offsetX = ox < 0 ? Abs(ox) : 0;
        var offsetY = oy < 0 ? Abs(oy) : 0;

        offset = new Vector(offsetX, offsetY);

        Log($"[CalculateScrollable] Extent: {extent} | Offset: {offset} | Viewport: {viewport}");
    }

    /// <inheritdoc/>
    Size IScrollable.Extent => _extent;

    /// <inheritdoc/>
    Vector IScrollable.Offset
    {
        get => _offset;
        set
        {
            Log($"[Offset] offset value: {value}");
            if (_updating)
            {
                return;
            }
            _updating = true;

            var (x, y) = _offset;
            var dx = x - value.X;
            var dy = y - value.Y;

            _offset = value;

            Log($"[Offset] offset: {_offset}, dx: {dx}, dy: {dy}");

            if (dx != 0 || dy != 0)
            {
                _matrix = MatrixHelper.ScaleAndTranslate(_zoomX, _zoomY, _matrix.M31 + dx, _matrix.M32 + dy);
                Invalidate(!this.IsPointerOver);
            }

            _updating = false;
        }
    }

    /// <inheritdoc/>
    Size IScrollable.Viewport => _viewport;

    /// <summary>
    /// Gets or sets whether horizontal scrolling is enabled for the logical scroll contract.
    /// </summary>
    public bool CanHorizontallyScroll
    {
        get => _canHorizontallyScroll;
        set
        {
            _canHorizontallyScroll = value;
            InvalidateMeasure();
        }
    }

    /// <summary>
    /// Gets or sets whether vertical scrolling is enabled for the logical scroll contract.
    /// </summary>
    public bool CanVerticallyScroll
    {
        get => _canVerticallyScroll;
        set
        {
            _canVerticallyScroll = value;
            InvalidateMeasure();
        }
    }

    bool ILogicalScrollable.IsLogicalScrollEnabled => true;

    event EventHandler? ILogicalScrollable.ScrollInvalidated
    {
        add => _scrollInvalidated += value;
        remove => _scrollInvalidated -= value;
    }

    Size ILogicalScrollable.ScrollSize => new Size(1, 1);

    Size ILogicalScrollable.PageScrollSize => new Size(10, 10);

    bool ILogicalScrollable.BringIntoView(Control target, Rect targetRect)
    {
        if (_element == null || target == null)
        {
            return false;
        }

        // Get the bounds of the target control relative to the ZoomBorder's child element
        var targetBounds = targetRect;
        
        // If targetRect has zero size, use the target's bounds
        if (targetRect.Width <= 0 && targetRect.Height <= 0)
        {
            targetBounds = target.Bounds;
        }

        // Try to translate target coordinates to our content coordinate system
        if (target != _element)
        {
            var transform = target.TransformToVisual(_element);
            if (transform.HasValue)
            {
                targetBounds = targetBounds.TransformToAABB(transform.Value);
            }
            else
            {
                // Cannot determine transform, fail gracefully
                return false;
            }
        }

        // Transform the target bounds using current matrix to get viewport coordinates
        var transformedBounds = targetBounds.TransformToAABB(_matrix);

        // Get current viewport
        var viewportRect = new Rect(0, 0, Bounds.Width, Bounds.Height);

        // Check if already fully visible
        if (viewportRect.Contains(transformedBounds))
        {
            return true;
        }

        // Calculate required pan to bring target into view
        var deltaX = 0.0;
        var deltaY = 0.0;

        // Check horizontal visibility
        if (transformedBounds.Left < viewportRect.Left)
        {
            deltaX = viewportRect.Left - transformedBounds.Left;
        }
        else if (transformedBounds.Right > viewportRect.Right)
        {
            deltaX = viewportRect.Right - transformedBounds.Right;
        }

        // Check vertical visibility
        if (transformedBounds.Top < viewportRect.Top)
        {
            deltaY = viewportRect.Top - transformedBounds.Top;
        }
        else if (transformedBounds.Bottom > viewportRect.Bottom)
        {
            deltaY = viewportRect.Bottom - transformedBounds.Bottom;
        }

        // Apply pan if needed
        if (deltaX != 0 || deltaY != 0)
        {
            PanDelta(deltaX, deltaY);
        }

        return true;
    }

    Control? ILogicalScrollable.GetControlInDirection(NavigationDirection direction, Control? from)
    {
        return null;
    }

    void ILogicalScrollable.RaiseScrollInvalidated(EventArgs e)
    {
        _scrollInvalidated?.Invoke(this, e);
    }

    private void InvalidateScrollable()
    {
        if (this is not ILogicalScrollable scrollable)
        {
            return;
        }

        if (_element == null)
        {
            return;
        }

        CalculateScrollable(_element.Bounds, this.Bounds.Size, _matrix, out var extent, out var viewport, out var offset);

        Log($"[InvalidateScrollable] _element.Bounds: {_element.Bounds}, _matrix: {_matrix}");
        Log($"[InvalidateScrollable] _extent: {_extent}, extent: {extent}, diff: {extent - _extent}");
        Log($"[InvalidateScrollable] _offset: {_offset}, offset: {offset}, diff: {offset - _offset}");
        Log($"[InvalidateScrollable] _viewport: {_viewport}, viewport: {viewport}, diff: {viewport - _viewport}");

        _extent = extent;
        _offset = offset;
        _viewport = viewport;

        // Temporarily set updating flag to prevent feedback loop when ScrollViewer
        // reads the new offset and sets it back through IScrollable.Offset setter
        var wasUpdating = _updating;
        _updating = true;
        
        scrollable.RaiseScrollInvalidated(EventArgs.Empty);
        
        _updating = wasUpdating;
    }
}
