using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.ColorPicker;

public partial class ColorPickerViewModel : ObservableObject
{
    [ObservableProperty] private Color _selectedColor = Colors.White;

    [ObservableProperty] private byte _red = 255;
    [ObservableProperty] private byte _green = 255;
    [ObservableProperty] private byte _blue = 255;
    [ObservableProperty] private byte _alpha = 255;

    [ObservableProperty] private int _hue = 0;
    [ObservableProperty] private int _hsvSaturation = 0;
    [ObservableProperty] private int _hslSaturation = 0;
    [ObservableProperty] private int _lightness = 100;
    [ObservableProperty] private int _value = 100;

    [ObservableProperty] private string _hexColor = "FFFFFFFF";
    [ObservableProperty] private string _assaString = "&H00FFFFFF";
    [ObservableProperty] private string _htmlString = "#FFFFFFFF";

    [ObservableProperty] private Color _redGradientStart = Colors.Black;
    [ObservableProperty] private Color _redGradientEnd = Colors.Red;
    [ObservableProperty] private Color _greenGradientStart = Colors.Black;
    [ObservableProperty] private Color _greenGradientEnd = Colors.Green;
    [ObservableProperty] private Color _blueGradientStart = Colors.Black;
    [ObservableProperty] private Color _blueGradientEnd = Colors.Blue;
    [ObservableProperty] private Color _alphaGradientStart = Colors.Transparent;
    [ObservableProperty] private Color _alphaGradientEnd = Colors.White;

    [ObservableProperty] private bool _showAlpha = true;

    [ObservableProperty] private Color _lastColorPickerColor;
    [ObservableProperty] private Color _lastColorPickerColor1;
    [ObservableProperty] private Color _lastColorPickerColor2;
    [ObservableProperty] private Color _lastColorPickerColor3;
    [ObservableProperty] private Color _lastColorPickerColor4;
    [ObservableProperty] private Color _lastColorPickerColor5;
    [ObservableProperty] private Color _lastColorPickerColor6;
    [ObservableProperty] private Color _lastColorPickerColor7;
    [ObservableProperty] private Color _lastColorPickerDropper;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private bool _isUpdating;

    public ColorPickerViewModel()
    {
        LoadSettings();
    }

    public void Initialize(Color initialColor)
    {
        _isUpdating = true;
        SelectedColor = initialColor;
        UpdateFromColor(initialColor);
        _isUpdating = false;
    }

    partial void OnRedChanged(byte value)
    {
        if (!_isUpdating)
        {
            UpdateColorFromRgb();
        }
    }

    partial void OnGreenChanged(byte value)
    {
        if (!_isUpdating)
        {
            UpdateColorFromRgb();
        }
    }

    partial void OnBlueChanged(byte value)
    {
        if (!_isUpdating)
        {
            UpdateColorFromRgb();
        }
    }

    partial void OnAlphaChanged(byte value)
    {
        if (!_isUpdating)
        {
            UpdateColorFromRgb();
        }
    }

    partial void OnHueChanged(int value)
    {
        if (!_isUpdating) UpdateColorFromHsv();
    }

    partial void OnHsvSaturationChanged(int value)
    {
        if (!_isUpdating) UpdateColorFromHsv();
    }

    partial void OnHslSaturationChanged(int value)
    {
        if (!_isUpdating) UpdateColorFromHsl();
    }

    partial void OnLightnessChanged(int value)
    {
        if (!_isUpdating) UpdateColorFromHsl();
    }

    partial void OnValueChanged(int value)
    {
        if (!_isUpdating) UpdateColorFromHsv();
    }

    partial void OnHexColorChanged(string value)
    {
        if (!_isUpdating && !string.IsNullOrWhiteSpace(value))
        {
            try
            {
                var hexValue = value.TrimStart('#');
                if (hexValue.Length == 6 || hexValue.Length == 8)
                {
                    var color = ("#" + hexValue).FromHexToColor();
                    _isUpdating = true;
                    SelectedColor = color;
                    UpdateFromColor(color);
                    _isUpdating = false;
                }
            }
            catch
            {
                // Invalid hex color, ignore
            }
        }
    }

    partial void OnAssaStringChanged(string value)
    {
        if (!_isUpdating && !string.IsNullOrWhiteSpace(value))
        {
            try
            {
                // ASS format: &HAABBGGRR or &HBBGGRR
                var hexValue = value.TrimStart('&', 'H').TrimEnd('&');
                if (hexValue.Length == 6)
                {
                    byte b = Convert.ToByte(hexValue.Substring(0, 2), 16);
                    byte g = Convert.ToByte(hexValue.Substring(2, 2), 16);
                    byte r = Convert.ToByte(hexValue.Substring(4, 2), 16);
                    var color = Color.FromArgb(Alpha, r, g, b);
                    _isUpdating = true;
                    SelectedColor = color;
                    UpdateFromColor(color);
                    _isUpdating = false;
                }
                else if (hexValue.Length == 8)
                {
                    byte a = Convert.ToByte(hexValue.Substring(0, 2), 16);
                    byte b = Convert.ToByte(hexValue.Substring(2, 2), 16);
                    byte g = Convert.ToByte(hexValue.Substring(4, 2), 16);
                    byte r = Convert.ToByte(hexValue.Substring(6, 2), 16);
                    var color = Color.FromArgb(a, r, g, b);
                    _isUpdating = true;
                    SelectedColor = color;
                    UpdateFromColor(color);
                    _isUpdating = false;
                }
            }
            catch
            {
                // Ignore
            }
        }
    }

    partial void OnHtmlStringChanged(string value)
    {
        OnHexColorChanged(value);
    }

    public void UpdateFromColorWheel(Color color)
    {
        if (!_isUpdating)
        {
            _isUpdating = true;
            SelectedColor = Color.FromArgb(Alpha, color.R, color.G, color.B);
            UpdateFromColor(SelectedColor);
            _isUpdating = false;
        }
    }

    public void SelectRecentColor(Color color)
    {
        if (!_isUpdating)
        {
            _isUpdating = true;
            SelectedColor = color;
            UpdateFromColor(color);
            _isUpdating = false;
        }
    }

    private void UpdateColorFromRgb()
    {
        _isUpdating = true;
        SelectedColor = Color.FromArgb(Alpha, Red, Green, Blue);
        OnPropertyChanged(nameof(SelectedColor));
        UpdateHexColor();
        UpdateHslHsvFromColor(SelectedColor);
        _isUpdating = false;
    }

    private void UpdateColorFromHsv()
    {
        _isUpdating = true;
        var color = HsvToColor(Alpha, Hue, HsvSaturation, Value);
        SelectedColor = color;
        Red = color.R;
        Green = color.G;
        Blue = color.B;
        OnPropertyChanged(nameof(SelectedColor));
        UpdateHexColor();

        // Also update HSL to match
        var hsl = RgbToHsl(SelectedColor);
        Lightness = hsl.Lightness;

        _isUpdating = false;
    }

    private void UpdateColorFromHsl()
    {
        _isUpdating = true;
        var color = HslToColor(Alpha, Hue, HslSaturation, Lightness);
        SelectedColor = color;
        Red = color.R;
        Green = color.G;
        Blue = color.B;
        OnPropertyChanged(nameof(SelectedColor));
        UpdateHexColor();

        // Also update HSV to match
        var hsv = RgbToHsv(SelectedColor);
        Value = hsv.Value;

        _isUpdating = false;
    }

    private void UpdateFromColor(Color color)
    {
        Red = color.R;
        OnPropertyChanged(nameof(Red));
        Green = color.G;
        OnPropertyChanged(nameof(Green));
        Blue = color.B;
        OnPropertyChanged(nameof(Blue));
        Alpha = color.A;
        OnPropertyChanged(nameof(Alpha));
        UpdateHexColor();
        UpdateHslHsvFromColor(color);
    }

    private void UpdateHslHsvFromColor(Color color)
    {
        var hsv = RgbToHsv(color);
        Hue = hsv.Hue;
        HsvSaturation = hsv.Saturation;
        Value = hsv.Value;

        var hsl = RgbToHsl(color);
        HslSaturation = hsl.Saturation;
        Lightness = hsl.Lightness;
    }

    private void UpdateHexColor()
    {
        // Only include the alpha byte (AARRGGBB) when an alpha/opacity channel is shown;
        // otherwise the hex is a plain 6-char RRGGBB value. (#11342 follow-up)
        var hex = SelectedColor.FromColorToHex(ShowAlpha).TrimStart('#');
        HexColor = hex;
        OnPropertyChanged(nameof(HexColor));

        HtmlString = "#" + hex;

        if (ShowAlpha)
        {
            AssaString = $"&H{SelectedColor.A:X2}{SelectedColor.B:X2}{SelectedColor.G:X2}{SelectedColor.R:X2}";
        }
        else
        {
            AssaString = $"&H{SelectedColor.B:X2}{SelectedColor.G:X2}{SelectedColor.R:X2}";
        }
    }

    partial void OnShowAlphaChanged(bool value)
    {
        // ShowAlpha is set after Initialize(), so refresh the hex to match the channel count.
        UpdateHexColor();
    }

    private void LoadSettings()
    {
        LastColorPickerColor = Se.Settings.Tools.LastColorPickerColor.FromHexToColor();
        LastColorPickerColor1 = Se.Settings.Tools.LastColorPickerColor1.FromHexToColor();
        LastColorPickerColor2 = Se.Settings.Tools.LastColorPickerColor2.FromHexToColor();
        LastColorPickerColor3 = Se.Settings.Tools.LastColorPickerColor3.FromHexToColor();
        LastColorPickerColor4 = Se.Settings.Tools.LastColorPickerColor4.FromHexToColor();
        LastColorPickerColor5 = Se.Settings.Tools.LastColorPickerColor5.FromHexToColor();
        LastColorPickerColor6 = Se.Settings.Tools.LastColorPickerColor6.FromHexToColor();
        LastColorPickerColor7 = Se.Settings.Tools.LastColorPickerColor7.FromHexToColor();
    }

    private void SaveSettings()
    {
        var color = SelectedColor.FromColorToHex();
        var colorList = new List<string>
        {
            Se.Settings.Tools.LastColorPickerColor,
            Se.Settings.Tools.LastColorPickerColor1,
            Se.Settings.Tools.LastColorPickerColor2,
            Se.Settings.Tools.LastColorPickerColor3,
            Se.Settings.Tools.LastColorPickerColor4,
            Se.Settings.Tools.LastColorPickerColor5,
            Se.Settings.Tools.LastColorPickerColor6,
            Se.Settings.Tools.LastColorPickerColor7,
        };

        colorList = colorList.Where(c => c != color).ToList();
        var random = new Random();
        while (colorList.Count < 7)
        {
            colorList.Add(
                new Color(
                    255,
                    (byte)random.Next(256),
                    (byte)random.Next(256),
                    (byte)random.Next(256)
                ).FromColorToHex()
            );
        }

        Se.Settings.Tools.LastColorPickerColor = color;
        Se.Settings.Tools.LastColorPickerColor1 = colorList[0];
        Se.Settings.Tools.LastColorPickerColor2 = colorList[1];
        Se.Settings.Tools.LastColorPickerColor3 = colorList[2];
        Se.Settings.Tools.LastColorPickerColor4 = colorList[3];
        Se.Settings.Tools.LastColorPickerColor5 = colorList[4];
        Se.Settings.Tools.LastColorPickerColor6 = colorList[5];
        Se.Settings.Tools.LastColorPickerColor7 = colorList[6];

        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        // Shift the last colors
        LastColorPickerColor7 = LastColorPickerColor6;
        LastColorPickerColor6 = LastColorPickerColor5;
        LastColorPickerColor5 = LastColorPickerColor4;
        LastColorPickerColor4 = LastColorPickerColor3;
        LastColorPickerColor3 = LastColorPickerColor2;
        LastColorPickerColor2 = LastColorPickerColor1;
        LastColorPickerColor1 = LastColorPickerColor;
        LastColorPickerColor = SelectedColor;

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            var hexColor = "#" + SelectedColor.FromColorToHex(true);
            Dispatcher.UIThread.Post(async () =>
            {
                if (Window == null || Window.Clipboard == null)
                {
                    return;
                }

                await ClipboardHelper.SetTextAsync(Window, hexColor);
            });
        }
        else if (e.Key == Key.V && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            Dispatcher.UIThread.Post(async () =>
            {
                if (Window == null || Window.Clipboard == null)
                {
                    return;
                }

                var clipboardText = await ClipboardHelper.GetTextAsync(Window);
                if (!string.IsNullOrWhiteSpace(clipboardText))
                {
                    try
                    {
                        var color = clipboardText.FromHexToColor();
                        _isUpdating = true;
                        SelectedColor = color;
                        UpdateFromColor(color);
                        _isUpdating = false;
                    }
                    catch
                    {
                        // Invalid hex color, ignore
                    }
                }
            });
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    private static Color HsvToColor(byte alpha, int hue, int saturation, int value)
    {
        double r = 0, g = 0, b = 0;

        var h = ((double)hue / 255 * 360) % 360;
        var s = (double)saturation / 100;
        var v = (double)value / 100;

        if (Math.Abs(s) < 0.01)
        {
            r = g = b = v;
        }
        else
        {
            var sectorPos = h / 60;
            var sectorNumber = (int)Math.Floor(sectorPos);
            var fractionalSector = sectorPos - sectorNumber;

            var p = v * (1 - s);
            var q = v * (1 - (s * fractionalSector));
            var t = v * (1 - (s * (1 - fractionalSector)));

            switch (sectorNumber)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }
        }

        return Color.FromArgb(alpha, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    private static (int Hue, int Saturation, int Value) RgbToHsv(Color color)
    {
        var r = (double)color.R / 255;
        var g = (double)color.G / 255;
        var b = (double)color.B / 255;

        var min = Math.Min(Math.Min(r, g), b);
        var max = Math.Max(Math.Max(r, g), b);

        double h, s;
        var v = max;
        var delta = max - min;

        if (Math.Abs(max) < 0.01 || Math.Abs(delta) < 0.01)
        {
            s = 0;
            h = 0;
        }
        else
        {
            s = delta / max;
            if (Math.Abs(r - max) < 0.01)
            {
                h = (g - b) / delta;
            }
            else if (Math.Abs(g - max) < 0.01)
            {
                h = 2 + (b - r) / delta;
            }
            else
            {
                h = 4 + (r - g) / delta;
            }
        }

        h *= 60;
        if (h < 0)
        {
            h += 360;
        }

        return ((int)Math.Round(h), (int)Math.Round(s * 100), (int)Math.Round(v * 100));
    }

    private static Color HslToColor(byte alpha, int hue, int saturation, int lightness)
    {
        double h = hue / 360.0;
        double s = saturation / 100.0;
        double l = lightness / 100.0;

        double r = l, g = l, b = l;
        double v = (l <= 0.5) ? (l * (1.0 + s)) : (l + s - l * s);

        if (v > 0)
        {
            double m = l + l - v;
            double sv = (v - m) / v;
            h *= 6.0;
            int sextant = (int)h;
            double fract = h - sextant;
            double vsf = v * sv * fract;
            double mid1 = m + vsf;
            double mid2 = v - vsf;
            switch (sextant)
            {
                case 0:
                case 6:
                    r = v; g = mid1; b = m; break;
                case 1:
                    r = mid2; g = v; b = m; break;
                case 2:
                    r = m; g = v; b = mid1; break;
                case 3:
                    r = m; g = mid2; b = v; break;
                case 4:
                    r = mid1; g = m; b = v; break;
                case 5:
                    r = v; g = m; b = mid2; break;
            }
        }
        return Color.FromArgb(alpha, (byte)Math.Round(r * 255.0), (byte)Math.Round(g * 255.0), (byte)Math.Round(b * 255.0));
    }

    private static (int Hue, int Saturation, int Lightness) RgbToHsl(Color color)
    {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        double min = Math.Min(r, Math.Min(g, b));
        double max = Math.Max(r, Math.Max(g, b));
        double delta = max - min;

        double h = 0;
        double s = 0;
        double l = (max + min) / 2.0;

        if (delta > 0)
        {
            s = (l <= 0.5) ? (delta / (max + min)) : (delta / (2.0 - max - min));

            if (r == max) h = (g - b) / delta;
            else if (g == max) h = 2.0 + (b - r) / delta;
            else h = 4.0 + (r - g) / delta;

            h *= 60.0;
            if (h < 0) h += 360.0;
        }

        return ((int)Math.Round(h), (int)Math.Round(s * 100), (int)Math.Round(l * 100));
    }
}
