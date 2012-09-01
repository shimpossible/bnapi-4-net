using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BNapi4Net
{
    class FastAsyncResult : IAsyncResult, IDisposable
    {

        // Fields

        private object m_state;
        private ManualResetEvent m_waitHandle;
        private bool m_isCompleted;
        private AsyncCallback m_callback;
        internal object m_internal;

        // Constructors

        internal FastAsyncResult(AsyncCallback callback, object state)
        {            
            m_callback = callback;
            m_state = state;
        }

        // Properties

        public object AsyncState
        {
            get { return m_state; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return LazyCreateWaitHandle(); }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public bool IsCompleted
        {
            get { return m_isCompleted; }
        }

        internal object InternalState
        {
            get { return m_internal; }
        }

        // Methods

        public void Dispose()
        {
            if (m_waitHandle != null)
            {
                m_waitHandle.Close();
            }
        }

        public void SetComplete()
        {
            // We set the boolean first.
            m_isCompleted = true;

            // And then, if the wait handle was created, we need to signal it. Note the
            // use of a memory barrier. This is required to ensure the read of m_waitHandle
            // never moves before the store of m_isCompleted; otherwise we might encounter a
            // race that leads us to not signal the handle, leading to a deadlock. We can't
            // just do a volatile read of m_waitHandle, because it is possible for an acquire
            // load to move before a store release.
            Thread.MemoryBarrier();
            if (m_waitHandle != null)
            {
                m_waitHandle.Set();
            }

            // If the callback is non-null, we invoke it.
            if (m_callback != null)
            {
                m_callback(this);
            }
        }

        private WaitHandle LazyCreateWaitHandle()
        {
            if (m_waitHandle != null)
            {
                return m_waitHandle;
            }

            ManualResetEvent newHandle = new ManualResetEvent(false);
            if (Interlocked.CompareExchange(ref m_waitHandle, newHandle, null) != null)
            {
                // We lost the race. Release the handle we created, it's garbage.
                newHandle.Close();
            }

            if (m_isCompleted)
            {
                // If the result has already completed, we must ensure we return the
                // handle in a signaled state. The read of m_isCompleted must never move
                // before the read of m_waitHandle earlier; the use of an interlocked
                // compare-exchange just above ensures that. And there's a race that could
                // lead to multiple threads setting the event; that's no problem.
                m_waitHandle.Set();
            }

            return m_waitHandle;
        }

    }
}
