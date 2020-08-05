#if (UNITY_EDITOR && CRAZYPANDA_UNITYCORE_PROMISES_ENABLE_ASYNC_TESTS_EDITOR) || CRAZYPANDA_UNITYCORE_PROMISES_ENABLE_ASYNC_TESTS_PLAYMODE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// SynchronizationContext tailored for usage with AsyncTest.
    /// It doesn't use thread pool and just queues all callbacks internally.
    /// Call <see cref="Tick"/> to dispatch all queued callbacks.
    /// </summary>
    public class TestSynchronizationContext : SynchronizationContext, IDisposable
    {
        private Queue< (SendOrPostCallback callback, object state) > _queue = new Queue< (SendOrPostCallback callback, object state) >();
        private SynchronizationContext _oldContext;
        private List< ExceptionDispatchInfo > _recordedExceptions = new List< ExceptionDispatchInfo >();

        /// <summary>
        /// Event that will be called alongside Tick.
        /// This event will be automatically cleared after each test, so you can safely assign local lamdbas to it.
        /// </summary>
        public static event Action OnTick;

        internal TestSynchronizationContext()
        {
            _oldContext = Current;
            SetSynchronizationContext( this );
        }

        /// <summary>
        /// Returns first unhandled exception and removes it from list. If no unhandled exceptions left returns null
        /// </summary>
        public static Exception HandleException()
        {
            if( Current is TestSynchronizationContext self )
            {
                if( self._recordedExceptions.Count > 0 )
                {
                    var ret = self._recordedExceptions[ 0 ];
                    self._recordedExceptions.RemoveAt( 0 );
                    return ret.SourceException;
                }
            }

            return null;
        }

        internal void Tick()
        {
            (SendOrPostCallback, object)[] callbacks = null;
            lock( _queue )
            {
                callbacks = _queue.ToArray();
                _queue.Clear();
            }

            foreach( var (callback, state) in callbacks )
            {
                try
                {
                    callback( state );
                }
                catch( Exception e )
                {
                    _recordedExceptions.Add( ExceptionDispatchInfo.Capture( e ) );
                }
            }

            OnTick?.Invoke();
        }

        public override void Post( SendOrPostCallback d, object state )
        {
            lock( _queue )
            {
                _queue.Enqueue( ( d, state ) );
            }
        }

        public void Dispose()
        {
            if( _oldContext != null )
            {
                SetSynchronizationContext( _oldContext );
                OnTick = null;
                _oldContext = null;

                var exceptions = _recordedExceptions;
                _recordedExceptions = null;

                if( exceptions.Count == 1 )
                {
                    exceptions[ 0 ].Throw();
                }
                else if( exceptions.Count > 1 )
                {
                    throw new AggregateException( exceptions.Select( e => e.SourceException ) );
                }
            }
        }
    }
}

#endif