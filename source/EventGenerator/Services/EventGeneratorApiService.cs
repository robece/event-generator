using EventGenerator.Models;
using EventGenerator.Services.Interfaces;
using System.Net.Http.Json;

namespace EventGenerator.Services
{
    internal class EventGeneratorApiService : IEventGeneratorApiService
    {
        private readonly IHttpClientFactory? _httpClientFactory = null;

        public EventGeneratorApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> PostAsync(string sourceName, string versionType, string version, string eventSchema, string eventType, int numberOfEvents)
        {
            var request = new GenerateCodeForEventsRequest()
            {
                sourceName = sourceName,
                versionType = versionType,
                version = version,
                eventSchema = eventSchema,
                eventType = eventType,
                numberOfEvents = numberOfEvents
            };

            if (_httpClientFactory == null)
                throw new Exception("Http client initialization error");

            var httpClient = _httpClientFactory.CreateClient();
            var contentsUrl = $"http://localhost:7190/api/generatecodeforevents";

            var httpResponseMessage = await httpClient.PostAsJsonAsync(contentsUrl, request);
            httpResponseMessage.EnsureSuccessStatusCode();

            return await httpResponseMessage.Content.ReadAsStringAsync();
        }
    }
}