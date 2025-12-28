using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResultsCollection
{
    public interface IResultsService
    {
        Task<Dictionary<int, string>> GetResultsAsync();
        Task<KeyValuePair<int, string>> GetResultAsync(int key);
        Task<KeyValuePair<int, string>> AddResultAsync(string value);
        Task<KeyValuePair<int, string>> UpdateResultAsync(int key, string value);
        Task<KeyValuePair<int, string>> DeleteResultAsync(int key);
    }
}