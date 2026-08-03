using System;
using System.IO;
using Xunit;
using Nikse.SubtitleEdit.Logic.Automation;

namespace SubtitleEdit.Tests
{

    public class LuaAutomationTest
    {
        private string _autoLoadDir;


        public LuaAutomationTest()
        {
            _autoLoadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "automation", "autoload");
            if (!Directory.Exists(_autoLoadDir))
            {
                Directory.CreateDirectory(_autoLoadDir);
            }
        }

        [Fact]
        public void Test_LuaAutomationService_Initialization_And_Scan()
        {
            var testFilePath = Path.Combine(_autoLoadDir, "test_macro.lua");
            File.WriteAllText(testFilePath, "aegisub.register_macro(\"Test Macro\", \"Desc\", function() end)");

            try
            {
                var service = new LuaAutomationService();
                var macros = service.ScanForMacros();

                Assert.True(macros.Count > 0);
                Assert.Equal("Test Macro", macros[0].Name);

                // Try to execute
                var ex = Record.Exception(() => service.ExecuteMacro(testFilePath, null));
                Assert.Null(ex);
            }
            finally
            {
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }
    }
}
