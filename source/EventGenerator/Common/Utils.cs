using EventGenerator.Models;
using System.Text.Json;

namespace EventGenerator.Common
{
    internal class Utils
    {
        public static void InitSettings()
        {
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "settings");
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "settings", "settings.json");
            Directory.CreateDirectory(directoryPath);
            var settings = new Settings() { AzureFunctionEndpoint = string.Empty, RememberAzureFunctionEndpoint = false };
            var strSettings = JsonSerializer.Serialize(settings);
            File.WriteAllText(filePath, strSettings);
        }

        public static Settings? GetSettings()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "settings", "settings.json");
                string strSettings = File.ReadAllText(filePath);
                if (string.IsNullOrEmpty(strSettings))
                    return null;

                return JsonSerializer.Deserialize<Settings>(strSettings);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void UpdateSettings(Settings settings)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "settings", "settings.json");
            var strSettings = JsonSerializer.Serialize(settings);
            File.WriteAllText(filePath, strSettings);
        }

        public static void DeleteSettings()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "settings", "settings.json");
            File.Delete(filePath);
        }
    }
}
