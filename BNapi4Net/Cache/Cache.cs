using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNapi4Net.Cache
{
    public class Cache : BNapi4Net.ICache, IDisposable
    {
        ICacheBackend backend;

        public Cache(ICacheBackend backend)
        {
            this.backend = backend;
        }

        public int Count
        {
            get
            {
                return backend.Count;
            }
        }

        public bool TryGetValue(string key, out object val)
        {
            CacheItem item = backend.Get(key);
            if (item == null)
            {
                val = null;
                return false;
            }
            else
            {
                val = item.Value;
                return true;
            }
        }
        /// <summary>
        /// Gets an object from the cache or creates it if it doesnt exist
        /// </summary>
        /// <param name="key">key to item</param>
        /// <param name="f">Factory method if key doesnt exist</param>
        /// <returns></returns>
        public object Get(string key, Func<object> f)
        {
            // dont ever expire this key
            return Get(key, TimeSpan.FromTicks(-1), f);
        }

        public CacheItem Get(string key)
        {
            CacheItem item = backend.Get(key);
            if (item != null)
            {
                // expired?
                if (item.Stats.Expire <= DateTime.Now)
                {
                    backend.Remove(item);
                    item = null;
                }
            }

            return item;
        }


        /// <summary>
        /// Gets an object from the cache or creates it if it doesnt exist
        /// </summary>
        /// <param name="key">key to item</param>
        /// <param name="expire">Expire this item after this much time.  -1 for NEVER</param>
        /// <param name="f">Factory method if key doesnt exist</param>
        /// <returns></returns>
        public object Get(string key, TimeSpan expire, Func<object> f)
        {
            CacheItem item = backend.Get(key);
            if (item == null)
            {
                if (f != null)
                {
                    object val = f();
                    item = new CacheItem(key, val);
                    
                    // anything less than 0 is NEVER
                    if (expire.Ticks < 0)
                    {
                        item.Stats.Expire = DateTime.MaxValue;
                    }
                    else
                    {
                        item.Stats.Expire = item.Stats.Created + expire;
                    }
                    backend.Set(item);
                }
            }
            if (item == null) return null;

            // item is expired..
            if (item.Stats.Expire <= DateTime.Now)
            {
                // recreate it..
                if (f != null)
                {
                    object val = f();
                    item = new CacheItem(key, val);                    
                    backend.Set(item);
                }
            }

            // anything less than 0 is NEVER
            if (expire.Ticks < 0)
            {
                item.Stats.Expire = DateTime.MaxValue;
            }
            else
            {
                item.Stats.Expire = item.Stats.Created + expire;
            }
            
            // update access statistics for the item
            backend.Touch(item);
            return item.Value;
        }

        public bool Set(CacheItem item)
        {            
            return backend.Set(item);
        }

        public bool Set(string key, object obj)
        {
            CacheItem item = new CacheItem(key, obj);
            return Set(item);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (backend is IDisposable)
            {
                ((IDisposable)backend).Dispose();
            }
            backend = null;
            
        }

        #endregion
    }
}
