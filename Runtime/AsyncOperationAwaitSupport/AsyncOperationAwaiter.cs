using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CrazyPanda.UnityCore.PandaTasks
{
    [ DebuggerNonUserCode ]
    public readonly struct AsyncOperationAwaiter <TOperation> : INotifyCompletion where TOperation : AsyncOperation
    {
        public readonly TOperation AsyncOperation;

        public bool IsCompleted => AsyncOperation.isDone;

        public AsyncOperationAwaiter( TOperation asyncOperation ) => AsyncOperation = asyncOperation;

        public void OnCompleted( Action continuation ) => AsyncOperation.completed += _ => continuation();

        public TOperation GetResult() => AsyncOperation;
    }
}