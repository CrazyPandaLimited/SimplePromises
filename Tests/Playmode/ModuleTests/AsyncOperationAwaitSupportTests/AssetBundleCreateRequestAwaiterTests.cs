using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using UnityEngine;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    public sealed class AssetBundleCreateRequestAwaiterTests
    {
        private AssetBundleCreateRequest _asyncOperation;

		[ SetUp ]
        public void Initialize()
        {
            AssetBundle.UnloadAllAssetBundles( true );
            
#if UNITY_EDITOR            
            _asyncOperation = AssetBundle.LoadFromFileAsync( "Assets/UnityCoreSystems/Promises/Tests/Playmode/ModuleTests/AsyncOperationAwaitSupportTests/Bundle/Editor/test.bundle" );
#else            
            _asyncOperation = AssetBundle.LoadFromFileAsync( Path.Combine(Application.streamingAssetsPath,"test.bundle") );
#endif
        }

        [ TearDown ]
        public void ShutDown()
        {
            AssetBundle.UnloadAllAssetBundles( true );
        }
        
        [ AsyncTest ]
        public async Task AsyncOperationAwait_Should_Succeed()
        {
            await _asyncOperation;
            Assert.IsTrue( _asyncOperation.isDone );
            Assert.IsNotNull( _asyncOperation.assetBundle );
        }

        [ AsyncTest ]
        public async Task AsyncOperationAwait_WithProgressTracker_Should_Succeed()
        {
            var progressTracker = new ProgressTracker< float >();
            await _asyncOperation.WaitAsync( progressTracker );            
            Assert.IsTrue( _asyncOperation.isDone );
            Assert.That( progressTracker.Progress, Is.EqualTo( 1 ) );
            Assert.IsNotNull( _asyncOperation.assetBundle );
        } 
    }
}
