using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Debug = UnityEngine.Debug;

namespace CrazyPanda.UnityCore.PandaTasks.PerfTests
{
    internal sealed class UnitySynchronizationContext : SynchronizationContext, IDisposable
    {
        private const int kAwqInitialCapacity = 20;
        private readonly List<WorkRequest> m_AsyncWorkQueue;
        private readonly List<WorkRequest> m_CurrentFrameWork = new List<WorkRequest>(kAwqInitialCapacity);
        private readonly int m_MainThreadID;
        private int m_TrackedCount = 0;
        private SynchronizationContext _oldContext;

        private UnitySynchronizationContext(int mainThreadID)
        {
            _oldContext = SynchronizationContext.Current;
            m_AsyncWorkQueue = new List<WorkRequest>(kAwqInitialCapacity);
            m_MainThreadID = mainThreadID;
        }

        private UnitySynchronizationContext(List<WorkRequest> queue, int mainThreadID)
        {
            _oldContext = SynchronizationContext.Current;
            m_AsyncWorkQueue = queue;
            m_MainThreadID = mainThreadID;
        }

        public void Dispose()
        {
            SetSynchronizationContext( _oldContext );
        }

        // Send will process the call synchronously. If the call is processed on the main thread, we'll invoke it
        // directly here. If the call is processed on another thread it will be queued up like POST to be executed
        // on the main thread and it will wait. Once the main thread processes the work we can continue
        public override void Send(SendOrPostCallback callback, object state)
        {
            if (m_MainThreadID == System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                callback(state);
            }
            else
            {
                using (var waitHandle = new ManualResetEvent(false))
                {
                    lock (m_AsyncWorkQueue)
                    {
                        m_AsyncWorkQueue.Add(new WorkRequest(callback, state, waitHandle));
                    }
                    waitHandle.WaitOne();
                }
            }
        }

        public override void OperationStarted() { Interlocked.Increment(ref m_TrackedCount); }
        public override void OperationCompleted() { Interlocked.Decrement(ref m_TrackedCount); }

        // Post will add the call to a task list to be executed later on the main thread then work will continue asynchronously
        public override void Post(SendOrPostCallback callback, object state)
        {
            lock (m_AsyncWorkQueue)
            {
                m_AsyncWorkQueue.Add(new WorkRequest(callback, state));
            }
        }

        // CreateCopy returns a new UnitySynchronizationContext object, but the queue is still shared with the original
        public override SynchronizationContext CreateCopy()
        {
            return new UnitySynchronizationContext(m_AsyncWorkQueue, m_MainThreadID);
        }

        // Exec will execute tasks off the task list
        private void Exec()
        {
            lock (m_AsyncWorkQueue)
            {
                m_CurrentFrameWork.AddRange(m_AsyncWorkQueue);
                m_AsyncWorkQueue.Clear();
            }

            // When you invoke work, remove it from the list to stop it being triggered again (case 1213602)
            while (m_CurrentFrameWork.Count > 0)
            {
                WorkRequest work = m_CurrentFrameWork[0];
                m_CurrentFrameWork.Remove(work);
                work.Invoke();
            }
        }

        public bool HasPendingTasks()
        {
            return m_AsyncWorkQueue.Count != 0;
        }

        // SynchronizationContext must be set before any user code is executed. This is done on
        // Initial domain load and domain reload at MonoManager ReloadAssembly
        public static UnitySynchronizationContext CreateSynchronizationContext()
        {
            var context = new UnitySynchronizationContext( System.Threading.Thread.CurrentThread.ManagedThreadId );
            SynchronizationContext.SetSynchronizationContext(context);
            return context;
        }

        // All requests must be processed on the main thread where the full Unity API is available
        // See ScriptRunDelayedTasks in PlayerLoopCallbacks.h
        public void ExecuteTasks()
        {
            this.Exec();
        }

        public static bool ExecutePendingTasks(long millisecondsTimeout)
        {
            var context = SynchronizationContext.Current as UnitySynchronizationContext;
            if (context == null)
            {
                return true;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (context.HasPendingTasks())
            {
                if (stopwatch.ElapsedMilliseconds > millisecondsTimeout)
                {
                    break;
                }

                context.Exec();
                Thread.Sleep(1);
            }

            return !context.HasPendingTasks();
        }

        private struct WorkRequest
        {
            private readonly SendOrPostCallback m_DelagateCallback;
            private readonly object m_DelagateState;
            private readonly ManualResetEvent m_WaitHandle;

            public WorkRequest(SendOrPostCallback callback, object state, ManualResetEvent waitHandle = null)
            {
                m_DelagateCallback = callback;
                m_DelagateState = state;
                m_WaitHandle = waitHandle;
            }

            public void Invoke()
            {
                try
                {
                    m_DelagateCallback( m_DelagateState );
                }
                finally
                {
                    if (m_WaitHandle != null)
                        m_WaitHandle.Set();
                }
            }
        }
    }
}