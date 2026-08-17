using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.ColorPicker;

public class ColorPickerWindow : Window
{
    public ColorPickerWindow(ColorPickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ColorPickerTitle;
        CanResize = Se.Settings.General.AllowWindowResizing;
        MinWidth = 750;
        MinHeight = 450;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        vm.Window = this;
        DataContext = vm;

        var colorView = MakeColorView(vm, out var hexTextBox);

        var btnCancel = UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand);
        var btnOk = UiUtil.MakeButton(Se.Language.General.Ok, vm.OkCommand);

        var panelButtons = UiUtil.MakeButtonBar(btnCancel, btnOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = new Thickness(15, 15, 15, 25),
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(colorView, 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate { hexTextBox.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Grid MakeColorView(ColorPickerViewModel vm, out TextBox hexTextBox)
    {
        var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            RowDefinitions = new RowDefinitions("*"),
            Margin = new Thickness(15),
            ColumnSpacing = 20
        };

        // --- Left Side (Visual Spectrum) ---
        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10
        };

        // Target color ComboBox (optional/dummy for now, based on requirements but often useful in context)
        var targetColorCombo = new ComboBox
        {
            ItemsSource = new[] { "Primary Color", "Secondary Color", "Outline Color", "Shadow Color" },
            SelectedIndex = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        // We add it just to match standard editor layout. Might not have a backing property yet.

        var visualGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,10,Auto,10,Auto"),
            RowDefinitions = new RowDefinitions("*")
        };

        // Large 2D color spectrum
        var colorSpectrum = new ColorSpectrumControl
        {
            MinWidth = 256,
            MinHeight = 256,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        colorSpectrum.Bind(ColorSpectrumControl.HueProperty, new Binding(nameof(vm.Hue)) { Mode = BindingMode.TwoWay });
        colorSpectrum.Bind(ColorSpectrumControl.SaturationProperty, new Binding(nameof(vm.HsvSaturation)) { Mode = BindingMode.TwoWay });
        colorSpectrum.Bind(ColorSpectrumControl.ValueProperty, new Binding(nameof(vm.Value)) { Mode = BindingMode.TwoWay });
        Grid.SetColumn(colorSpectrum, 0);

        // Vertical Hue slider
        var hueSlider = new HueSliderControl
        {
            Width = 30,
            Height = 256,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        hueSlider.Bind(HueSliderControl.HueProperty, new Binding(nameof(vm.Hue)) { Mode = BindingMode.TwoWay });
        Grid.SetColumn(hueSlider, 2);

        // Vertical Alpha slider
        var alphaSlider = new AlphaSliderControl
        {
            Width = 30,
            Height = 256,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        alphaSlider.Bind(AlphaSliderControl.AlphaProperty, new Binding(nameof(vm.Alpha)) { Mode = BindingMode.TwoWay });
        alphaSlider.Bind(AlphaSliderControl.BaseColorProperty, new Binding(nameof(vm.SelectedColor)) { Mode = BindingMode.OneWay });
        alphaSlider.Bind(AlphaSliderControl.IsVisibleProperty, new Binding(nameof(vm.ShowAlpha)));
        Grid.SetColumn(alphaSlider, 4);

        visualGrid.Children.Add(colorSpectrum);
        visualGrid.Children.Add(hueSlider);
        visualGrid.Children.Add(alphaSlider);

        var previewBorder = new Border
        {
            Height = 40,
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        previewBorder.Bind(Border.BackgroundProperty, new Binding(nameof(vm.SelectedColor)) { Converter = new ColorToBrushConverter() });

        leftPanel.Children.Add(targetColorCombo);
        leftPanel.Children.Add(visualGrid);
        leftPanel.Children.Add(previewBorder);

        Grid.SetColumn(leftPanel, 0);


        // --- Right Side (Numeric Inputs & Tools) ---
        var rightPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            MinWidth = 250
        };

        var rgbGroup = MakeGroupBox("RGB", CreateRgbPanel(vm, out hexTextBox));
        var formatsGroup = MakeGroupBox("Output Formats", CreateFormatsPanel(vm));

        var hslHsvPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                MakeGroupBox("HSL", CreateHslPanel(vm)),
                MakeGroupBox("HSV", CreateHsvPanel(vm))
            }
        };

        var toolsGroup = MakeGroupBox("Tools & Palette", CreateToolsPanel(vm));

        rightPanel.Children.Add(rgbGroup);
        rightPanel.Children.Add(formatsGroup);
        rightPanel.Children.Add(hslHsvPanel);
        rightPanel.Children.Add(toolsGroup);

        Grid.SetColumn(rightPanel, 1);

        mainGrid.Children.Add(leftPanel);
        mainGrid.Children.Add(rightPanel);

        return mainGrid;
    }

    private static Grid CreateRgbPanel(ColorPickerViewModel vm, out TextBox hexInputTextBox)
    {
        var panel = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, Auto, Auto"),
            ColumnSpacing = 15
        };

        var redInput = MakeNumericUpDown(vm, "R:", nameof(vm.Red), 0, 255);
        var greenInput = MakeNumericUpDown(vm, "G:", nameof(vm.Green), 0, 255);
        var blueInput = MakeNumericUpDown(vm, "B:", nameof(vm.Blue), 0, 255);

        Grid.SetColumn(redInput, 0);
        Grid.SetColumn(greenInput, 1);
        Grid.SetColumn(blueInput, 2);

        panel.Children.Add(redInput);
        panel.Children.Add(greenInput);
        panel.Children.Add(blueInput);

        hexInputTextBox = new TextBox(); // Placeholder since we returned out parameter

        return panel;
    }

    private static StackPanel CreateHslPanel(ColorPickerViewModel vm)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 5 };
        panel.Children.Add(MakeNumericUpDown(vm, "H:", nameof(vm.Hue), 0, 360));
        panel.Children.Add(MakeNumericUpDown(vm, "S:", nameof(vm.HslSaturation), 0, 100));
        panel.Children.Add(MakeNumericUpDown(vm, "L:", nameof(vm.Lightness), 0, 100));
        return panel;
    }

    private static StackPanel CreateHsvPanel(ColorPickerViewModel vm)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 5 };
        panel.Children.Add(MakeNumericUpDown(vm, "H:", nameof(vm.Hue), 0, 360)); // Bind to same Hue
        panel.Children.Add(MakeNumericUpDown(vm, "S:", nameof(vm.HsvSaturation), 0, 100)); // Same Sat
        panel.Children.Add(MakeNumericUpDown(vm, "V:", nameof(vm.Value), 0, 100));
        return panel;
    }

    private static StackPanel CreateFormatsPanel(ColorPickerViewModel vm)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 5 };

        var assaBox = new TextBox { Width = 120, Margin = new Thickness(5, 0, 0, 0) };
        assaBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.AssaString)) { Mode = BindingMode.TwoWay });
        var assaPanel = new StackPanel { Orientation = Orientation.Horizontal };
        assaPanel.Children.Add(new TextBlock { Text = "ASS:", VerticalAlignment = VerticalAlignment.Center, Width = 50 });
        assaPanel.Children.Add(assaBox);

        var htmlBox = new TextBox { Width = 120, Margin = new Thickness(5, 0, 0, 0) };
        htmlBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.HtmlString)) { Mode = BindingMode.TwoWay });
        var htmlPanel = new StackPanel { Orientation = Orientation.Horizontal };
        htmlPanel.Children.Add(new TextBlock { Text = "HTML:", VerticalAlignment = VerticalAlignment.Center, Width = 50 });
        htmlPanel.Children.Add(htmlBox);

        var alphaInput = MakeNumericUpDown(vm, "Alpha:", nameof(vm.Alpha), 0, 255);
        alphaInput.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.ShowAlpha)));

        panel.Children.Add(assaPanel);
        panel.Children.Add(htmlPanel);
        panel.Children.Add(alphaInput);

        return panel;
    }

    private static StackPanel CreateToolsPanel(ColorPickerViewModel vm)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 10 };

        // Dropper button (placeholder logic, usually requires external service, but we just add the button UI)
        var dropperPathData = "M14.39,4.41L15.6,5.63L11.56,9.66L10.35,8.45L14.39,4.41M11.66,2.23C12,2.23 12.41,2.39 12.72,2.7L17.3,7.28C17.92,7.9 17.92,8.9 17.3,9.53L13.84,13H15.11L15.3,13.2L11,17.5V19.46L9.62,20.84L7.5,18.71L2.14,19.34L2.66,13.97L4.79,11.84L6.17,13.22V11L10.47,6.7L10.66,6.89V5.62L14.12,2.16C14.44,1.85 14.84,1.69 15.25,1.69M7.09,14.65C6.96,14.65 6.84,14.7 6.74,14.8L4.35,17.18L3.92,17L3.43,14L5.61,11.83L6.04,11.4L7.09,14.65Z";
        var dropperIcon = new Avalonia.Controls.PathIcon
        {
            Data = Geometry.Parse(dropperPathData),
            Width = 16,
            Height = 16
        };

        var btnDropper = new Button
        {
            Content = dropperIcon,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        ToolTip.SetTip(btnDropper, "Eyedropper");
        // Not binding command yet since it wasn't requested, just the layout

        var colorsGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*,*,*,*,*"),
            HorizontalAlignment = HorizontalAlignment.Left,
            Height = 30,
        };

        var colorBoxes = new[]
        {
            CreateColorBox(vm, nameof(vm.LastColorPickerColor), 0),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor1), 1),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor2), 2),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor3), 3),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor4), 4),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor5), 5),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor6), 6),
            CreateColorBox(vm, nameof(vm.LastColorPickerColor7), 7),
        };

        foreach (var box in colorBoxes)
        {
            colorsGrid.Children.Add(box);
        }

        panel.Children.Add(btnDropper);
        panel.Children.Add(new TextBlock { Text = Se.Language.Tools.RecentColors, Margin = new Thickness(0, 5, 0, 0) });
        panel.Children.Add(colorsGrid);

        return panel;
    }

    private static Border CreateColorBox(ColorPickerViewModel vm, string propertyName, int column)
    {
        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            Margin = new Thickness(2),
            Width = 25,
            Cursor = new Cursor(StandardCursorType.Hand),
        };

        Grid.SetColumn(border, column);

        border.Bind(Border.BackgroundProperty, new Binding
        {
            Path = propertyName,
            Converter = new ColorToBrushConverter(),
        });

        border.PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(border).Properties.IsLeftButtonPressed)
            {
                var color = propertyName switch
                {
                    nameof(vm.LastColorPickerColor) => vm.LastColorPickerColor,
                    nameof(vm.LastColorPickerColor1) => vm.LastColorPickerColor1,
                    nameof(vm.LastColorPickerColor2) => vm.LastColorPickerColor2,
                    nameof(vm.LastColorPickerColor3) => vm.LastColorPickerColor3,
                    nameof(vm.LastColorPickerColor4) => vm.LastColorPickerColor4,
                    nameof(vm.LastColorPickerColor5) => vm.LastColorPickerColor5,
                    nameof(vm.LastColorPickerColor6) => vm.LastColorPickerColor6,
                    nameof(vm.LastColorPickerColor7) => vm.LastColorPickerColor7,
                    _ => Colors.White
                };
                vm.SelectRecentColor(color);
            }
        };

        return border;
    }


    private static Border MakeGroupBox(string header, Control content)
    {
        var border = new Border
        {
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(0, 10, 0, 5),
            Padding = new Thickness(10, 15, 10, 10),
            Child = content
        };

        var headerText = new TextBlock
        {
            Text = header,
            Background = Brushes.Transparent, // Assuming dark theme
            Padding = new Thickness(5, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(10, -10, 0, 0) // Overlap the border
        };

        var panel = new Panel();
        panel.Children.Add(border);
        panel.Children.Add(headerText);

        // Wrap in border just to be safe with type matching
        var wrapper = new Border { Child = panel };
        return wrapper;
    }

    private static StackPanel MakeNumericUpDown(ColorPickerViewModel vm, string label, string propertyName, int min, int max)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
        panel.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Width = 20 });

        var num = UiUtil.MakeNumericUpDownInt(min, max, 0, 60, vm, propertyName);
        num.MinWidth = 65;
        num.MinHeight = 30;
        panel.Children.Add(num);
        return panel;
    }

    }
