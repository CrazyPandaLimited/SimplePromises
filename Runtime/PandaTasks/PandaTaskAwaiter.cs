using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// Awaiter used with <see cref="IPandaTask"/>
    /// </summary>
    [ DebuggerNonUserCode ]
    public struct PandaTaskAwaiter : INotifyCompletion
    {
        private readonly IPandaTask _task;

        public bool IsCompleted => _task.Status != PandaTaskStatus.Pending;

        public PandaTaskAwaiter( IPandaTask task )
        {
            _task = task;
        }

        public void OnCompleted( Action continuation )
        {
            _task.Done( continuation ).Fail( _ => continuation() );
        }

        public void GetResult()
        {
            _task.ThrowIfError();
        }
    }
}