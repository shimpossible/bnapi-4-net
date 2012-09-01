using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNapi4Net
{
    public class CacheStatistics
    {
        public DateTime Created;

        /// <summary>
        /// When to expire this item
        /// </summary>
        public DateTime Expire; 
        public DateTime LastAccessTime;

    }
    public class CacheItem
    {
        public string Key;          // cached key
        public object Value;        // cached object
        public CacheStatistics Stats = new CacheStatistics();

        public CacheItem(string key, object val, DateTime lat, DateTime cr)
        {
            this.Key = key;
            this.Value = val;
            Stats.LastAccessTime = lat;
            Stats.Created = cr;
        }
        public CacheItem(string key, object val)
            :this(key,val, DateTime.Now, DateTime.Now)
        {
        }

        public override bool Equals(object obj)
        {
            CacheItem other = obj as CacheItem;

            return other != null && other.Key == this.Key;
        }
    }

    /// <summary>
    /// Cache is BASICALY a dictionary
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Gets an object from the cache, or creates it if its not there
        /// </summary>
        /// <param name="key">key to cached item</param>
        /// <param name="f">Function to call if item not already cached</param>
        /// <returns></returns>
        object Get(string key, Func<object> f);

        //object this[string key] { get; set; }

        bool TryGetValue(string key, out object val);
    }
}
