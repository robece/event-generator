namespace EventGenerator.Handlers
{
    public class EventSourceHandler
    {
        public EventSourceHandler()
        {
        }

        public static List<string> GetSources(List<KeyValuePair<string, string>> repositoryTree)
        {
            List<string> result = new List<string>();

            try
            {
                foreach (KeyValuePair<string, string> kv in repositoryTree)
                {
                    string[] words = kv.Key.Split('/');

                    if (words.Length > 0)
                    {
                        // source
                        if (!string.IsNullOrEmpty(words[0]))
                        {
                            if (kv.Value == "tree")
                                if (!result.Contains(words[0]))
                                    result.Add(words[0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }

        public static List<string> GetVersionTypes(List<KeyValuePair<string, string>> repositoryTree, string sourceName)
        {
            List<string> result = new List<string>();

            try
            {
                string path = $"{sourceName}/";
                foreach (KeyValuePair<string, string> kv in repositoryTree)
                {
                    kv.Key.Replace(path, "");
                }

                int level = 2;
                foreach (KeyValuePair<string, string> kv in repositoryTree)
                {
                    string[] words = kv.Key.Split('/');
                    if (words.Length == level)
                    {
                        if (!string.IsNullOrEmpty(words[0]))
                        {
                            if (kv.Value == "tree")
                                if (!result.Contains(words[level-1]))
                                    result.Add(words[level-1]);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        public static List<string> GetVersions(List<KeyValuePair<string, string>> repositoryTree, string sourceName, string versionType)
        {
            List<string> result = new List<string>();

            try
            {
                string path = $"{sourceName}/{versionType}/";
                foreach (KeyValuePair<string, string> kv in repositoryTree)
                {
                    kv.Key.Replace(path, "");
                }

                int level = 3;
                foreach (KeyValuePair<string, string> kv in repositoryTree)
                {
                    string[] words = kv.Key.Split('/');
                    if (words.Length == level)
                    {
                        if (!string.IsNullOrEmpty(words[0]))
                        {
                            if (kv.Value == "tree")
                                if (!result.Contains(words[level-1]))
                                    result.Add(words[level-1]);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        public static List<string> GetEventSchemas(List<KeyValuePair<string, string>> repositoryTree, string sourceName, string versionType, string version)
        {
            List<string> result = new List<string>();

            try
            {
                string path = $"{sourceName}/{versionType}/{version}/examples/";
                foreach (KeyValuePair<string, string> kv in repositoryTree)
                {
                    kv.Key.Replace(path, "");
                }

                int level = 5;
                foreach (KeyValuePair<string, string> kv in repositoryTree)
                {
                    string[] words = kv.Key.Split('/');
                    if (words.Length == level)
                    {
                        if (!string.IsNullOrEmpty(words[0]))
                        {
                            if (kv.Value == "tree")
                                if (!result.Contains(words[level-1]))
                                    result.Add(words[level-1]);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        public static List<string> GetEventTypes(List<KeyValuePair<string, string>> repositoryTree, string sourceName, string versionType, string version, string eventSchema)
        {
            List<string> result = new List<string>();

            try
            {
                string path = $"{sourceName}/{versionType}/{version}/examples/{eventSchema}/ai/";
                foreach (KeyValuePair<string, string> kv in repositoryTree)
                {
                    kv.Key.Replace(path, "");
                }

                int level = 7;
                foreach (KeyValuePair<string, string> kv in repositoryTree)
                {
                    string[] words = kv.Key.Split('/');
                    if (words.Length == level)
                    {
                        if (!string.IsNullOrEmpty(words[0]))
                        {
                            if (kv.Value == "blob")
                                if (!result.Contains(words[level-1]))
                                    result.Add(words[level-1]);
                        }
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

