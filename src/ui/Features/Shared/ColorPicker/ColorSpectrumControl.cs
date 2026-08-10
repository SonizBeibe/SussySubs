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

public class ColorSpectrumControl : Control
{
    public static readonly StyledProperty<int> HueProperty =
        AvaloniaProperty.Register<ColorSpectrumControl, int>(nameof(Hue), 0);

    public static readonly StyledProperty<int> SaturationProperty =
        AvaloniaProperty.Register<ColorSpectrumControl, int>(nameof(Saturation), 100);

    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<ColorSpectrumControl, int>(nameof(Value), 100);

    public int Hue
    {
        get => GetValue(HueProperty);
        set => SetValue(HueProperty, value);
    }

    public int Saturation
    {
        get => GetValue(SaturationProperty);
        set => SetValue(SaturationProperty, value);
    }

    public int Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private bool _isDragging;

    static ColorSpectrumControl()
    {
        AffectsRender<ColorSpectrumControl>(HueProperty, SaturationProperty, ValueProperty);
    }

    public ColorSpectrumControl()
    {
        Width = 256;
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
        double s = Math.Clamp(p.X / Bounds.Width, 0, 1) * 100;
        double v = Math.Clamp(1.0 - (p.Y / Bounds.Height), 0, 1) * 100;

        Saturation = (int)Math.Round(s);
        Value = (int)Math.Round(v);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.Custom(new ColorSpectrumDrawOperation(new Rect(0, 0, Bounds.Width, Bounds.Height), Hue, Saturation, Value));
    }

    private class ColorSpectrumDrawOperation : ICustomDrawOperation
    {
        private readonly Rect _bounds;
        private readonly int _hue;
        private readonly int _saturation;
        private readonly int _value;

        public ColorSpectrumDrawOperation(Rect bounds, int hue, int saturation, int value)
        {
            _bounds = bounds;
            _hue = hue;
            _saturation = saturation;
            _value = value;
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

            // 1. Draw base hue color
            using var paint = new SKPaint();
            var hueColor = SKColor.FromHsv(_hue, 100, 100);

            // Draw horizontal gradient (white to hue)
            using var shaderH = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, 0),
                new[] { SKColors.White, hueColor },
                null,
                SKShaderTileMode.Clamp);

            paint.Shader = shaderH;
            canvas.DrawRect(rect, paint);

            // 2. Draw vertical gradient (transparent to black)
            using var paintV = new SKPaint();
            using var shaderV = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(0, height),
                new[] { SKColors.Transparent, SKColors.Black },
                null,
                SKShaderTileMode.Clamp);
            paintV.Shader = shaderV;
            canvas.DrawRect(rect, paintV);

            // Draw selection indicator
            float px = (_saturation / 100f) * width;
            float py = (1f - (_value / 100f)) * height;

            using var strokePaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                IsAntialias = true
            };
            canvas.DrawCircle(px, py, 5, strokePaint);

            strokePaint.Color = SKColors.Black;
            strokePaint.StrokeWidth = 1;
            canvas.DrawCircle(px, py, 6, strokePaint);
        }
    }
}
