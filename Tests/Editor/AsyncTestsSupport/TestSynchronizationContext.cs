using System.Collections.Generic;
using System.Threading;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// SynchronizationContext tailored for usage with AsyncTest
    /// It doesn't use thread pool and just queues all callbacks internally
    /// Call <see cref="TestSynchronizationContext.Tick"/> to dispatch all queued callbacks
    /// </summary>
    internal class TestSynchronizationContext : SynchronizationContext
    {
        private Queue< (SendOrPostCallback callback, object state) > _queue = new Queue< (SendOrPostCallback callback, object state) >();

        public void Tick()
        {
            while( _queue.Count > 0 )
            {
                var (callback, state) = _queue.Dequeue();
                callback( state );
            }
        }

        public override void Post( SendOrPostCallback d, object state )
        {
            _queue.Enqueue( (d, state) );
        }
    }
}