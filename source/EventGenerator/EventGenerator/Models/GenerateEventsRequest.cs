namespace EventGenerator.Models
{
    public class GenerateEventsRequest
    {
        public string sourceName { get; set; }
        public string versionType { get; set; }
        public string version { get; set; }
        public string eventType { get; set; }
        public string eventSchema { get; set; }
        public int numberOfEvents { get; set; }
    }
}