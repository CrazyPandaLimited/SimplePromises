using System.Diagnostics;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.Utils;
using UnityEngine;

[ DebuggerNonUserCode ]
public static class AsyncOperationAwaiterExtensions
{
    public static AsyncOperationAwaiter< T > GetAwaiter <T>( this T asyncOperation ) where T : AsyncOperation => 
        new AsyncOperationAwaiter< T >( asyncOperation );

    public static async IPandaTask< T > WaitAsync< T >( this T asyncOperation, IProgressTracker< float > progressTracker ) where T : AsyncOperation
    {
        progressTracker.ThrowArgumentNullExceptionIfNull( nameof(progressTracker) );
        
        while( !asyncOperation.isDone )
        {
            progressTracker.ReportProgress( asyncOperation.progress );
            await Task.Yield();
        }

        progressTracker.ReportProgress( 1.0f );

        return asyncOperation;
    }
}