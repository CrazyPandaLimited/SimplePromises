using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.Utils;
using UnityEngine.Networking;

[ DebuggerNonUserCode ]
public static class UnityWebRequestExtensions
{
    public static AsyncOperationAwaiter< UnityWebRequestAsyncOperation > GetAwaiter( this UnityWebRequest webRequest )
    {
        return webRequest.SendWebRequest().GetAwaiter();
    }

    public static IPandaTask< UnityWebRequestAsyncOperation > WaitAsync( this UnityWebRequest webRequest, IProgressTracker< float > progressTracker )
    {
        progressTracker.CheckArgumentForNull( nameof(progressTracker) );
        return webRequest.SendWebRequest().WaitAsync( progressTracker, CancellationToken.None );
    }

    public static IPandaTask< UnityWebRequestAsyncOperation > WaitAsync( this UnityWebRequest webRequest, CancellationToken cancellationToken )
    {
        return webRequest.SendWebRequest().WaitAsync( null, cancellationToken );
    }

    public static IPandaTask< UnityWebRequestAsyncOperation > WaitAsync( this UnityWebRequest webRequest, IProgressTracker< float > progressTracker, CancellationToken cancellationToken )
    {
        progressTracker.CheckArgumentForNull( nameof(progressTracker) );
        return webRequest.SendWebRequest().WaitAsync( progressTracker, cancellationToken );
    }
    
    internal static async IPandaTask< UnityWebRequestAsyncOperation > WaitAsync( this UnityWebRequestAsyncOperation asyncOperation,
                                                                                        IProgressTracker< float > progressTracker = default,
                                                                                        CancellationToken cancellationToken = default )
    {
        try
        {
            bool webRequestWasAborted = false;

            while( !asyncOperation.isDone )
            {
                if( !webRequestWasAborted )
                {
                    if( cancellationToken.IsCancellationRequested )
                    {
                        asyncOperation.webRequest.Abort();
                        webRequestWasAborted = true;
                    }

                    progressTracker?.ReportProgress( asyncOperation.progress );
                }

                await Task.Yield();
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
        catch( OperationCanceledException )
        {
            asyncOperation.webRequest.Dispose();
            throw;
        }
        finally
        {
            progressTracker?.ReportProgress( 1.0f );
        }
        
        return asyncOperation;
    }
}