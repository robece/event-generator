using EventGenerator.Modules;
using System.Globalization;
using Terminal.Gui;

namespace EventGenerator
{
    internal partial class Program
    {
        public static string Version = "1.0-preview";

        static void Main(string[] args)
        {
            if (OperatingSystem.IsWindows())
            {
                Console.SetWindowSize(200, 50);
            }

            Application.Init();

            var editor = new Editor();
            editor.DisplayEditorWindow();

            Application.Top.Closed += (_) => Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            Application.Run();
        }
    }
}
