using System;
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
    private readonly WrapPanel _panel;
    private readonly Canvas _overlayCanvas;
    private readonly Border _guideLine;
    private INotifyPropertyChanged? _currentSubtitle;

    public KaraokeBarControl()
    {
        _panel = new WrapPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        _overlayCanvas = new Canvas { IsHitTestVisible = false };
        _guideLine = new Border
        {
            Background = Brushes.Red,
            Width = 1,
            IsVisible = false
        };
        _overlayCanvas.Children.Add(_guideLine);

        var innerGrid = new Grid();
        innerGrid.Children.Add(_panel);
        innerGrid.Children.Add(_overlayCanvas);

        Children.Add(innerGrid);
    }

    public void Setup(MainViewModel vm)
    {
        _vm = vm;

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

    private string GetTag()
    {
        return "\\k";
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
                    Padding = new Thickness(12, 2)
                };

                var stack = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center };
                var textBlock = new TextBlock
                {
                    Text = displaySyllableText,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    LetterSpacing = 2
                };
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

                border.PointerMoved += (s, e) =>
                {
                    var point = e.GetPosition(_overlayCanvas);
                    Canvas.SetLeft(_guideLine, point.X);
                    Canvas.SetTop(_guideLine, border.Bounds.Top);
                    _guideLine.Height = border.Bounds.Height;
                    _guideLine.IsVisible = true;
                };

                border.PointerExited += (s, e) =>
                {
                    _guideLine.IsVisible = false;
                };

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
        var tag = GetTag();

        if (!Regex.IsMatch(_vm.SelectedSubtitle.Text, @"\{[^}]*\\[kK][fo]?(\d+)[^}]*\}"))
        {
            int totalCentiseconds = (int)System.Math.Round(_vm.SelectedSubtitle.Duration.TotalMilliseconds / 10.0);
            string initText = "{" + tag + totalCentiseconds + "}" + _vm.SelectedSubtitle.Text;
            ActualizarTexto(initText);
            return;
        }

        int newDur = 0;
        int remainingDur = 0;
        Match? prevMatch = null;
        var allMatches = Regex.Matches(_vm.SelectedSubtitle.Text, @"\{[^}]*\\[kK][fo]?(\d+)[^}]*\}");

        double totalMs = _vm.SelectedSubtitle.StartTime.TotalMilliseconds;
        for (int j = 0; j < matchIndex; j++)
        {
            if (int.TryParse(allMatches[j].Groups[1].Value, out int dur))
            {
                totalMs += dur * 10;
            }
        }
        _vm.ActiveSyllableStartTime = TimeSpan.FromMilliseconds(totalMs);

        if (matchIndex < allMatches.Count && int.TryParse(allMatches[matchIndex].Groups[1].Value, out int clickedDur))
        {
            _vm.ActiveSyllableEndTime = TimeSpan.FromMilliseconds(totalMs + (clickedDur * 10));
        }
        else
        {
            _vm.ActiveSyllableEndTime = _vm.SelectedSubtitle.EndTime;
        }

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

                if (fullDur > 1)
                {
                    if (newDur == 0)
                    {
                        newDur = 1;
                        remainingDur = fullDur - 1;
                    }
                    else if (remainingDur == 0)
                    {
                        remainingDur = 1;
                        newDur = fullDur - 1;
                    }
                }
            }
        }

        if (prevMatch == null)
        {
            return; // Do not insert \k0 tags before the first valid tag
        }

        double charRatio = border.Bounds.Width > 0 ? e.GetPosition(border).X / border.Bounds.Width : 0.5;
        charRatio = System.Math.Max(0.0, System.Math.Min(1.0, charRatio));

        var cleanSyllableText = Regex.Replace(text, @"\{[^}]*\}", "");
        int targetCleanIndex = (int)System.Math.Round(cleanSyllableText.Length * charRatio);
        int cleanCount = 0;
        int rawInsertIndex = startIdx;
        bool insideTag = false;
        var fullText = _vm.SelectedSubtitle.Text;

        while (rawInsertIndex < fullText.Length && cleanCount < targetCleanIndex)
        {
            if (fullText[rawInsertIndex] == '{') insideTag = true;
            if (!insideTag) cleanCount++;
            if (fullText[rawInsertIndex] == '}') insideTag = false;
            rawInsertIndex++;
        }

        string newText = fullText.Insert(rawInsertIndex, "{" + tag + remainingDur + "}");

        if (remainingDur >= 0)
        {
            newText = newText.Remove(prevMatch.Groups[1].Index, prevMatch.Groups[1].Length).Insert(prevMatch.Groups[1].Index, newDur.ToString());
        }

        ActualizarTexto(newText);
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

        ActualizarTexto(newText);
    }

    private void ActualizarTexto(string newText)
    {
        if (_vm?.SelectedSubtitle != null)
        {
            _vm.SelectedSubtitle.Text = newText;
            _vm.SubtitleTextChanged(null, null);
        }
    }
}
