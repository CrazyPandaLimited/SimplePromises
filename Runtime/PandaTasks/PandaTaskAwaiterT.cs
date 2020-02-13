using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// Awaiter used with <see cref="IPandaTask{T}"/>
    /// </summary>
    [ DebuggerNonUserCode ]
    public struct PandaTaskAwaiter< T > : INotifyCompletion
    {
        private readonly IPandaTask< T > _task;

        public bool IsCompleted => _task.Status != PandaTaskStatus.Pending;

        public PandaTaskAwaiter( IPandaTask< T > task )
        {
            _task = task;
        }

        public void OnCompleted( Action continuation )
        {
            _task.Done( continuation ).Fail( _ => continuation() );
        }

        public T GetResult()
        {
            return _task.Result;
        }
    }
}