using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Controls;

public class KaraokeBarControl : Grid
{
    public KaraokeBarControl()
    {
        // Placeholder for K-timing UI
        Children.Add(new TextBlock
        {
            Text = "Karaoke Bar Placeholder",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });
    }
}
