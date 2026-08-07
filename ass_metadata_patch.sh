cat << 'PATCH_EOF' > patch_ass.diff
--- src/libse/SubtitleFormats/AdvancedSubStationAlpha.cs
+++ src/libse/SubtitleFormats/AdvancedSubStationAlpha.cs
@@ -1584,6 +1584,10 @@
             var errors = new StringBuilder();
             var lineNumber = 0;

+            string? playResX = null;
+            string? playResY = null;
+            string? videoFile = null;
+            string? audioFile = null;
             var header = new StringBuilder();
             var footer = new StringBuilder();
             var textBuilder = new StringBuilder();
@@ -1600,6 +1604,23 @@
                     header.AppendLine(line);
                 }

+                if (!eventsStarted && !fontsStarted && !graphicsStarted)
+                {
+                    if (trimmedLine.StartsWith("PlayResX:", StringComparison.OrdinalIgnoreCase))
+                    {
+                        playResX = line;
+                    }
+                    else if (trimmedLine.StartsWith("PlayResY:", StringComparison.OrdinalIgnoreCase))
+                    {
+                        playResY = line;
+                    }
+                    else if (trimmedLine.StartsWith("Video File:", StringComparison.OrdinalIgnoreCase))
+                    {
+                        videoFile = line;
+                    }
+                    else if (trimmedLine.StartsWith("Audio File:", StringComparison.OrdinalIgnoreCase))
+                    {
+                        audioFile = line;
+                    }
+                }
+
                 if (string.IsNullOrWhiteSpace(line) || trimmedLine.StartsWith(';'))
                 {
                     continue;
@@ -1860,6 +1881,23 @@
             }
             if (header.Length > 0)
             {
+                string headerStr = header.ToString();
+                if (playResX != null && GetTagValueFromHeader("PlayResX", "[Script Info]", headerStr) == null)
+                {
+                    headerStr = AddTagToHeader("PlayResX", playResX, "[Script Info]", headerStr);
+                }
+                if (playResY != null && GetTagValueFromHeader("PlayResY", "[Script Info]", headerStr) == null)
+                {
+                    headerStr = AddTagToHeader("PlayResY", playResY, "[Script Info]", headerStr);
+                }
+                if (videoFile != null && GetTagValueFromHeader("Video File", "[Script Info]", headerStr) == null)
+                {
+                    headerStr = AddTagToHeader("Video File", videoFile, "[Script Info]", headerStr);
+                }
+                if (audioFile != null && GetTagValueFromHeader("Audio File", "[Script Info]", headerStr) == null)
+                {
+                    headerStr = AddTagToHeader("Audio File", audioFile, "[Script Info]", headerStr);
+                }
+                header.Clear().Append(headerStr);
                 subtitle.Header = header.ToString();
             }

PATCH_EOF
patch src/libse/SubtitleFormats/AdvancedSubStationAlpha.cs patch_ass.diff
