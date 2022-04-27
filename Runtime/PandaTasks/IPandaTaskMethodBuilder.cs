using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// This is left only for compatibility with old code
    /// </summary>
    [Obsolete("Use PandaTaskMethodBuilder instead it", false)]
    public struct IPandaTaskMethodBuilder
    {
        private PandaTask< VoidResult > _task;

        public IPandaTask Task => _task ?? (_task = new PandaTask< VoidResult >());

        public static IPandaTaskMethodBuilder Create() => default;

        public void SetStateMachine( IAsyncStateMachine stateMachine )
        {
        }

        [ DebuggerStepThrough ]
        public void Start< TStateMachine >( ref TStateMachine stateMachine )
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void AwaitOnCompleted< TAwaiter, TStateMachine >( ref TAwaiter awaiter, ref TStateMachine machine )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            PandaTaskMethodBuilder< VoidResult >.AwaitOnCompleted( ref awaiter, ref machine, ref _task );
        }

        public void AwaitUnsafeOnCompleted< TAwaiter, TStateMachine >( ref TAwaiter awaiter, ref TStateMachine machine )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            PandaTaskMethodBuilder< VoidResult >.AwaitOnCompleted( ref awaiter, ref machine, ref _task );
        }

        public void SetResult()
        {
            if( _task == null )
            {
                _task = new PandaTask< VoidResult >();
            }

            _task.Resolve();
        }
        
        public void SetException( Exception ex )
        {
            if( _task == null )
            {
                _task = new PandaTask< VoidResult >();
            }

            _task.Reject( ex );
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        struct VoidResult
        {
        }
    }

    [Obsolete("Use PandaTaskMethodBuilder instead it", false)]
    public struct IPandaTaskMethodBuilder< TResult >
    {
        private PandaTask< TResult > _task;

        public IPandaTask< TResult > Task => _task ?? (_task = new PandaTask< TResult >());

        public static IPandaTaskMethodBuilder< TResult > Create() => default;

        public void SetStateMachine( IAsyncStateMachine stateMachine )
        {
        }

        [ DebuggerStepThrough ]
        public void Start< TStateMachine >( ref TStateMachine stateMachine )
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void AwaitOnCompleted< TAwaiter, TStateMachine >( ref TAwaiter awaiter, ref TStateMachine machine )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            PandaTaskMethodBuilder< TResult >.AwaitOnCompleted( ref awaiter, ref machine, ref _task );
        }

        public void AwaitUnsafeOnCompleted< TAwaiter, TStateMachine >( ref TAwaiter awaiter, ref TStateMachine machine )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            PandaTaskMethodBuilder<TResult>.AwaitOnCompleted( ref awaiter, ref machine, ref _task );
        }

        public void SetResult( TResult value )
        {
            if( _task == null )
            {
                _task = new PandaTask< TResult >();
            }

            _task.SetValue( value );
        }

        public void SetException( Exception ex )
        {
            if( _task == null )
            {
                _task = new PandaTask< TResult >();
            }

            _task.Reject( ex );
        }
    }
}