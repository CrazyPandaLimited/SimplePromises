using System;
using System.Threading;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using NUnit.Framework;
using UnityEngine;
using Assert = CrazyPanda.UnityCore.PandaTasks.Asserts.Assert;
using static CrazyPanda.UnityCore.PandaTasks.AssetBundleLoader;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
#if !UNITY_EDITOR    
    [Ignore("")]
#endif
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    public sealed class AssetBundleDownloadingTests
    {
        private const string TestUri = "https://crazypandalimited.github.io/assets-system/files-for-testing/bundles/blue.bundle3d";

        [ SetUp ]
        public void Initialize()
        {
            AssetBundle.UnloadAllAssetBundles( true );
        }

        [ TearDown ]
        public void Shutdown()
        {
            AssetBundle.UnloadAllAssetBundles( true );
        }

        [ AsyncTest ]
        public async Task FullLoadAssetBundleAsync_Should_Succeed()
        {
            Assert.IsNotNull( await FullLoadAssetBundleAsync( TestUri ) );
        }

        [ AsyncTest ]
        public async Task FullLoadAssetBundleAsync_WithCancellation_Token_Should_ThrowOperationCancelledException()
        {
            await LoadAssetBundleAsync_WithCancellation_Token_Should_ThrowOperationCancelledException( token => FullLoadAssetBundleAsync( TestUri, token ) );
        }

        [ AsyncTest ]
        public async Task FullLoadAssetBundleAsync_WithProgressTracker_Should_Succeed()
        {
            await LoadAssetBundleAsync_WithProgressTracker_Should_Succeed( tracker => FullLoadAssetBundleAsync( TestUri, tracker ) );
        }

        [ AsyncTest ]
        public async Task FullLoadAssetBundleAsync_WithProgressTrackerAndCancellationToken_Should_ThrowOperationCancelledException()
        {
            await LoadAssetBundleAsync_WithProgressTrackerAndCancellationToken_Should_ThrowOperationCancelledException( ( tracker, token ) => FullLoadAssetBundleAsync( TestUri, tracker, token ) );
        }

        [ AsyncTest ]
        public async Task PartlyLoadAssetBundleAsync_Should_Succeed()
        {
            Assert.IsNotNull( await PartlyLoadAssetBundle( TestUri ) );
        }

        [ AsyncTest ]
        public async Task PartlyLoadAssetBundleAsync_WithCancellation_Token_Should_ThrowOperationCancelledException()
        {
            await LoadAssetBundleAsync_WithCancellation_Token_Should_ThrowOperationCancelledException( token => PartlyLoadAssetBundle( TestUri, token ) );
        }

        [ AsyncTest ]
        public async Task PartlyLoadAssetBundleAsync_WithProgressTracker_Should_Succeed()
        {
            await LoadAssetBundleAsync_WithProgressTracker_Should_Succeed( tracker => PartlyLoadAssetBundle( TestUri, tracker ) );
        }

        [ AsyncTest ]
        public async Task PartlyLoadAssetBundleAsync_WithProgressTrackerAndCancellationToken_Should_ThrowOperationCancelledException()
        {
            await LoadAssetBundleAsync_WithProgressTrackerAndCancellationToken_Should_ThrowOperationCancelledException( ( tracker, token ) => PartlyLoadAssetBundle( TestUri, tracker, token ) );
        }

        private async IPandaTask LoadAssetBundleAsync_WithCancellation_Token_Should_ThrowOperationCancelledException< T >( Func< CancellationToken, IPandaTask< T > > taskToWait )
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            cancellationToken.Cancel();
            await Assert.ThrowsAsync< OperationCanceledException >( async () => await taskToWait.Invoke( cancellationToken.Token ) );
        }

        private async IPandaTask LoadAssetBundleAsync_WithProgressTracker_Should_Succeed< T >( Func< IProgressTracker< float >, IPandaTask< T > > taskToWait )
        {
            var progressTracker = new ProgressTracker< float >();
            await taskToWait.Invoke( progressTracker );
            Assert.That( progressTracker.Progress, Is.EqualTo( 1 ) );
        }

        private async IPandaTask LoadAssetBundleAsync_WithProgressTrackerAndCancellationToken_Should_ThrowOperationCancelledException< T >( 
            Func< IProgressTracker< float >, CancellationToken, IPandaTask< T > > taskToWait )
        {
            var progressTracker = new ProgressTracker< float >();
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            cancellationToken.Cancel();

            await Assert.ThrowsAsync< OperationCanceledException >( async () => await taskToWait.Invoke( progressTracker, cancellationToken.Token ) );
            Assert.That( progressTracker.Progress, Is.EqualTo( 1 ) );
        }
    }
}