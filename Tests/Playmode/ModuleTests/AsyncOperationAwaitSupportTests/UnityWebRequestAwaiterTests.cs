using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using NUnit.Framework;
using UnityEngine.Networking;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    public sealed class UnityWebRequestAwaiterTests
    {
        private UnityWebRequestAsyncOperation _asyncOperation;

        [ SetUp ]
        public void Initialize()
        {
            _asyncOperation = UnityWebRequest.Get("http://yandex.ru").SendWebRequest();
        }
        
        [ AsyncTest ]
        public async Task AsyncOperationAwait_Should_Succeed()
        {
            var result = await _asyncOperation;
            Assert.IsNotNull( result );
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