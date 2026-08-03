import sys
import re

file_path = "src/ui/Features/Main/Layout/InitListViewAndEditBox.cs"

with open(file_path, "r") as f:
    content = f.read()

# Locate the MakeTextBox(vm) call and insert the new toolbar just before it.
search_str = "var textEditor = MakeTextBox(vm);"
replace_str = """
        var toolbarPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 3,
            Margin = new Thickness(0, 0, 0, 5),
            Children =
            {
                new Button { Content = "Bold" },
                new Button { Content = "Italic" },
                new Button { Content = "Underline" },
                new Button { Content = "Strikethrough" },
                new Avalonia.Controls.Primitives.ToggleButton { Content = "Modo Karaoke" },
                new Button { Content = @"\1c" },
                new Button { Content = @"\2c" },
                new Button { Content = @"\3c" },
                new Button { Content = @"\4c" }
            }
        };
        textEditGrid.Children.Add(toolbarPanel);
        // We might need to adjust rows if we add it to the textEditGrid, or place it inside a DockPanel.
        // Wait, textEditGrid is a Grid. We'd have to manage its Row definitions. Let's look at textEditGrid.
"""

# Let's inspect the setup of textEditGrid first before injecting.
