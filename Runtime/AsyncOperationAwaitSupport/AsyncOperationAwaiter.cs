using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CrazyPanda.UnityCore.PandaTasks
{
    [ DebuggerNonUserCode ]
    public readonly struct AsyncOperationAwaiter : INotifyCompletion
    {
        private readonly AsyncOperation _asyncOperation;
        public bool IsCompleted => _asyncOperation.isDone;

        public AsyncOperationAwaiter( AsyncOperation asyncOperation ) => _asyncOperation = asyncOperation;

        public void OnCompleted( Action continuation ) => _asyncOperation.completed += _ => continuation();

        public void GetResult() { }
    }
}