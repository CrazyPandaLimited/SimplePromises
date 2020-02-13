using System;
using System.Collections.Generic;
using System.Threading;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// SynchronizationContext tailored for usage with AsyncTest.
    /// It doesn't use thread pool and just queues all callbacks internally.
    /// Call <see cref="Tick"/> to dispatch all queued callbacks.
    /// </summary>
    internal class TestSynchronizationContext : SynchronizationContext, IDisposable
    {
        private Queue< (SendOrPostCallback callback, object state) > _queue = new Queue< (SendOrPostCallback callback, object state) >();
        private SynchronizationContext _oldContext;

        /// <summary>
        /// Event that will be called alongside Tick.
        /// This event will be automatically cleared after each test, so you can safely assign local lamdbas to it.
        /// </summary>
        public static event Action OnTick;

        public TestSynchronizationContext()
        {
            _oldContext = Current;
            SetSynchronizationContext(this);
        }

        public void Tick()
        {
            (SendOrPostCallback, object)[] callbacks = null;
            lock( _queue )
            {
                callbacks = _queue.ToArray();
                _queue.Clear();
            }

            foreach( var (callback, state) in callbacks )
            {
                callback( state );                
            }

            OnTick?.Invoke();
        }

        public override void Post( SendOrPostCallback d, object state )
        {
            lock( _queue )
            {
                _queue.Enqueue( (d, state) );
            }
        }

        public void Dispose()
        {
            if(_oldContext != null)
            {
                SetSynchronizationContext(_oldContext);
                OnTick = null;
                _oldContext = null;
            }
        }
    }
}