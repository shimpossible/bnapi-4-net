using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Reflection; // for Authentication
using System.Security.Cryptography; // HMAC
namespace BNapi4Net
{
    /// <summary>
    /// Base client for all Battle.Net API clients
    /// </summary>
    public abstract class BaseClient : IDisposable
    {
        Cache.Cache cache;

        #region Properties
        public System.Globalization.CultureInfo Locale { get; set; }

        private byte[] privateKey = null;
        public string PrivateKey
        {
            protected get
            {
                return Encoding.UTF8.GetString(privateKey, 0, privateKey.Length);
            }
            set
            {
                if (value == null)
                {
                    privateKey = null;
                }
                else
                {
                    privateKey = Encoding.UTF8.GetBytes(value);

                }
            }
        }

        public string PublicKey
        {
            protected get;
            set;
        }

        protected Uri regionUri;
        protected Uri mediaUri;

        Region region;
        public Region Region
        {
            get
            {
                return region;
            }
            set
            {
                region = value;

                switch (region)
                {
                    case Region.EU:
                        regionUri = new Uri("https://eu.battle.net/api/");
                        mediaUri = new Uri("http://eu.media.blizzard.com/");
                        break;
                    case Region.KR:
                        regionUri = new Uri("https://kr.battle.net/api/");
                        mediaUri = new Uri("http://kr.media.blizzard.com/");
                        break;
                    case Region.TW:
                        regionUri = new Uri("https://tw.battle.net/api/");
                        mediaUri = new Uri("http://tw.media.blizzard.com/");
                        break;
                    case Region.CN:
                        regionUri = new Uri("https://cn.battle.net/api/");
                        mediaUri = new Uri("http://cn.media.blizzard.com/");
                        break;
                    case Region.US:
                    default:    // fallback to the US region
                        regionUri = new Uri("http://us.battle.net/api/");
                        mediaUri = new Uri("http://us.media.blizzard.com/");
                        break;
                }
            }
        }
        #endregion

        protected BaseClient(Region r = Region.US)
            : this(r, System.Globalization.CultureInfo.CurrentCulture)
        {        
        }

        /// <summary>
        /// Create a new client
        /// </summary>
        /// <param name="r">Region</param>
        /// <param name="loc">Current Culture</param>
        /// <param name="publicKey">public key for authentication</param>
        /// <param name="privateKey">private key for authentication</param>
        /// <param name="cache">Use a </param>
        public BaseClient(Region r, 
            System.Globalization.CultureInfo loc, 
            string publicKey = null, 
            string privateKey = null,
            bool cache=true)
        {
            // default to current culture
            Locale = loc;

            this.Region = r;
            // need a workaround for this in silverlight
#if !SILVERLIGHT
            //Change SSL checks so that all checks pass
            ServicePointManager.ServerCertificateValidationCallback =
                new System.Net.Security.RemoteCertificateValidationCallback(
                    delegate
                    { return true; }
                );
#endif

            try
            {
                Cache.DiskBackend backend = new Cache.DiskBackend("data.cache", 10 * 1024 * 1024, null);
                this.cache = new Cache.Cache(backend);
            }
            catch (Exception ex)
            {
                // no cache
                Console.WriteLine(ex.Message);
            }
        }

        HttpWebRequest BuildRequest(Uri url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.KeepAlive = true;
            req.MediaType = "GET";
            req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            req.AllowAutoRedirect = true;

            return req;
        }

        public Stream ReadMedia(string url)
        {
            string key = "media:" + url;

            CacheItem item = cache.Get(key);
            byte[] data;

            if (item == null)
            {
                Uri abs = new Uri(mediaUri, url);

                Console.WriteLine("getting " + abs);
                HttpWebRequest req = BuildRequest(abs);                
                HttpWebResponse res = GetResponse(req);
                Console.WriteLine("got " + abs + " (" + res.ContentLength);

                Stream st = DecodeResponse(res);
                data = ReadStream((int)res.ContentLength, st);

                // without closing these.. bad things happen
                st.Close();
                res.Close();

                item = new CacheItem(key, data);
                // never expire media.. since it shouldnt EVER change
                item.Stats.Expire = DateTime.MaxValue;
                cache.Set(item);

            }
            else
            {
                data = (byte[])item.Value;
            }

            return new MemoryStream(data);            
        }


        /// <summary>
        /// Decodes response content if it was gzip or deflated,
        /// otherwise returns the straight Response stream
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        protected Stream DecodeResponse(HttpWebResponse res)
        {
            Stream st = null;
            if (res.ContentEncoding.ToLower().Contains("gzip"))
            {
                st = new GZipStream(res.GetResponseStream(), CompressionMode.Decompress);
            }
            else if (res.ContentEncoding.ToLower().Contains("deflate"))
            {
                st = new DeflateStream(res.GetResponseStream(), CompressionMode.Decompress);
            }
            else
            {
                st = res.GetResponseStream();
            }
            return st;
        }

        /// <summary>
        /// Read the stream into a byte array
        /// </summary>
        /// <param name="len">Number of bytes in stream, or -1 if unknown</param>
        /// <param name="st">stream to read</param>
        /// <returns>byte contents of stream</returns>
        protected byte[] ReadStream(int len,Stream st)
        {
            byte[] data = null;

            if (len == -1)
            {
                MemoryStream ms = new MemoryStream();
                int r = 0;
                byte[] buffer = new byte[4096];

                do
                {
                    r = st.Read(buffer, 0, 4096);
                    ms.Write(buffer, 0, r);
                } while (r > 0);

                data = new byte[ms.Length];
                Array.Copy(ms.GetBuffer(), data, ms.Length);
            }
            else
            {
                data = new byte[len];

                int r = 0;
                int offset = 0;
                do
                {
                    r = st.Read(data, offset, (int)len - offset);
                    offset += r;
                } while (offset < len);

                Console.WriteLine("read " + offset + " of " + len);
            }
            return data;

        }

        /// <summary>
        /// Read data from the given URL
        /// </summary>
        /// <param name="url"></param>
        /// <param name="useCache">If item exists in the cache, dont even make a request</param>
        /// <returns></returns>
        public Stream ReadData(string url, bool useCache=false)
        {
            string key = "data:" + url;
            byte[] data;
            CacheItem item = cache.Get("data:" + url);

            Uri abs = new Uri(regionUri, url);
            HttpWebRequest req = BuildRequest(abs);
            
            if (item != null)
            {
                // if a cache item exists send its create date
                // along with the request
                req.IfModifiedSince = item.Stats.Created;
            }

            // add authentication headers if
            // keys have been defined
            Authenticate(req);

            // item WAS cached, at we want to use it
            // instead of checking for new stuff
            if(item!=null && useCache==true)
            {
                // check if there is newer data on the sever by cache time
                data = (byte[])item.Value;
            }
            else
            {
                Console.WriteLine("getting " + abs);

                //  HttpStatusCode.NotModified
                HttpWebResponse res = GetResponse(req);
                
                if (res.StatusCode == HttpStatusCode.NotModified)
                {
                    // only way we get a NotModified is we had an 
                    // item to start with
                    data = (byte[])item.Value;
                }
                else
                {
                    data = HandleResponse(key, res);                
                }

            }

            return new MemoryStream(data);
        }

        private HttpWebResponse GetResponse(HttpWebRequest req)
        {
            HttpWebResponse res;
            try
            {
                res = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException wex)
            {
                // 404, 302, etc...
                res = wex.Response as HttpWebResponse;
            }
            return res;
        }

        private byte[] HandleResponse(string key, HttpWebResponse res)
        {
            byte[] data;

            Stream st = DecodeResponse(res);
            int len = (int)res.ContentLength;
            Console.WriteLine("got " + res.ResponseUri + " (" + len);

            // if the item has an Expires date use that
            string exp = res.Headers["Expires"];
            DateTime expire = DateTime.UtcNow + TimeSpan.FromHours(1);
            if (exp != null)
            {
                expire = DateTime.Parse(exp).ToUniversalTime();
            }
            data = ReadStream(len, st);
            
            // must close STREAM and RESPONSE
            st.Close();
            res.Close();

            CacheItem item = new CacheItem(key, data);
            item.Stats.Expire = expire;

            cache.Set(item);

            return data;
        }
        #region BeginRead
        /*
        class AsyncState
        {
            public FastAsyncResult res;
            public HttpWebRequest request;
            public AsyncState(FastAsyncResult res)
            {
                this.res = res;                
            }
        }
        private IAsyncResult BeginReadData(string url, AsyncCallback callback, object state)
        {
            Uri abs = new Uri(regionUri, url);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(abs);
            Authenticate(req);

            FastAsyncResult far = new FastAsyncResult(callback, state);
            AsyncState st = new AsyncState(far);
            st.request = req;
            req.BeginGetResponse(ReadComplete, st);

            ThreadPool.RegisterWaitForSingleObject(far.AsyncWaitHandle, new WaitOrTimerCallback(ReadTimeout), st, 3000, true);
            return far;
        }

        // Abort the request if the timer fires. 
        private static void ReadTimeout(object state, bool timedOut)
        {
            if (timedOut)
            {
                AsyncState asyncState = state as AsyncState;
                WebRequest request = asyncState.request;
                if (request != null)
                {
                    request.Abort();
                }

                // set complete
                asyncState.res.SetComplete();
            }
        }

        private Stream EndRead(IAsyncResult result)
        {
            FastAsyncResult far = result as FastAsyncResult;
            // wait for it to finish
            far.AsyncWaitHandle.WaitOne();

            // timed out
            if (far.InternalState == null) return null;

            WebResponse res = far.InternalState as WebResponse;
            return res.GetResponseStream();
        }

        /// <summary>
        /// Callback for BeginGetResponse
        /// </summary>
        /// <param name="result"></param>
        private void ReadComplete(IAsyncResult result)
        {
            AsyncState st = result.AsyncState as AsyncState;
            WebResponse response = st.request.EndGetResponse(result);
            st.res.m_internal = response;

            // now that we have set the response, set the Complete flag
            st.res.SetComplete();
        }
         */
        #endregion

        /// <summary>
        /// Add Authentication headers to request
        /// </summary>
        /// <param name="req"></param>
        public void Authenticate(HttpWebRequest req)
        {
            // no key set, so dont even try
            if (privateKey == null) return;

            // use the same date through out
            DateTime date = DateTime.Now.ToUniversalTime();

            //  To make this v2.0 to v3.5 friendly we have to use a little reflection
            // to set the "Date" header.  v4.0 gives us the Date property to do it.
            //            

            // use the Protected method to set the value
            Type type = req.Headers.GetType();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            MethodInfo method = type.GetMethod("AddWithoutValidate", flags);
            method.Invoke(req.Headers, new[] { "Date", date.ToString("r") });

            // this is .net v4.0 only
            //req.Date = DateTime.Now;

            string dateStr = date.ToString("r");

            // HttpUtility.UrlPathEncode(req.RequestUri.LocalPath)
            // Absolute path should already be encoded
            string enc = req.RequestUri.AbsolutePath; 

            string stringToSign = req.Method + "\n" +
                dateStr + "\n" +
                enc + "\n";

            byte[] key = new byte[32];
            byte[] buffer = Encoding.UTF8.GetBytes(stringToSign);
            HMACSHA1 hmac = new HMACSHA1(privateKey);
            byte[] hash = hmac.ComputeHash(buffer);
            string sig = Convert.ToBase64String(hash);

            string auth = "BNET " + PublicKey + ":" + sig;

            req.Headers["Authorization"] = auth;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if(cache!=null) cache.Dispose();
            cache = null;
        }

        #endregion
    }
}
