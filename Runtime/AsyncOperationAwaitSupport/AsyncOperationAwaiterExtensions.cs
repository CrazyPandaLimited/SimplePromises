using System.Diagnostics;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using UnityEngine;

[ DebuggerNonUserCode ]
public static class AsyncOperationAwaiterExtensions
{
    public static AsyncOperationAwaiter GetAwaiter( this AsyncOperation asyncOperation ) => new AsyncOperationAwaiter( asyncOperation );

    public static async IPandaTask WithProgressTracker( this AsyncOperation asyncOperation, IProgressTracker< float > progressTracker )
    {
        while( !asyncOperation.isDone )
        {
            progressTracker.ReportProgress ( asyncOperation.progress );
            await Task.Yield();
        }

        progressTracker.ReportProgress( 1.0f );
    }
}