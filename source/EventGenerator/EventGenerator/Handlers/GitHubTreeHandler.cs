using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Xml.Linq;

namespace EventGenerator.Handlers
{
	public class GitHubTreeHandler
	{
		public GitHubTreeHandler()
		{
		}

        public static async Task<Dictionary<string, string>> GetRepositoryTree(bool recursive = true)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            var httpClient = new HttpClient();
            try
            {
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EventGenerator", "1"));
                var contentsUrl = $"https://api.github.com/repos/robece/event-generator/git/trees/3ae2609ae4aede6abb5def6d09813f683ea58d78?recursive=true";
                var contentsJson = await httpClient.GetStringAsync(contentsUrl);

                var contents = JsonConvert.DeserializeObject(contentsJson) as JObject;
                if (contents == null)
                    return result;

                var tree = contents.GetValue("tree");
                if (tree == null)
                    return result;

                var aTree = tree.Value<JArray>();
                if (aTree == null)
                    return result;

                foreach (var record in aTree)
                {
                    var o = (JObject)record;

                    var path = o.GetValue("path");
                    if (path == null) { continue; }
                    var sPath = path.Value<string>();

                    var type = o.GetValue("type");
                    if (type == null) { continue; }
                    var sType = type.Value<string>();

                    if (sPath == null) { continue; }
                    if (sType == null) { continue; }

                    result.Add(sPath, sType);
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
