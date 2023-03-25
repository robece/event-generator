namespace EventGenerator.Models
{
    internal class GenerateEventsRequest
    {
        public string sourceName { get; set; } = string.Empty;
        public string versionType { get; set; } = string.Empty;
        public string version { get; set; } = string.Empty;
        public string eventType { get; set; } = string.Empty;
        public string eventSchema { get; set; } = string.Empty;
        public int numberOfEvents { get; set; } = 0;
    }
}