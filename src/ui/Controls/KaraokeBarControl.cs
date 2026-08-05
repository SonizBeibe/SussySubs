using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Nikse.SubtitleEdit.Controls;

public class KaraokeBarControl : Grid
{
    private MainViewModel? _vm;
    private ComboBox? _tagComboBox;
    private readonly WrapPanel _panel;
    private INotifyPropertyChanged? _currentSubtitle;

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
            if (e.PropertyName == nameof(_vm.SelectedSubtitle))
            {
                UpdateSubtitleSubscription();
                RenderSyllables();
            }
        };

        UpdateSubtitleSubscription();
        RenderSyllables();
    }

    private void UpdateSubtitleSubscription()
    {
        if (_currentSubtitle != null)
        {
            _currentSubtitle.PropertyChanged -= OnSubtitlePropertyChanged;
        }

        _currentSubtitle = _vm?.SelectedSubtitle as INotifyPropertyChanged;

        if (_currentSubtitle != null)
        {
            _currentSubtitle.PropertyChanged += OnSubtitlePropertyChanged;
        }
    }

    private void OnSubtitlePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Text")
        {
            RenderSyllables();
        }
    }

    private void RenderSyllables()
    {
        _panel.Children.Clear();
        if (_vm?.SelectedSubtitle == null || string.IsNullOrEmpty(_vm.SelectedSubtitle.Text)) return;

        var text = _vm.SelectedSubtitle.Text;
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
                border.PointerPressed += (s, e) => OnSyllableClicked(currentStringIndex, rawSyllableText, currentMatchIndex, e, border);
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

    private void OnSyllableClicked(int startIdx, string text, int matchIndex, PointerPressedEventArgs e, Border border)
    {
        if (_vm?.SelectedSubtitle == null) return;
        var tag = _tagComboBox?.SelectedItem?.ToString() ?? "\\k";

        int newDur = 0;
        int remainingDur = 0;
        Match? prevMatch = null;
        var allMatches = Regex.Matches(_vm.SelectedSubtitle.Text, @"\{[^}]*\\[kK][fo]?(\d+)[^}]*\}");
        if (matchIndex > 0 && matchIndex - 1 < allMatches.Count)
        {
            prevMatch = allMatches[matchIndex - 1];
            if (int.TryParse(prevMatch.Groups[1].Value, out int fullDur))
            {
                var pos = e.GetPosition(border);
                double ratio = pos.X / border.Bounds.Width;
                ratio = System.Math.Max(0.0, System.Math.Min(1.0, ratio));

                newDur = (int)System.Math.Round(fullDur * ratio);
                remainingDur = fullDur - newDur;
            }
        }

        if (prevMatch == null)
        {
            return; // Do not insert \k0 tags before the first valid tag
        }

        double charRatio = border.Bounds.Width > 0 ? e.GetPosition(border).X / border.Bounds.Width : 0.5;
        charRatio = System.Math.Max(0.0, System.Math.Min(1.0, charRatio));
        var insertIdx = startIdx + (int)System.Math.Round(text.Length * charRatio);
        string newText = _vm.SelectedSubtitle.Text.Insert(insertIdx, "{" + tag + remainingDur + "}");

        if (remainingDur >= 0)
        {
            newText = newText.Remove(prevMatch.Groups[1].Index, prevMatch.Groups[1].Length).Insert(prevMatch.Groups[1].Index, newDur.ToString());
        }

        _vm.SelectedSubtitle.Text = newText;
        _vm.SubtitleTextChanged(null, null);
    }

    private void OnSeparatorClicked(Match match)
    {
        if (_vm?.SelectedSubtitle == null) return;

        int durToRemove = int.Parse(match.Groups[1].Value);

        var innerMatch = Regex.Match(match.Value, @"\\[kK][fo]?\d+");
        if (!innerMatch.Success) return;

        int removeStart = match.Index + innerMatch.Index;
        int removeLen = innerMatch.Length;

        var newText = _vm.SelectedSubtitle.Text.Remove(removeStart, removeLen);

        // Find previous tag to add duration to it BEFORE cleaning up empty {} so index is reliable
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

        // Clean up empty {} if any
        newText = newText.Replace("{}", "");

        _vm.SelectedSubtitle.Text = newText;
        _vm.SubtitleTextChanged(null, null);
    }
}
