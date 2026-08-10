import re

with open('src/ui/Features/Shared/ColorPicker/ColorPickerWindow.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Window Sizing & Margins
# - Set the Window Width="750", Height="450", and CanResize="False"
content = re.sub(r'Width = \d+;', 'Width = 750;', content)
content = re.sub(r'Height = \d+;', 'Height = 450;', content)
content = re.sub(r'CanResize = \w+;', 'CanResize = false;', content)

# Update the main grid row def for star height and correct margin
content = content.replace(
    'new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },\n                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },',
    'new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },\n                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },'
)

# Replace Margin in grid
content = content.replace(
    'Margin = UiUtil.MakeWindowMargin(),',
    'Margin = new Thickness(15),'
)

# - Wrap the main content in a Grid with Margin="15"
# - Use ColumnDefinitions="Auto, *" with a ColumnSpacing="20"
content = re.sub(
    r'var mainGrid = new Grid\s*\{\s*ColumnDefinitions = new ColumnDefinitions\("Auto,\*"\),\s*RowDefinitions = new RowDefinitions\("Auto"\),\s*Margin = new Thickness\(10\)\s*\};',
    '''var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            RowDefinitions = new RowDefinitions("*"),
            Margin = new Thickness(15),
            ColumnSpacing = 20
        };''',
    content
)

# Remove the inner margin in right panel
content = re.sub(
    r'var rightPanel = new StackPanel\s*\{\s*Orientation = Orientation.Vertical,\s*Spacing = 10,\s*MinWidth = 250,\s*Margin = new Thickness\(20, 0, 0, 0\)\s*\};',
    '''var rightPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            MinWidth = 250
        };''',
    content
)

# Fix Squished Controls (Right Column)
content = content.replace(
    '''    private static StackPanel MakeNumericUpDown(ColorPickerViewModel vm, string label, string propertyName, int min, int max)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
        panel.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Width = 20 });

        var num = UiUtil.MakeNumericUpDownInt(min, max, 0, 60, vm, propertyName);
        panel.Children.Add(num);
        return panel;
    }''',
    '''    private static StackPanel MakeNumericUpDown(ColorPickerViewModel vm, string label, string propertyName, int min, int max)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
        panel.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Width = 20 });

        var num = UiUtil.MakeNumericUpDownInt(min, max, 0, 60, vm, propertyName);
        num.MinWidth = 65;
        num.MinHeight = 30;
        panel.Children.Add(num);
        return panel;
    }'''
)


content = content.replace(
    '''    private static StackPanel CreateRgbPanel(ColorPickerViewModel vm, out TextBox hexInputTextBox)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 15 };

        var redInput = MakeNumericUpDown(vm, "R:", nameof(vm.Red), 0, 255);
        var greenInput = MakeNumericUpDown(vm, "G:", nameof(vm.Green), 0, 255);
        var blueInput = MakeNumericUpDown(vm, "B:", nameof(vm.Blue), 0, 255);

        // We only use the input numeric part from MakeNumericUpDown which returns a StackPanel
        panel.Children.Add(redInput);
        panel.Children.Add(greenInput);
        panel.Children.Add(blueInput);

        hexInputTextBox = new TextBox(); // Placeholder since we returned out parameter

        return panel;
    }''',
    '''    private static Grid CreateRgbPanel(ColorPickerViewModel vm, out TextBox hexInputTextBox)
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
    }'''
)


with open('src/ui/Features/Shared/ColorPicker/ColorPickerWindow.cs', 'w', encoding='utf-8') as f:
    f.write(content)
