import re
with open('src/ui/Features/Shared/ColorPicker/ColorPickerWindow.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
'''var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            RowDefinitions = new RowDefinitions("*"),
            Margin = new Thickness(15)
        };''',
'''var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            RowDefinitions = new RowDefinitions("*"),
            Margin = new Thickness(15),
            ColumnSpacing = 20
        };'''
)

with open('src/ui/Features/Shared/ColorPicker/ColorPickerWindow.cs', 'w', encoding='utf-8') as f:
    f.write(content)
