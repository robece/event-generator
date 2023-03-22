﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Xml.Linq;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace EventGenerator.Handlers
{
	public class GitHubTreeHandler
	{
		public GitHubTreeHandler()
		{
		}

        public static async Task<Dictionary<string, string>> GetRepositoryTree(bool recursive = true)
        {
            Dictionary<string, string> result = new();
            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EventGenerator", "1"));
            var contentsUrl = $"https://api.github.com/repos/robece/event-generator/git/trees/3ae2609ae4aede6abb5def6d09813f683ea58d78?recursive={recursive}";

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
    }
}