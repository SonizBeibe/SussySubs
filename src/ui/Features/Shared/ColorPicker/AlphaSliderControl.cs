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

public class AlphaSliderControl : Control
{
    public static readonly StyledProperty<int> AlphaProperty =
        AvaloniaProperty.Register<AlphaSliderControl, int>(nameof(Alpha), 255);

    public static readonly StyledProperty<Color> BaseColorProperty =
        AvaloniaProperty.Register<AlphaSliderControl, Color>(nameof(BaseColor), Colors.White);

    public int Alpha
    {
        get => GetValue(AlphaProperty);
        set => SetValue(AlphaProperty, value);
    }

    public Color BaseColor
    {
        get => GetValue(BaseColorProperty);
        set => SetValue(BaseColorProperty, value);
    }

    private bool _isDragging;

    static AlphaSliderControl()
    {
        AffectsRender<AlphaSliderControl>(AlphaProperty, BaseColorProperty);
    }

    public AlphaSliderControl()
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
        double a = Math.Clamp(1.0 - (p.Y / Bounds.Height), 0, 1) * 255;
        Alpha = (int)Math.Round(a);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.Custom(new AlphaSliderDrawOperation(new Rect(0, 0, Bounds.Width, Bounds.Height), Alpha, BaseColor));
    }

    private class AlphaSliderDrawOperation : ICustomDrawOperation
    {
        private readonly Rect _bounds;
        private readonly int _alpha;
        private readonly Color _baseColor;

        public AlphaSliderDrawOperation(Rect bounds, int alpha, Color baseColor)
        {
            _bounds = bounds;
            _alpha = alpha;
            _baseColor = baseColor;
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

            // Draw checkerboard background
            using var paint = new SKPaint();
            var checkerSize = 8;
            var bm = new SKBitmap(checkerSize * 2, checkerSize * 2);
            using (var bmCanvas = new SKCanvas(bm))
            {
                bmCanvas.Clear(SKColors.White);
                var grayPaint = new SKPaint { Color = SKColors.LightGray };
                bmCanvas.DrawRect(0, 0, checkerSize, checkerSize, grayPaint);
                bmCanvas.DrawRect(checkerSize, checkerSize, checkerSize, checkerSize, grayPaint);
            }
            paint.Shader = SKShader.CreateBitmap(bm, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
            canvas.DrawRect(rect, paint);

            // Draw gradient
            var colorOpaque = new SKColor(_baseColor.R, _baseColor.G, _baseColor.B, 255);
            var colorTransparent = new SKColor(_baseColor.R, _baseColor.G, _baseColor.B, 0);

            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(0, height),
                new[] { colorOpaque, colorTransparent },
                null,
                SKShaderTileMode.Clamp);

            paint.Shader = shader;
            canvas.DrawRect(rect, paint);

            // Draw selection indicator
            float py = (1f - (_alpha / 255f)) * height;

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
