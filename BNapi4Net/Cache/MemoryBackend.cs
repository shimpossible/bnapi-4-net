using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNapi4Net.Cache
{
    public class MemoryBackend : ICacheBackend
    {
        ICacheBackend fallback;

        Dictionary<string, CacheItem> data = new Dictionary<string, CacheItem>();
        int max;
        public MemoryBackend(int max, ICacheBackend fb)
        {
            this.max = max;
            fallback = fb;
        }

        #region ICacheBackend Members

        public int Count
        {
            get { return data.Count;  }
        }
        public bool Full
        {
            get { return data.Count >= max; }
        }

        public void Touch(CacheItem item)
        {
            item.Stats.LastAccessTime = DateTime.Now;
        }

        public CacheItem Get(string key)
        {
            CacheItem ci = null;
            if (data.TryGetValue(key, out ci))
            {
                Touch(ci);
                return ci;
            }

            if (fallback!=null)
            {
                // if the fallback contains the data,
                // bring it into our cache
                ci = fallback.Get(key);
                if (ci != null)
                {
                    this.Set(ci);
                }
            }
            return null;
        }

        /// <summary>
        /// Places an item into the cache
        /// </summary>
        /// <param name="item">Item to place</param>
        /// <returns>true if key didnt already exists</returns>
        public bool Set(CacheItem item)
        {
            bool isNew = data.ContainsKey(item.Key)==false;

            // prune as needed
            Prune();

            data[item.Key] = item;

            return isNew;
        }

        public CacheItem Remove(string key)
        {
            CacheItem ci;
            if (data.TryGetValue(key, out ci))
            {
                data.Remove(key);
            }
            
            return ci;
        }

        public CacheItem Remove(CacheItem item)
        {
            return Remove(item.Key);
        }

        #endregion

        /// <summary>
        /// Prune old entries from the cache
        /// </summary>
        protected void Prune()
        {
            if (Full == false) return;

            // ascending order (ie, oldest first)
            var tmp = data.OrderBy(x => x.Value.Stats.LastAccessTime);

            foreach (KeyValuePair<string, CacheItem> c in tmp)
            {
                CacheItem ci = Remove(c.Key);
                
                if (fallback != null)
                {
                    fallback.Set(ci);
                }

                // dont need to remove any more
                if (Full == false) break;
            }
        }

    }
}
