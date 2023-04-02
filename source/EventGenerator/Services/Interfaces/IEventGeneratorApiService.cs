namespace EventGenerator.Services.Interfaces
{
    internal interface IEventGeneratorApiService
    {
        Task<string> PostAsync(string sourceName, string versionType, string version, string eventSchema, string eventType, int numberOfEvents);
    }
}
