using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// This is left only for compatibility with old code
    /// </summary>
    public struct IPandaTaskMethodBuilder
    {
        private PandaTaskMethodBuilder _pandaTaskMethodBuilder;

        public IPandaTask Task => _pandaTaskMethodBuilder.Task;

        public static IPandaTaskMethodBuilder Create() => default;

        public void SetStateMachine( IAsyncStateMachine stateMachine )
        {
            _pandaTaskMethodBuilder.SetStateMachine( stateMachine );
        }

        [ DebuggerStepThrough ]
        public void Start< TStateMachine >( ref TStateMachine stateMachine )
            where TStateMachine : IAsyncStateMachine
        {
            _pandaTaskMethodBuilder.Start( ref stateMachine );
        }

        public void AwaitOnCompleted< TAwaiter, TStateMachine >( ref TAwaiter awaiter, ref TStateMachine machine )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            _pandaTaskMethodBuilder.AwaitOnCompleted( ref awaiter, ref machine );
        }

        public void AwaitUnsafeOnCompleted< TAwaiter, TStateMachine >( ref TAwaiter awaiter, ref TStateMachine machine )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            _pandaTaskMethodBuilder.AwaitUnsafeOnCompleted( ref awaiter, ref machine );
        }

        public void SetResult() => _pandaTaskMethodBuilder.SetResult();
        
        public void SetException( Exception ex ) => _pandaTaskMethodBuilder.SetException( ex );
    }

    public struct IPandaTaskMethodBuilder< TResult >
    {
        private PandaTaskMethodBuilder< TResult > _pandaTaskMethodBuilder;

        public IPandaTask< TResult > Task => _pandaTaskMethodBuilder.Task;

        public static IPandaTaskMethodBuilder< TResult > Create() => default;

        public void SetStateMachine( IAsyncStateMachine stateMachine )
        {
            _pandaTaskMethodBuilder.SetStateMachine( stateMachine );
        }

        [ DebuggerStepThrough ]
        public void Start< TStateMachine >( ref TStateMachine stateMachine )
            where TStateMachine : IAsyncStateMachine
        {
            _pandaTaskMethodBuilder.Start( ref stateMachine );
        }

        public void AwaitOnCompleted< TAwaiter, TStateMachine >( ref TAwaiter awaiter, ref TStateMachine machine )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            _pandaTaskMethodBuilder.AwaitOnCompleted( ref awaiter, ref machine  );
        }

        public void AwaitUnsafeOnCompleted< TAwaiter, TStateMachine >( ref TAwaiter awaiter, ref TStateMachine machine )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            _pandaTaskMethodBuilder.AwaitUnsafeOnCompleted( ref awaiter, ref machine  );
        }

        public void SetResult( TResult value ) => _pandaTaskMethodBuilder.SetResult( value );

        public void SetException( Exception ex ) => _pandaTaskMethodBuilder.SetException( ex );
    }
}