﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CrazyPanda.UnityCore.PandaTasks
{
    public struct PandaTaskMethodBuilder< TResult >
    {
        private PandaTask< TResult > _task;

        public PandaTask< TResult > Task => _task ?? (_task = new PandaTask< TResult >());

        public static PandaTaskMethodBuilder< TResult > Create() => default;

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
            AwaitOnCompleted( ref awaiter, ref machine, ref _task );
        }

        public void AwaitUnsafeOnCompleted< TAwaiter, TStateMachine >( ref TAwaiter awaiter, ref TStateMachine machine )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            AwaitOnCompleted( ref awaiter, ref machine, ref _task );
        }

        public void SetResult( TResult value )
        {
            if( _task == null )
            {
                _task = new PandaTask<TResult>();
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

        internal static void AwaitOnCompleted<TAwaiter, TStateMachine>( ref TAwaiter awaiter, ref TStateMachine machine, ref PandaTask< TResult > taskField )
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted( GetMoveNextAction( ref taskField, ref machine ) );
        }

        private static Action GetMoveNextAction<TStateMachine>(ref PandaTask< TResult > taskField, ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            if( taskField is AsyncStateMachineTask<TStateMachine> stateMachineTask )
            {
                return stateMachineTask.MoveNextAction;
            }

            var ret = new AsyncStateMachineTask< TStateMachine >();

            taskField = ret;
            ret.StateMachine = stateMachine;

            return ret.MoveNextAction;
        }

        [ DebuggerNonUserCode ]
        private class AsyncStateMachineTask< TStateMachine > : PandaTask< TResult >
            where TStateMachine : IAsyncStateMachine
        {
            private Action _moveNextAction;

            public TStateMachine StateMachine;

            public Action MoveNextAction => _moveNextAction ?? (_moveNextAction = MoveNextStateMachine);

            // avoid using lambda cause it will generate another type
            private void MoveNextStateMachine()
            {
                StateMachine.MoveNext();
            }
        }
    }
}
