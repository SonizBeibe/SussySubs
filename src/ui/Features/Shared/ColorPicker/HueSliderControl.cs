using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.ColorPicker;

public class HueSliderControl : Control
{
    public static readonly StyledProperty<int> HueProperty =
        AvaloniaProperty.Register<HueSliderControl, int>(nameof(Hue), 0);

    public int Hue
    {
        get => GetValue(HueProperty);
        set => SetValue(HueProperty, value);
    }

    private bool _isDragging;

    static HueSliderControl()
    {
        AffectsRender<HueSliderControl>(HueProperty);
    }

    public HueSliderControl()
    {
        Width = 30;
        Height = 256;
        ClipToBounds = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _isDragging = true;
        UpdateFromPoint(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isDragging)
        {
            UpdateFromPoint(e.GetPosition(this));
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isDragging = false;
        e.Handled = true;
    }

    private void UpdateFromPoint(Point p)
    {
        double h = Math.Clamp(1.0 - (p.Y / Bounds.Height), 0, 1) * 360;
        Hue = (int)Math.Round(h);
        if (Hue == 360) Hue = 0;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.Custom(new HueSliderDrawOperation(new Rect(0, 0, Bounds.Width, Bounds.Height), Hue));
    }

    private class HueSliderDrawOperation : ICustomDrawOperation
    {
        private readonly Rect _bounds;
        private readonly int _hue;

        public HueSliderDrawOperation(Rect bounds, int hue)
        {
            _bounds = bounds;
            _hue = hue;
        }

        public void Dispose() { }

        public Rect Bounds => _bounds;

        public bool HitTest(Point p) => _bounds.Contains(p);

        public bool Equals(ICustomDrawOperation? other) => false;

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null) return;

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            var width = (float)_bounds.Width;
            var height = (float)_bounds.Height;
            var rect = new SKRect(0, 0, width, height);

            using var paint = new SKPaint();
            var colors = new[]
            {
                SKColor.FromHsv(360, 100, 100),
                SKColor.FromHsv(300, 100, 100),
                SKColor.FromHsv(240, 100, 100),
                SKColor.FromHsv(180, 100, 100),
                SKColor.FromHsv(120, 100, 100),
                SKColor.FromHsv(60, 100, 100),
                SKColor.FromHsv(0, 100, 100)
            };

            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(0, height),
                colors,
                null,
                SKShaderTileMode.Clamp);

            paint.Shader = shader;
            canvas.DrawRect(rect, paint);

            // Draw selection indicator
            float py = (1f - (_hue / 360f)) * height;

            using var strokePaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                IsAntialias = true
            };

            // Draw horizontal line for indicator
            canvas.DrawLine(0, py, width, py, strokePaint);

            strokePaint.Color = SKColors.White;
            strokePaint.StrokeWidth = 1;
            canvas.DrawLine(0, py - 1, width, py - 1, strokePaint);
            canvas.DrawLine(0, py + 1, width, py + 1, strokePaint);
        }
    }
}
