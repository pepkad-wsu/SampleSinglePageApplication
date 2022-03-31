using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace SampleSinglePageApplication;
/// <summary>
/// extends the CacheStore to create a Clear method to remove all cached items
/// </summary>
public static class CacheExtensions
{
    /// <summary>
    /// removes all cached items from the cache
    /// </summary>
    /// <param name="cache"></param>
    public static void Clear(this ObjectCache cache) => cache.Select(kvp => kvp.Key).ToList().ForEach(k => cache.Remove(k));
}

public static class CacheStore
{
    /// <summary>
    /// Retrieve an item from the cache, loading if necessary
    /// </summary>
    /// <param name="TenantId">The Unique TenantId</param>
    /// <param name="cacheKey">Name/Key for the cache</param>
    /// <param name="cacheLoadFunc">Function returning object to use if cache is empty</param>
    /// <param name="absoluteExpiration">The absolute expiration of the cache item. Default is beginning of next day.</param>
    /// <returns>Stored object from cache</returns>
    public static T GetCachedItem<T>(Guid TenantId, string cacheKey, Func<T> cacheLoadFunc, DateTimeOffset? absoluteExpiration = null)
    {
        var memCache = MemoryCache.Default;

        string key = cacheKey + "_" + TenantId.ToString();

        if (!memCache.Contains(key)) {
            var policy = new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration ?? DateTimeOffset.Now.Date.AddDays(1.0) };

            var cItem = new CacheItem(key, cacheLoadFunc());

            memCache.Set(cItem, policy);
        }

        return (T)memCache.GetCacheItem(key).Value;
    }

    /// <summary>
    /// Retrieve an item from the cache
    /// </summary>
    /// <param name="TenantId">The Unique TenantId</param>
    /// <param name="cacheKey">Name/Key for the cache</param>
    /// <returns>Stored object from cache</returns>
    public static T? GetCachedItem<T>(Guid TenantId, string cacheKey)
    {
        dynamic? output = null;
        var memCache = MemoryCache.Default;

        string key = cacheKey + "_" + TenantId.ToString();

        if (memCache.Contains(key)) {
            output = (T)memCache.GetCacheItem(key).Value;
        }

        return output;
    }

    /// <summary>
    /// Store an item in the cache
    /// </summary>
    /// <param name="TenantId">The Unique TenantId</param>
    /// <param name="cacheKey">Name/Key for the cache</param>
    /// <param name="item">Object to store in the cache. If null, then the item is removed from the cache.</param>
    /// <param name="absoluteExpiration">The absolute expiration of the cache item. Default is beginning of next day.</param>
    public static void SetCacheItem(Guid TenantId, string cacheKey, object? item, DateTimeOffset? absoluteExpiration = null)
    {
        var memCache = MemoryCache.Default;
        // If the item is null, then clear this item
        string key = cacheKey + "_" + TenantId.ToString();


        if (item == null) {
            memCache.Remove(key);
        } else {
            var policy = new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration ?? DateTimeOffset.Now.AddHours(1.0) };
            var cItem = new CacheItem(key, item);
            memCache.Set(cItem, policy);
        }
    }
}
