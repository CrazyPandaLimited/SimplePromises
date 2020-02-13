using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CrazyPanda.UnityCore.PandaTasks
{
    public struct PandaTaskMethodBuilder
    {
        private PandaTask< VoidResult > _task;

        public IPandaTask Task => _task ?? (_task = new PandaTask< VoidResult >());

        public static PandaTaskMethodBuilder Create() => default;

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
}
