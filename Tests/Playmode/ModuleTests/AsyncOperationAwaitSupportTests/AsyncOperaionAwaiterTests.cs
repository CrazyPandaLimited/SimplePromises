using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using UnityEngine;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    public sealed class AsyncOperationAwaiterTests
    {
        private AsyncOperation _asyncOperation;

		[ SetUp ]
        public void Initialize()
        {
            AssetBundle.UnloadAllAssetBundles( true );
            _asyncOperation = AssetBundle.LoadFromFileAsync( "Assets/UnityCoreSystems/Systems/Tests/Promises/Playmode/ModuleTests/AsyncOperationAwaitSupportTests/Resources/test.bundle" );
        }
        
        [ AsyncTest ]
        public async Task AsyncOperationAwait_Should_Succeed()
        {
            await _asyncOperation;
            Assert.IsTrue( _asyncOperation.isDone );
        }

        [ AsyncTest ]
        public async Task AsyncOperationAwait_WithProgressTracker_Should_Succeed()
        {
            var progressTracker = new ProgressTracker< float >();
            await _asyncOperation.WithProgressTracker( progressTracker );            
            Assert.IsTrue( _asyncOperation.isDone );
            Assert.AreEqual( progressTracker.Progress, 1 );
        } 
    }
}
