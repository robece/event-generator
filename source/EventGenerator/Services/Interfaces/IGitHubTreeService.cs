using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventGenerator.Services.Interfaces
{
    internal interface IGitHubTreeService
    {
        Task<Dictionary<string, string>> GetRepositoryTreeAsync(bool recursive = true);
    }
}
