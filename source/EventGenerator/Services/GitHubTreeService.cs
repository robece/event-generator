using EventGenerator.Services.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace EventGenerator.Services
{
    internal class GitHubTreeService : IGitHubTreeService
    {
        private readonly IHttpClientFactory? _httpClientFactory = null;

        public GitHubTreeService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Dictionary<string, string>> GetRepositoryTreeAsync(bool recursive = true)
        {
            var result = new Dictionary<string, string>();

            if (_httpClientFactory == null)
                throw new Exception("Http client initialization error");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EventGenerator", "1"));
            var sha = await GetMainRepositoryTreeShaAsync();
            var contentsUrl = $"https://api.github.com/repos/robece/event-generator-specs/git/trees/{sha}?recursive={recursive}";

            var httpResponseMessage = await httpClient.GetAsync(contentsUrl);
            httpResponseMessage.EnsureSuccessStatusCode();

            var contents = await httpResponseMessage.Content.ReadFromJsonAsync<JsonObject>();

            if (contents is not null && contents.TryGetPropertyValue("tree", out JsonNode? aTree) && aTree is not null)
            {
                foreach (var record in aTree.AsArray())
                {
                    var path = record?["path"]?.ToString();
                    var type = record?["type"]?.ToString();
                    if (type is not null && path is not null)
                        result.Add(path, type);
                }
            }

            return result;
        }

        private async Task<string> GetMainRepositoryTreeShaAsync()
        {
            string result = string.Empty;

            if (_httpClientFactory == null)
                throw new Exception("Http client initialization error");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EventGenerator", "1"));
            var contentsUrl = $"https://api.github.com/repos/robece/event-generator-specs/contents?ref=main";

            var httpResponseMessage = await httpClient.GetAsync(contentsUrl);
            httpResponseMessage.EnsureSuccessStatusCode();

            var contents = await httpResponseMessage.Content.ReadFromJsonAsync<JsonArray>();

            if (contents is not null)
            {
                foreach (var record in contents.AsArray())
                {
                    var name = record?["name"]?.ToString();

                    if (name is not null)
                        if (name == "data-plane")
                        {
                            var sha = record?["sha"]?.ToString();
                            if (sha is not null)
                            {
                                result = sha;
                                break;
                            }
                        }
                }
            }

            return result;
        }
    }
}
