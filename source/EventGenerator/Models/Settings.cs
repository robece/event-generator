namespace EventGenerator.Models
{
    internal class Settings
    {
        public string AzureFunctionEndpoint { get; set; } = string.Empty;

        public bool RememberAzureFunctionEndpoint { get; set; } = false;

        public string OpenAIAPIKey { get; set; } = string.Empty;

        public bool RememberOpenAIAPIKey { get; set; } = false;
    }
}
