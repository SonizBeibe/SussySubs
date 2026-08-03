using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public static class SubtitleFormatHelper
{

    public static List<SubtitleFormat> GetSubtitleFormatsWithFavoritesAtTop()
    {
        var allFormats = SubtitleFormat.AllSubtitleFormats;
        var result = new List<SubtitleFormat>();

        var allowedNames = new[]
        {
            "Advanced Sub Station Alpha",
            "SubRip",
            "YouTube SBV",
            "Adobe After Effects"
        };

        var allowedTypes = new[]
        {
            typeof(Nikse.SubtitleEdit.Core.SubtitleFormats.AdvancedSubStationAlpha),
            typeof(Nikse.SubtitleEdit.Core.SubtitleFormats.SubRip),
            typeof(Nikse.SubtitleEdit.Core.SubtitleFormats.YouTubeSbv),
            typeof(Nikse.SubtitleEdit.Core.SubtitleFormats.AdobeAfterEffectsFTME)
        };

        foreach (var allowedType in allowedTypes)
        {
            var format = allFormats.FirstOrDefault(f => f.GetType() == allowedType);
            if (format != null)
            {
                result.Add(format);
            }
        }

        return result;
    }
}
