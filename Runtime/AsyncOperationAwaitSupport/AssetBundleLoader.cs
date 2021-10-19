using System.Threading;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace CrazyPanda.UnityCore.PandaTasks
{
    public static class AssetBundleLoader 
    {
        public static IPandaTask< UnityWebRequestAsyncOperation > PartlyLoadAssetBundle( string uri )
        {
            uri.ThrowArgumentNullExceptionIfNull( nameof(uri) );
            return PartlyLoadAssetBundleInternal( uri );
        }
        
        public static IPandaTask< UnityWebRequestAsyncOperation > PartlyLoadAssetBundle( string uri, IProgressTracker<float> progressTracker )
        {
            uri.ThrowArgumentNullExceptionIfNull( nameof(uri) );
            progressTracker.ThrowArgumentNullExceptionIfNull( nameof(progressTracker) );
            return PartlyLoadAssetBundleInternal( uri, progressTracker, CancellationToken.None );
        }
        
        public static IPandaTask< UnityWebRequestAsyncOperation > PartlyLoadAssetBundle( string uri, CancellationToken cancellationToken )
        {
            uri.ThrowArgumentNullExceptionIfNull( nameof(uri) );
            return PartlyLoadAssetBundleInternal( uri, null, cancellationToken );
        }

        public static IPandaTask< UnityWebRequestAsyncOperation > PartlyLoadAssetBundle( string uri, IProgressTracker<float> progressTracker,
                                                                                         CancellationToken cancellationToken )
        {
            uri.ThrowArgumentNullExceptionIfNull( nameof(uri) );
            progressTracker.ThrowArgumentNullExceptionIfNull( nameof(progressTracker) );
            return PartlyLoadAssetBundleInternal( uri, progressTracker, cancellationToken );
        }

        public static IPandaTask< AssetBundle > FullLoadAssetBundleAsync( string uri )
        {
            uri.ThrowArgumentNullExceptionIfNull( nameof(uri) );
            return FullLoadAssetBundleInternal( uri, null, CancellationToken.None );
        }

        public static IPandaTask< AssetBundle > FullLoadAssetBundleAsync( string uri, IProgressTracker<float> progressTracker )
        {
            uri.ThrowArgumentNullExceptionIfNull( nameof(uri) );
            progressTracker.ThrowArgumentNullExceptionIfNull( nameof(progressTracker) );
            return FullLoadAssetBundleInternal( uri, progressTracker, CancellationToken.None );
        }
        
        public static IPandaTask< AssetBundle > FullLoadAssetBundleAsync( string uri, CancellationToken cancellationToken )
        {
            uri.ThrowArgumentNullExceptionIfNull( nameof(uri) );
            return FullLoadAssetBundleInternal( uri, null, cancellationToken );
        }

        public static IPandaTask< AssetBundle > FullLoadAssetBundleAsync( string uri, IProgressTracker<float> progressTracker, CancellationToken cancellationToken )
        {
            uri.ThrowArgumentNullExceptionIfNull( nameof(uri) );
            progressTracker.ThrowArgumentNullExceptionIfNull( nameof(progressTracker) );
            return FullLoadAssetBundleInternal( uri, progressTracker, cancellationToken );
        }
        
        private static async IPandaTask< AssetBundle > FullLoadAssetBundleInternal( string uri, IProgressTracker< float > progressTracker = default,
                                                                       CancellationToken cancellationToken = default )
        {
            using( var webRequest = UnityWebRequestAssetBundle.GetAssetBundle( uri ) )
            {
                await webRequest.SendWebRequest().WaitAsync( progressTracker, cancellationToken );
                return DownloadHandlerAssetBundle.GetContent( webRequest );
            }
        }
        
        private static IPandaTask< UnityWebRequestAsyncOperation > PartlyLoadAssetBundleInternal( string uri, IProgressTracker< float > progressTracker = default,
                                                                                    CancellationToken cancellationToken = default )
        {
            return UnityWebRequestAssetBundle.GetAssetBundle( uri ).SendWebRequest().WaitAsync( progressTracker, cancellationToken );
        }
    }    
}
