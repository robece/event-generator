﻿using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace EventGenerator.Handlers
{
    public class GitHubTreeHandler
    {
        public GitHubTreeHandler()
        {
        }

        public static async Task<Dictionary<string, string>> GetRepositoryTreeAsync(bool recursive = true)
        {
            var result = new Dictionary<string, string>();
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EventGenerator", "1"));
            var sha = await GetMainRepositoryTreeShaAsync();
            var contentsUrl = $"https://api.github.com/repos/robece/event-generator-specs/git/trees/{sha}?recursive={recursive}";

            try
            {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        private static async Task<string> GetMainRepositoryTreeShaAsync()
        {
            string result = string.Empty;
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EventGenerator", "1"));
            var contentsUrl = $"https://api.github.com/repos/robece/event-generator-specs/contents?ref=main";

            try
            {
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
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }
    }
}