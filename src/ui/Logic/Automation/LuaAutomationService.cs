using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;

namespace Nikse.SubtitleEdit.Logic.Automation;

public class LuaMacro
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}

public class LuaAutomationService
{
    private readonly string _automationDirectory;

    public LuaAutomationService()
    {
        _automationDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "automation", "autoload");
        if (!Directory.Exists(_automationDirectory))
        {
            Directory.CreateDirectory(_automationDirectory);
        }
    }

    public List<LuaMacro> ScanForMacros()
    {
        var macros = new List<LuaMacro>();
        if (!Directory.Exists(_automationDirectory)) return macros;

        var luaFiles = Directory.GetFiles(_automationDirectory, "*.lua");
        var regex = new Regex(@"aegisub\.register_macro\s*\(\s*(['""])(.*?)\1", RegexOptions.Compiled);

        foreach (var file in luaFiles)
        {
            try
            {
                var content = File.ReadAllText(file);
                var match = regex.Match(content);
                if (match.Success)
                {
                    macros.Add(new LuaMacro
                    {
                        Name = match.Groups[2].Value,
                        FilePath = file
                    });
                }
            }
            catch (Exception)
            {
                // Ignore unreadable files
            }
        }
        return macros;
    }

    public void ExecuteMacro(string filePath, object subtitles)
    {
        var script = new Script();
        // Emulate the aegisub.register_macro structure just to prevent errors
        script.DoString(@"
            aegisub = {}
            function aegisub.register_macro(name, description, processing_function, validation_function)
                -- infrastructure stub
            end
        ");

        script.DoFile(filePath);
        // Note: the actual execution bridging would go here
    }
}
