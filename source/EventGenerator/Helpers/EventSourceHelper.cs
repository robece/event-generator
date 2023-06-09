﻿namespace EventGenerator.Helpers
{
    internal class EventSourceHelper
    {
        public EventSourceHelper()
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

            string path = $"{sourceName}/";
            int level = 2;
            result = GetList(repositoryTree, path, level, "tree");

            return result;
        }

        public static List<string> GetVersions(List<KeyValuePair<string, string>> repositoryTree, string sourceName, string versionType)
        {
            List<string> result = new List<string>();

            string path = $"{sourceName}/{versionType}/";
            int level = 3;
            result = GetList(repositoryTree, path, level, "tree");

            return result;
        }

        public static List<string> GetEventSchemas(List<KeyValuePair<string, string>> repositoryTree, string sourceName, string versionType, string version)
        {
            List<string> result = new List<string>();

            string path = $"{sourceName}/{versionType}/{version}/gpt-prompts/";
            int level = 5;
            result = GetList(repositoryTree, path, level, "tree");
            result = result.FindAll(x => x.Contains("schema"));
            return result;
        }

        public static List<string> GetEventTypes(List<KeyValuePair<string, string>> repositoryTree, string sourceName, string versionType, string version, string eventSchema)
        {
            List<string> result = new List<string>();

            string path = $"{sourceName}/{versionType}/{version}/gpt-prompts/{eventSchema}/";
            int level = 6;
            result = GetList(repositoryTree, path, level, "blob");
            result = result.Select(s => s.Replace(".prompt", "")).ToList();
            return result;
        }

        private static List<string> GetList(List<KeyValuePair<string, string>> repositoryTree, string path, int level, string type)
        {
            List<string> result = new List<string>();

            try
            {
                var filteredTree = repositoryTree.FindAll(x => x.Key.StartsWith(path));

                foreach (KeyValuePair<string, string> kv in filteredTree)
                {
                    kv.Key.Replace(path, "");
                }

                foreach (KeyValuePair<string, string> kv in filteredTree)
                {
                    string[] words = kv.Key.Split('/');
                    if (words.Length == level)
                    {
                        if (!string.IsNullOrEmpty(words[0]))
                        {
                            if (kv.Value == type)
                                if (!result.Contains(words[level - 1]))
                                    result.Add(words[level - 1]);
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
