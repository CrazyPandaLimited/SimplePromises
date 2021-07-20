using System.Diagnostics;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using UnityEngine.Networking;

[ DebuggerNonUserCode ]
public static class UnityWebRequestAsyncOperationAwaiterExtensions
{
    public static UnityWebRequestAwaiter GetAwaiter( this UnityWebRequestAsyncOperation asyncOperation ) => new UnityWebRequestAwaiter( asyncOperation );

    public static async IPandaTask< UnityWebRequest > WithProgressTracker( this UnityWebRequestAsyncOperation asyncOperation, IProgressTracker< float > progressTracker )
    {
        while( !asyncOperation.isDone )
        {
            progressTracker.ReportProgress ( asyncOperation.progress );
            await Task.Yield();
        }

        progressTracker.ReportProgress( 1.0f );

        return asyncOperation.webRequest;
    }
}