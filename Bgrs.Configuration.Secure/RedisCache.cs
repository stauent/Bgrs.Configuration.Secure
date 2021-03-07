using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Bgrs.Configuration.Secure
{
    public interface IRedisCache
    {
        bool KeyExists(string Key);
        void KeyDelete(string key);
        void StringSet(string Value, string Key);
        string StringGet(string Key);
        void CacheObject<T>(T cachedObject, string Key) where T : class;
        T GetCachedObject<T>(string Key) where T : class;
    }

    public class RedisCache : IRedisCache
    {
        public static string CacheConnectionString { get; private set; }
        public RedisCache(string cacheConnectionString)
        {
            CacheConnectionString = cacheConnectionString;
        }

        #region private methods
        private static IDatabase Cache
        {
            get
            {
                return Connection.GetDatabase();
            }
        }

        private static ConnectionMultiplexer Connection
        {
            get
            {
                return LazyConnection.Value;
            }
        }

        private static readonly Lazy<ConnectionMultiplexer> LazyConnection
            = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(CacheConnectionString));
        #endregion

        #region public methods

        public bool KeyExists(string Key)
        {
            return Cache.KeyExists(Key);
        }

        public void KeyDelete(string key)
        {
            if (key == null) throw new ArgumentNullException("key");
            Cache.KeyDelete(key);
        }

        public void StringSet(string Value, string Key)
        {
            Cache.StringSet(Key, Value);
        }

        public string StringGet(string Key)
        {
            return Cache.StringGet(Key);
        }

        public void CacheObject<T>(T cachedObject, string Key) where T : class
        {
            StringSet(JsonConvert.SerializeObject(cachedObject), Key);
        }
        public T GetCachedObject<T>(string Key) where T : class
        {
            string value = StringGet(Key);
            if (!string.IsNullOrEmpty(value))
                return JsonConvert.DeserializeObject<T>(value);
            return null;
        }
        #endregion
    }


    public static class RedisCacheHelper
    {
        public static IRedisCache InitializeRedisCache(this IConfigurationRoot configuration, IApplicationSecrets secrets)
        {
            IRedisCache cache = null;

            // Connect to the redis cache using the secret "ApplicationCache"
            string RedisConnectionString = secrets.ConnectionString("ApplicationCache");
            if (!string.IsNullOrEmpty(RedisConnectionString))
            {
                cache = new RedisCache(RedisConnectionString);
            }

            return (cache);
        }
    }


    public static class ScheduledRefreshFromCache
    {
        public static bool Initialized { get; set; } = false;
        private static AutoResetEvent waitForCompletion = new AutoResetEvent(false);

        public static bool RefreshConfigurationFromCache(this IRedisCache cache, IApplicationSecrets secrets, IConfigurationRoot configuration)
        {
            if (!Initialized)
            {
                // Get the information about what cache values we need refreshed into configuration
                IApplicationSecretsConnectionStrings TimedCacheRefresh = secrets.Secret("TimedCacheRefresh");
                if (TimedCacheRefresh != null)
                {
                    // Use the "TimedCacheRefresh" secret to get the list of cache keys that need to be auto-refreshed
                    // and placed into the contiguration. This allows the application to read cache values just like
                    // regular configuration settings in "appsettings.json". The "Value" for this secret contains
                    // an array of cache keys that must be refreshed periodically.
                    string[] keys = TimedCacheRefresh.Value.Split(',');

                    // The MetadataProperties "RefreshPeriodMinutes" contains the refresh period for the cache keys
                    string RefreshPeriodMinutes = TimedCacheRefresh["RefreshPeriodMinutes"];
                    if (!string.IsNullOrWhiteSpace(RefreshPeriodMinutes))
                    {
                        int minutes = int.Parse(RefreshPeriodMinutes);

                        // Start the thread that will read the cache every N minutes
                        Task task = new Task(() => LaunchTimedRefresh(cache, keys, minutes, configuration));
                        task.Start();

                        // Wait for the thread to read all cache keys for the first time before continuing
                        waitForCompletion.WaitOne();
                        Initialized = true;
                    }
                }
            }

            return (Initialized);
        }

        public static void LaunchTimedRefresh(IRedisCache cache, string[] keys, int minutes, IConfigurationRoot configuration)
        {
            while (true)
            {
                // Go through each key and pull it from the cache. Then
                // update the local configuration with the cache value.
                foreach (string key in keys)
                {
                    string value = cache.StringGet(key);
                    configuration[key] = value ?? "";
                }

                if (!Initialized)
                {
                    // Tell the client that the initial set of cache values have been read
                    waitForCompletion.Set();
                }

                // Wait N minutes till you read the cache again
                Thread.Sleep(TimeSpan.FromMinutes(minutes));
            }
        }
    }
}
