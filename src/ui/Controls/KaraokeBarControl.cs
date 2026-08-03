using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Controls;

public class KaraokeBarControl : Grid
{
    private MainViewModel? _vm;
    private ComboBox? _tagComboBox;
    private readonly WrapPanel _panel;

    public KaraokeBarControl()
    {
        _panel = new WrapPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        Children.Add(_panel);
    }

    public void Setup(MainViewModel vm, ComboBox tagComboBox)
    {
        _vm = vm;
        _tagComboBox = tagComboBox;
        _vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_vm.EditText))
                RenderSyllables();
        };
        RenderSyllables();
    }

    private void RenderSyllables()
    {
        _panel.Children.Clear();
        if (_vm == null || string.IsNullOrEmpty(_vm.EditText)) return;

        var text = _vm.EditText;
        var matches = Regex.Matches(text, @"\{[^}]*\\[kK][fo]?(\d+)[^}]*\}");

        int lastIdx = 0;
        for (int i = 0; i <= matches.Count; i++)
        {
            int start = lastIdx;
            int end = i < matches.Count ? matches[i].Index : text.Length;
            if (end > start)
            {
                var rawSyllableText = text.Substring(start, end - start);
                var displaySyllableText = Regex.Replace(rawSyllableText, @"\{[^}]*\}", ""); // Remove any trailing curly brace tags for display

                var border = new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#33888888")),
                    Margin = new Thickness(1),
                    Padding = new Thickness(4, 2)
                };

                var stack = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center };
                var textBlock = new TextBlock { Text = displaySyllableText, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                stack.Children.Add(textBlock);

                // Try to get duration of the previous tag, since in ASS the tag precedes the syllable
                if (i > 0 && i - 1 < matches.Count)
                {
                    var durText = new TextBlock { Text = matches[i - 1].Groups[1].Value, FontSize = 10, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center };
                    stack.Children.Add(durText);
                }

                border.Child = stack;

                int currentStringIndex = start;
                int currentMatchIndex = i;
                border.PointerPressed += (s, e) => OnSyllableClicked(currentStringIndex, rawSyllableText, currentMatchIndex);
                _panel.Children.Add(border);
            }

            if (i < matches.Count)
            {
                var match = matches[i];
                var sepBorder = new Border
                {
                    Background = Brushes.Transparent,
                    Width = 6,
                    Cursor = new Cursor(StandardCursorType.Hand)
                };
                var sepLine = new Border { Width = 2, Background = Brushes.Yellow, HorizontalAlignment = HorizontalAlignment.Center };
                sepBorder.Child = sepLine;
                sepBorder.PointerPressed += (s, e) => OnSeparatorClicked(match);
                _panel.Children.Add(sepBorder);
                lastIdx = match.Index + match.Length;
            }
        }
    }

    private void OnSyllableClicked(int startIdx, string text, int matchIndex)
    {
        if (_vm == null) return;
        var tag = _tagComboBox?.SelectedItem?.ToString() ?? "\\k";

        int newDur = 0;
        int remainingDur = 0;
        Match? prevMatch = null;
        var allMatches = Regex.Matches(_vm.EditText, @"\{[^}]*\\[kK][fo]?(\d+)[^}]*\}");
        if (matchIndex > 0 && matchIndex - 1 < allMatches.Count)
        {
            prevMatch = allMatches[matchIndex - 1];
            if (int.TryParse(prevMatch.Groups[1].Value, out int fullDur))
            {
                newDur = fullDur / 2;
                remainingDur = fullDur - newDur;
            }
        }

        var insertIdx = startIdx + (text.Length / 2);
        string newText = _vm.EditText.Insert(insertIdx, "{" + tag + newDur + "}");

        if (prevMatch != null && remainingDur > 0)
        {
            newText = newText.Remove(prevMatch.Groups[1].Index, prevMatch.Groups[1].Length).Insert(prevMatch.Groups[1].Index, remainingDur.ToString());
        }
        else if (prevMatch == null)
        {
             newText = _vm.EditText.Insert(insertIdx, "{" + tag + "0}");
        }

        _vm.EditTextBox.Text = newText;
    }

    private void OnSeparatorClicked(Match match)
    {
        if (_vm == null) return;

        int durToRemove = int.Parse(match.Groups[1].Value);

        var innerMatch = Regex.Match(match.Value, @"\\[kK][fo]?\d+");
        if (!innerMatch.Success) return;

        int removeStart = match.Index + innerMatch.Index;
        int removeLen = innerMatch.Length;

        var newText = _vm.EditText.Remove(removeStart, removeLen);

        // Clean up empty {} if any
        newText = newText.Replace("{}", "");

        // Find previous tag to add duration to it
        var prevMatches = Regex.Matches(newText.Substring(0, removeStart > newText.Length ? newText.Length : removeStart), @"\{[^}]*\\[kK][fo]?(\d+)[^}]*\}");
        if (prevMatches.Count > 0)
        {
            var prevMatch = prevMatches[prevMatches.Count - 1];
            if (int.TryParse(prevMatch.Groups[1].Value, out int prevDur))
            {
                var newDur = prevDur + durToRemove;
                newText = newText.Remove(prevMatch.Groups[1].Index, prevMatch.Groups[1].Length).Insert(prevMatch.Groups[1].Index, newDur.ToString());
            }
        }

        _vm.EditTextBox.Text = newText;
    }
}
