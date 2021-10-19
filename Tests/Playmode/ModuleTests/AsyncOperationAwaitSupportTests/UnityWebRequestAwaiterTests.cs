using System;
using System.Threading;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using NUnit.Framework;
using UnityEngine.Networking;
using Assert = CrazyPanda.UnityCore.PandaTasks.Asserts.Assert;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    public sealed class UnityWebRequestAwaiterTests
    {
        private UnityWebRequest _request;

        [ SetUp ]
        public void Initialize()
        {
            _request = UnityWebRequest.Get( "http://yandex.ru" );
        }  
        
        [ TearDown ]
        public void Shutdown()
        {
            _request.Dispose();
        }

        [ AsyncTest ]
        public async Task WaitAsync_Should_Succeed()
        {
            var result = await  UnityWebRequest.Get( "http://yandex.ru" );;
            Assert.IsNotNull( result );
            Assert.IsTrue( result.isDone );
        }

        [ AsyncTest ]
        public async Task WaitAsync_WithProgressTracker_Should_Succeed()
        {
            var progressTracker = new ProgressTracker< float >();
            await _request.WaitAsync( progressTracker );
            Assert.IsTrue( _request.isDone );
            Assert.That( progressTracker.Progress, Is.EqualTo( 1 ) );
        }

        [ AsyncTest ]
        public async Task WaitAsync_WithProgressTrackerAndCancellationToken_Should_Throw_OperationCancelledException()
        {
            var progressTracker = new ProgressTracker< float >();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync< OperationCanceledException >( async () => await _request.WaitAsync( progressTracker, cancellationTokenSource.Token ) );

            Assert.That( progressTracker.Progress, Is.EqualTo( 1 ) );
        }

        [ AsyncTest ]
        public async Task WaitAsync_WithCancellation_Token_Should_Throw_OperationCancelledException()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            await Assert.ThrowsAsync< OperationCanceledException >( async () => await _request.WaitAsync( cancellationTokenSource.Token ) );
        }
    }
}