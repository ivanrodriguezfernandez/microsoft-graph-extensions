using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Client;
using System.Text;

namespace microsoft_graph_extensions
{
    public class GraphTokenCacheMemory
    {
        private static readonly object FileLock = new object();
        private readonly string _cacheId;
        private readonly IDistributedCache _distCache;
        private TokenCache _cache = new TokenCache();

        private const string DistributedCacheId = "IS_";

        public GraphTokenCacheMemory(string tenantId, IDistributedCache distCache)
        {
            // not object, we want the SUB
            _cacheId = DistributedCacheId + tenantId + "_TokenCache";
            _distCache = distCache;

            Load();
        }

        public TokenCache GetCacheInstance()
        {
            _cache.SetBeforeAccess(BeforeAccessNotification);
            _cache.SetAfterAccess(AfterAccessNotification);
            Load();

            return _cache;
        }

        public void SaveUserStateValue(string state)
        {
            lock (FileLock)
            {
                _distCache.Set(_cacheId + "_state", Encoding.ASCII.GetBytes(state));
            }
        }

        public string ReadUserStateValue()
        {
            string state;
            lock (FileLock)
            {
                state = Encoding.ASCII.GetString(_distCache.Get(_cacheId + "_state") as byte[]);
            }

            return state;
        }

        public void Load()
        {
            lock (FileLock)
            {
                _cache.Deserialize(_distCache.Get(_cacheId) as byte[]);
            }
        }

        public void Persist()
        {
            lock (FileLock)
            {
                // reflect changes in the persistent store
                _distCache.Set(_cacheId, _cache.Serialize());
                // once the write operation took place, restore the HasStateChanged bit to false
#pragma warning disable CS0618 // Type or member is obsolete
                _cache.HasStateChanged = false;
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        // Empties the persistent store.
        public void Clear()
        {
            _cache = null;
            lock (FileLock)
            {
                _distCache.Remove(_cacheId);
            }
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after MSAL accessed the cache.
        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
#pragma warning disable CS0618 // Type or member is obsolete
            if (_cache.HasStateChanged)
#pragma warning disable CS0618 // Type or member is obsolete
            {
                Persist();
            }
        }
    }
}