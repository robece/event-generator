using EventGenerator.Models;
using System.Net.Http.Json;

namespace EventGenerator.Handlers
{
    internal class EventGeneratorApiHandler
    {
        public EventGeneratorApiHandler()
        {
        }

        public static async Task<string> PostAsync(string sourceName, string versionType, string version, string eventSchema, string eventType, int numberOfEvents)
        {
            string result = string.Empty;
            var httpClient = new HttpClient();
            var contentsUrl = $"http://localhost:7190/api/generatecodeforevents";

            try
            {
                var request = new GenerateEventsRequest()
                {
                    sourceName = sourceName,
                    versionType = versionType,
                    version = version,
                    eventSchema = eventSchema,
                    eventType = eventType,
                    numberOfEvents = numberOfEvents
                };
                var httpResponseMessage = await httpClient.PostAsJsonAsync(contentsUrl, request);
                httpResponseMessage.EnsureSuccessStatusCode();

                result = await httpResponseMessage.Content.ReadAsStringAsync();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }
    }
}