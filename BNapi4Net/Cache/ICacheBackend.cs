using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNapi4Net.Cache
{
    /// <summary>
    /// Provides the backend storage for the cache
    /// </summary>
    public interface ICacheBackend
    {
        /// <summary>
        /// Cache reached its limit?  Need to evict an entry from the cache?
        /// </summary>
        bool Full { get; }

        /// <summary>
        /// Update usage statistics of the given item
        /// </summary>
        /// <param name="item"></param>
        void Touch(CacheItem item);

        /// <summary>
        /// Gets an object from the cache
        /// </summary>
        /// <param name="key">key to item in cache</param>
        /// <returns>null if key was not found</returns>
        CacheItem Get(string key);

        /// <summary>
        /// Places an item into the cache
        /// </summary>
        /// <param name="item">Item to place</param>
        /// <returns>true if key didnt already exists</returns>
        bool Set(CacheItem item);

        /// <summary>
        /// Remove item with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        CacheItem Remove(string key);

        /// <summary>
        /// Removes the given item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        CacheItem Remove(CacheItem item);

        int Count { get; }
    }
}
