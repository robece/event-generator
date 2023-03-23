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
                foreach (KeyValuePair<string, string> kv in repositoryTree)
                {
                    string[] words = kv.Key.Split('/');

                    if (words.Length == 2)
                    {
                        // source
                        if (!string.IsNullOrEmpty(words[0]))
                        {
                            if (words[0] == sourceName)
                            {
                                // versionType
                                if (!string.IsNullOrEmpty(words[1]))
                                {
                                    if (kv.Value == "tree")
                                        if (!result.Contains(words[1]))
                                            result.Add(words[1]);
                                }
                            }
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
                foreach (KeyValuePair<string, string> kv in repositoryTree)
                {
                    string[] words = kv.Key.Split('/');

                    if (words.Length == 3)
                    {
                        // source
                        if (!string.IsNullOrEmpty(words[0]))
                        {
                            if (words[0] == sourceName)
                            {
                                // versionType
                                if (!string.IsNullOrEmpty(words[1]))
                                {
                                    if (words[1] == versionType)
                                    {
                                        // version
                                        if (!string.IsNullOrEmpty(words[2]))
                                        {
                                            if (kv.Value == "tree")
                                                if (!result.Contains(words[2]))
                                                    result.Add(words[2]);
                                        }
                                    }
                                }
                            }
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

        public static List<string> GetEventTypes(List<KeyValuePair<string, string>> repositoryTree, string sourceName, string versionType, string version)
        {
            List<string> result = new List<string>();

            try
            {
                foreach (KeyValuePair<string, string> kv in repositoryTree)
                {
                    string[] words = kv.Key.Split('/');

                    if (words.Length == 5)
                    {
                        // source
                        if (!string.IsNullOrEmpty(words[0]))
                        {
                            if (words[0] == sourceName)
                            {
                                // versionType
                                if (!string.IsNullOrEmpty(words[1]))
                                {
                                    if (words[1] == versionType)
                                    {
                                        // version
                                        if (!string.IsNullOrEmpty(words[2]))
                                        {
                                            if (words[2] == version)
                                            {
                                                // eventType folder
                                                if (!string.IsNullOrEmpty(words[3]))
                                                {
                                                    if (words[3] == "chatgpt")
                                                    {
                                                        // eventType
                                                        if (!string.IsNullOrEmpty(words[4]))
                                                        {
                                                            if (kv.Value == "blob")
                                                                if (!result.Contains(words[4]))
                                                                    result.Add(words[4]);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
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

