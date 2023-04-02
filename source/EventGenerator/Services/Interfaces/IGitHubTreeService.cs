namespace EventGenerator.Services.Interfaces
{
    internal interface IGitHubTreeService
    {
        Task<Dictionary<string, string>> GetRepositoryTreeAsync(bool recursive = true);
    }
}
