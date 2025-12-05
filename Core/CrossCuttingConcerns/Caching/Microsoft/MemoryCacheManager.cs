using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace Core.CrossCuttingConcerns.Caching.Microsoft;
public class MemoryCacheManager : ICacheManager
{
    IMemoryCache _memoryCache;
    private readonly HashSet<string> _cacheKeys;

    public MemoryCacheManager(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _cacheKeys = new HashSet<string>(); // Anahtarları tutan liste
    }

    public void Add(string key, object data, int duration)
    {
        _memoryCache.Set(key, data, TimeSpan.FromMinutes(duration));
        _cacheKeys.Add(key);
    }

    public T Get<T>(string key)
    {
        return _memoryCache.TryGetValue(key, out T value) ? value : default;

    }

    public object Get(string key)
    {
        return _memoryCache.Get(key);
    }

    public bool IsAdd(string key)
    {
        return _memoryCache.TryGetValue(key, out _);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
        _cacheKeys.Remove(key);
    }

    public void RemoveByPattern(string pattern)
    {
        var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var keysToRemove = _cacheKeys.Where(key => regex.IsMatch(key)).ToList();

        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
            _cacheKeys.Remove(key);
        }
    }
}
