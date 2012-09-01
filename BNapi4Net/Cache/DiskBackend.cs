using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BNapi4Net.Cache
{
    public class DiskBackend : ICacheBackend, IDisposable
    {
        /// <summary>
        /// fallback backend to put expired entries
        /// </summary>
        ICacheBackend fallback;

        long maxFileSize;   // size to limit file to
        long usedSize;      // total bytes used        
        string dataFile;
        string indexFile;

        BinaryFormatter formatter = new BinaryFormatter();

        // list of free segments
        LinkedList<FileSegment> FreeList = new LinkedList<FileSegment>();

        /// <summary>
        /// Defines a segment in the file
        /// </summary>
        protected class FileSegment
        {
            /// <summary>
            /// Position in file data starts
            /// </summary>
            public long Position;
            /// <summary>
            /// Length of entry in file
            /// </summary>
            public long Length;

            public FileSegment(long p, long l)
            {
                Position = p;
                Length = l;
            }

        }

        /// <summary>
        /// Describes the data stored in the file
        /// </summary>
        protected class FileEntry
        {
            public string Key;
            public CacheItem item;
            public int version;

            /// <summary>
            /// Where in the file the data is found
            /// </summary>
            public FileSegment Segment;

            /// <summary>
            /// Statistics, such as access time and expire time.
            /// This is in memory (and on disk) for quick access
            /// </summary>
            public CacheStatistics Stats;


            public FileEntry(long pos, long len, CacheStatistics stats)
            {
                Segment = new FileSegment(pos, len);                
                Stats = stats;
            }

            public override string ToString()
            {
                return "{FileEntry " + this.Segment.Position + " to " + (this.Segment.Position+this.Segment.Length) + " ("+
                    this.Segment.Length
                    +")}";
            }
        }

        /// <summary>
        /// In memory index of where to find the data
        /// </summary>
        Dictionary<string, FileEntry> data = new Dictionary<string, FileEntry>();
        LinkedList<FileEntry> spoolQueue = new LinkedList<FileEntry>();
        int pendingWrites = 0;

        public DiskBackend(string filename, long filesize, ICacheBackend fallback)
        {
            this.fallback = fallback;
            usedSize = 0;
            maxFileSize = filesize;

            FileInfo fi = new FileInfo(filename);
            FreeList.AddLast(new FileSegment(0, long.MaxValue));

            dataFile = filename;
            indexFile = dataFile + ".index";

            // create the file if it doesnt exist
            if (!File.Exists(filename))
            {
                using (FileStream fs = File.Create(filename))
                {
                    // do nothing
                }
            }
            try
            {
                LoadIndex();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // clear the index
                data.Clear();
                FreeList.Clear();
                FreeList.AddLast(new FileSegment(0, long.MaxValue));

            }
        }

        #region ICacheBackend Members

        public bool Full
        {
            get 
            {
                return usedSize >= maxFileSize;             
            }
        }

        public void Touch(CacheItem item)
        {
            FileEntry tmpEntry = data[item.Key];
            tmpEntry.Key = item.Key;
            tmpEntry.Stats.LastAccessTime = DateTime.Now;
            
            SpoolEntry(tmpEntry);
            
        }

        public CacheItem Get(string key)
        {
            FileEntry entry;
            CacheItem ci;
            if (data.TryGetValue(key, out entry) == false)
            {
                // not found, maybe the fallback has it?
                if (fallback != null)
                {
                    ci = fallback.Get(key);
                    if(ci!=null) Set(ci);
                    return ci;
                }
                else
                {
                    return null;
                }
            }

            ci = entry.item;
            if(ci==null) // not still in memory
            {
                // lock the entry so that it wont get moved during Compact
                lock (entry)
                {
                    // read from disk..
                    ci = Deserialize(entry);
                }
            }

            return ci;
        }

        CacheItem Deserialize(FileEntry entry)
        {
            CacheItem ci;
            // has been spooled to disk
            MemoryStream ms;
            using (Stream st = File.Open(dataFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite) )
            {
                st.Position = entry.Segment.Position;
                byte[] buff = new byte[entry.Segment.Length];
                st.Read(buff, 0, buff.Length);
                ms = new MemoryStream(buff);
            }
            using (BinaryReader reader = new BinaryReader(ms) )
            {
                string _key = reader.ReadString();
                long _lat = reader.ReadInt64();
                long _created = reader.ReadInt64();
                long _expired = reader.ReadInt64();

                object val = formatter.Deserialize(ms);
                ci = new CacheItem(_key, val, new DateTime(_lat, DateTimeKind.Utc), new DateTime(_created, DateTimeKind.Utc));
                ci.Stats.Expire = new DateTime(_expired);
            }
            return ci;
        }

        /// <summary>
        /// Queue FileEntry to be written to disk
        /// </summary>
        /// <param name="entry"></param>
        protected void SpoolEntry(FileEntry entry)
        {
            // if the spool gets too long, wait some time
            int count = spoolQueue.Count;
            while (count + pendingWrites > 5)
            {
                Console.WriteLine("Waiting for spool to finish: " + pendingWrites);
                Thread.Sleep(15);
                count = spoolQueue.Count;
            }

            lock (spoolQueue)
            {
                spoolQueue.AddLast(entry);
            }

            // Queue work
            Task.Factory.StartNew(new Action(() =>
            {
                SpoolTask();
            }));
        }

        public bool Set(CacheItem item)
        {
            bool isNew = data.ContainsKey(item.Key);

            FileEntry tmpEntry = new FileEntry(0, 0, item.Stats)
            {
                Key = item.Key,
                item = item
            };

            Console.WriteLine("adding " + item.Key);
            data[item.Key] = tmpEntry;

            SpoolEntry(tmpEntry);

            return isNew;
        }

        public CacheItem Remove(string key)
        {
            // TODO: get the cacheItem first..
            FileEntry e;
            CacheItem ret = null;
            pruneLock.EnterWriteLock();
            try
            {
                if (data.TryGetValue(key, out e))
                {
                    Console.WriteLine("Removing " + key);
                    ret = Deserialize(e);
                    data.Remove(key);

                    // place expired item into the fallback,
                    // if we have one
                    if (this.fallback != null)
                    {
                        fallback.Set(ret);
                    }
                }
            }
            finally
            {
                pruneLock.ExitWriteLock();
            }

            if (e != null)
            {
                MarkFreeSpace(e.Segment);
            }

            return ret;
        }

        public CacheItem Remove(CacheItem item)
        {
            return Remove(item.Key);
        }

        #endregion

        public int Count
        {
            get
            {
                return data.Count;
            }
        }
        /// <summary>
        /// Mark the segment as free and available for use
        /// This will update the FreeList and merge nearby 
        /// entries
        /// </summary>
        /// <param name="seg">segment to mark as free</param>
        protected void MarkFreeSpace(FileSegment seg)
        {
            usedSize -= seg.Length;

            LinkedListNode<FileSegment> node = FreeList.First;

            // Find where in the freelist to insert this segment
            // the list should be in order, so find the entry
            // just after this
            while (node != null)
            {
                // node is to the right
                if (node == null ||
                    node.Value.Position > seg.Position)
                {
                    FreeList.AddBefore(node, seg);
                    break;
                }
                node = node.Next;
            }

            // end of file..
            if (node.Value.Position + node.Value.Length == long.MaxValue)
            {
                // truncate file
                using (FileStream fs = File.Open(dataFile, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                {
                    fs.SetLength(node.Value.Position);
                }
            }

            node = node.Previous;

            // walk backwards from this node, and compare it to its NEXT node
            // if they touch, merge them. and remove one of them
            while (node != null)
            {
                FileSegment nextSeg = node.Next.Value;
                LinkedListNode<FileSegment> prev = node.Previous;

                // this node touches the next node
                if ((node.Value.Position + node.Value.Length) == nextSeg.Position)
                {
                    node.Next.Value.Position = node.Value.Position;
                    node.Next.Value.Length += node.Value.Length;                    
                    FreeList.Remove(node);                    
                }
                else
                {
                    if (node.Previous==null ||
                        (node.Previous.Value.Position + node.Previous.Value.Length) != node.Value.Position)
                    // nodes dont merge, give up merging..
                    break;
                }
                node = prev;
            }
        }


        /// <summary>
        /// Spool an entry to disk
        /// </summary>
        private void SpoolTask()
        {
            FileEntry e = null;
            lock (spoolQueue)
            {
                e = spoolQueue.First.Value;
                spoolQueue.RemoveFirst();
            }
            Interlocked.Increment(ref pendingWrites);
            try
            {
                // the entry was updated between now and then..
                if (data.ContainsValue(e) == false) return;

                ///////////////////////////////////////////
                // Do the actual file writing
                //
                // Serialize the object into a temporary stream
                // to get the size
                MemoryStream tmpStream = new MemoryStream(256);
                BinaryWriter w = new BinaryWriter(tmpStream);

                // write CacheItem values
                w.Write(e.Key);
                w.Write(e.Stats.LastAccessTime.Ticks);
                w.Write(e.Stats.Created.Ticks);
                w.Write(e.Stats.Expire.Ticks);
                w.Flush();

                FileSegment freeSeg;
                byte[] byteData;

                // serialize object
                if (e.item == null)
                {
                    // just updating Stats..
                    
                    freeSeg = e.Segment;  // where was this written the last time
                    byteData = tmpStream.ToArray();
                }
                else
                {
                    formatter.Serialize(tmpStream, e.item.Value);

                    byteData = tmpStream.ToArray();
                    long neededSpace = byteData.Length;

                    // keep track of the ammount of space used in the file
                    // prune as needed
                    Prune(neededSpace);

                    // The fileseg here ensures the writer below wont overwrite 
                    // other entries                        
                    freeSeg = FindFreeSegment(neededSpace);

                }


                long pos = freeSeg.Position;
                using (Stream writer = File.Open(dataFile, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                {
                    // write to file
                    writer.Position = pos;
                    writer.Write(byteData, 0, byteData.Length);
                    writer.Flush();
                }

                // wasnt a 'touch'
                if (e.item != null)
                {
                    // update the FileEntry with the known position
                    e.Segment.Position = pos;
                    e.Segment.Length = byteData.Length;
                }

                // now that everything has been set, we can clear this
                // TODO: Barrier
                e.item = null;

            }
            finally
            {
                Interlocked.Decrement(ref pendingWrites);
            }
        }


        ReaderWriterLockSlim pruneLock = new ReaderWriterLockSlim(  LockRecursionPolicy.SupportsRecursion );
        
        protected void Compact()
        {
            pruneLock.EnterWriteLock();
            try
            {
                foreach (FileEntry e in data.Values.ToArray() )
                {
                    // is there free space before this entry?
                    FileSegment before = FindFreeSegmentBefore(e.Segment.Position);

                    // no free space directy before?
                    if (before == null) continue;
                    
                    lock (e)
                    {
                        using (Stream st = File.Open(dataFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite) )
                        {
                            st.Position = e.Segment.Position;
                            byte[] buff = new byte[e.Segment.Length];
                            st.Read(buff, 0, buff.Length);

                            // mark the ending as free space
                            FileSegment oldSpace = new FileSegment(before.Position + e.Segment.Length, before.Length);
                            MarkFreeSpace(oldSpace);

                            st.Position = before.Position;
                            st.Write(buff, 0, buff.Length);
                            st.Flush();
                            
                            // move our spot back
                            e.Segment.Position = before.Position;
                            if (e.Segment.Position == 154)
                            {
                                e.Segment = e.Segment;
                            }
                        }
                    }
                }
            }
            finally
            {
                pruneLock.ExitWriteLock();
            }
        }
        /// <summary>
        /// Prune expired items and old items untill we are under the needed space
        /// </summary>
        /// <param name="needed">amount of space we need</param>
        protected void Prune(long needed)
        {
            long _maxSize = maxFileSize - needed;
            pruneLock.EnterWriteLock();
            try
            {
                if (usedSize < _maxSize) return;

                //everything that has expired
                DateTime now = DateTime.Now;
                var tmp1 = data.Where(x => x.Value.Stats.Expire <= now);
                foreach (KeyValuePair<string, FileEntry> c in tmp1)
                {
                    Remove(c.Key);
                }

                // do we still need to remove more?
                // remove the least accessed items first
                if (usedSize >= _maxSize)
                {

                    // ascending order (ie, oldest first)
                    var tmp = data.OrderBy(x => x.Value.Stats.LastAccessTime);

                    foreach (KeyValuePair<string, FileEntry> c in tmp)
                    {
                        // dont need to remove any more
                        if (usedSize < _maxSize) break;

                        Remove(c.Key);
                    }
                }
                Compact();
            }
            finally
            {
                pruneLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Find free space before a point in the file
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected FileSegment FindFreeSegmentBefore(long pos)
        {
            FileSegment seg;

            pruneLock.EnterWriteLock();
            try
            {
                LinkedListNode<FileSegment> node = FreeList.First;
                while (node != null)
                {
                    // this free space is DIRECTLY before the position
                    if ((node.Value.Position + node.Value.Length) == pos)
                    {
                        FileSegment s = node.Value;
                        // remove from free list so someone else doesnt use it
                        FreeList.Remove(node);
                        return s;
                    }
                    node = node.Next;
                }
            }
            finally
            {
                pruneLock.ExitWriteLock();
            }

            // couldn't find one
            return null;
        }

        /// <summary>
        /// Finds the first segment large enough 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        protected FileSegment FindFreeSegment(long length)
        {
            FileSegment seg;

            pruneLock.EnterWriteLock();
            try
            {
                usedSize += length;

                LinkedListNode<FileSegment> node = FreeList.First;
                while (node != null)
                {
                    // enough space..
                    if (node.Value.Length >= length)
                    {
                        long left = node.Value.Length - length;
                        seg = new FileSegment(node.Value.Position, length);

                        if (left > 0)
                        {
                            // some space left in the segment, just update the start
                            // position
                            node.Value.Length -= length;
                            node.Value.Position += length;
                        }
                        else
                        {
                            // used all the space, remove the node
                            FreeList.Remove(node);
                        }

                        return seg;
                    }
                    node = node.Next;
                }
            }
            finally
            {
                pruneLock.ExitWriteLock();
            }
            
            // couldn't find one
            return null;
        }

        protected void MarkUsed(FileSegment seg)
        {
            pruneLock.EnterWriteLock();
            try
            {
                long _st = seg.Position;
                long _sp = _st + seg.Length;
                LinkedListNode<FileSegment> node = FreeList.First;
                while (node != null)
                {
                    long st = node.Value.Position;
                    long sp = st + node.Value.Length;

                    // current segment contains used segment
                    if(st <= _st && _sp <= sp)
                    {
                        FileSegment before = new FileSegment(node.Value.Position, _st-st);
                        FileSegment after = new FileSegment(_sp, sp - _sp);

                        if (before.Length > 0)
                        {
                            FreeList.AddBefore(node, before);                            
                        }

                        if (after.Length > 0)
                        {
                            FreeList.AddAfter(node, after);
                        }
                        FreeList.Remove(node);
                        break;
                    }

                    node = node.Next;
                }
            }
            finally
            {
                pruneLock.ExitWriteLock();
            }
        }

        public void Flush()
        {
            // wait for spool to finish
            while (this.pendingWrites + spoolQueue.Count > 0)
            {
                Thread.Sleep(1);
            }
        }

        protected void SaveIndex()
        {
            using (BinaryWriter w = new BinaryWriter(File.Open(indexFile, FileMode.Create)))
            {
                w.Write(data.Count);
                foreach(KeyValuePair<string, FileEntry> entry in data)
                {
                    w.Write(entry.Value.Segment.Position);
                    w.Write(entry.Value.Segment.Length);
                }
            }
        }

        protected void LoadIndex()
        {
            using (BinaryReader r = new BinaryReader(File.Open(indexFile, FileMode.OpenOrCreate)))
            {
                if (r.BaseStream.Length < 4) return;

                int count = r.ReadInt32();
                for(int i=0;i<count;i++)
                {                    
                    long pos = r.ReadInt64();
                    long len = r.ReadInt64();
                    FileEntry entry = new FileEntry(pos, len, null);

                    CacheItem ci = Deserialize(entry);
                    data[ci.Key] = entry;

                    entry.Stats = ci.Stats;
                    entry.Key = ci.Key;

                    MarkUsed(entry.Segment);
                }
            }
        }
        #region IDisposable Members

        public void Dispose()
        {
            Flush();
            SaveIndex();
        }

        #endregion
    }
}
