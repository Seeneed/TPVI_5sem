using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ResultsCollection
{
    public class ResultsService : IResultsService
    {
        private const string FilePath = "results.json";
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private ConcurrentDictionary<int, string> _results;
        private int _nextId;

        public ResultsService()
        {
            LoadResultsFromFile().GetAwaiter().GetResult();
            _nextId = _results.Keys.Any() ? _results.Keys.Max() + 1 : 1;
        }

        private async Task LoadResultsFromFile()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(FilePath))
                {
                    _results = new ConcurrentDictionary<int, string>();
                    return;
                }
                var json = await File.ReadAllTextAsync(FilePath);
                var dictionary = JsonSerializer.Deserialize<Dictionary<int, string>>(json);
                _results = new ConcurrentDictionary<int, string>(dictionary ?? new Dictionary<int, string>());
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task SaveResultsToFile()
        {
            await _semaphore.WaitAsync();
            try
            {
                var dictionary = _results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                var json = JsonSerializer.Serialize(dictionary, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(FilePath, json);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<KeyValuePair<int, string>> AddResultAsync(string value)
        {
            var newId = Interlocked.Increment(ref _nextId);
            _results.TryAdd(newId, value);
            await SaveResultsToFile();
            return new KeyValuePair<int, string>(newId, value);
        }

        public async Task<KeyValuePair<int, string>> DeleteResultAsync(int key)
        {
            if (_results.TryRemove(key, out var value))
            {
                await SaveResultsToFile();
                return new KeyValuePair<int, string>(key, value);
            }
            return default;
        }

        public Task<KeyValuePair<int, string>> GetResultAsync(int key)
        {
            if (_results.TryGetValue(key, out var value))
            {
                return Task.FromResult(new KeyValuePair<int, string>(key, value));
            }
            return Task.FromResult(default(KeyValuePair<int, string>));
        }

        public Task<Dictionary<int, string>> GetResultsAsync()
        {
            return Task.FromResult(_results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }

        public async Task<KeyValuePair<int, string>> UpdateResultAsync(int key, string value)
        {
            if (_results.ContainsKey(key))
            {
                _results[key] = value;
                await SaveResultsToFile();
                return new KeyValuePair<int, string>(key, value);
            }
            return default;
        }
    }
}