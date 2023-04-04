using EventGenerator.Common;
using EventGenerator.Services;
using EventGenerator.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using Terminal.Gui;

namespace EventGenerator
{
    internal partial class Program
    {
        public static string Version = "1.0-preview";

        static void Main(string[] args)
        {
            var host = CreateHost();

            Application.Init();

            var editorService = host.Services.GetRequiredService<IEditorService>();
            editorService.DisplayEditorWindow();

            var settings = Utils.GetSettings();
            if (settings == null)
                Utils.InitSettings();

            Application.Top.Closed += (_) => Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            Application.Run();
        }

        static IHost CreateHost()
        {
            var builder = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                // register the IHttpClientFactory
                services.AddHttpClient();

                // register the dependency injection
                services.AddTransient<IGitHubTreeService, GitHubTreeService>();

                services.AddTransient<IEditorService, EditorService>();
                services.AddTransient<IGeneratorService, GeneratorService>();
                services.AddTransient<IPublisherService, PublisherService>();

            }).UseConsoleLifetime();

            return builder.Build();
        }
    }
}
