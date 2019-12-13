using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    class PandaAsyncAwaitTests
    {
        [ AsyncTest ]
        public async IPandaTask ReturnsFromAwait()
        {
            // arrange
            Func< IPandaTask > func = async () => await PandaTasksUtilitys.CompletedTask;

            // act
            await func();
        }

        [ AsyncTest ]
        public async IPandaTask ReturnsValueFromAwait()
        {
            // arrange
            Func< IPandaTask< int > > func = async () => await PandaTasksUtilitys.GetCompletedTask( 1 );

            // act
            var value = await func();

            // assert
            Assert.That( value, Is.EqualTo( 1 ) );
        }

        [ AsyncTest ]
        public async IPandaTask ThrowsOnAwaitRejected()
        {
            // arrange
            Func< IPandaTask > func = async () => await PandaTasksUtilitys.RejectedTask;
            bool hasException = false;

            // act
            try { await func(); }
            catch { hasException = true; }

            // assert
            Assert.That( hasException, Is.True );
        }

        [ AsyncTest ]
        public async IPandaTask ThrowsOnAwaitRejectedResult()
        {
            // arrange
            Func< IPandaTask< int > > func = async () => await PandaTasksUtilitys.GetRejectedTask<int>();
            bool hasException = false;

            // act
            try { await func(); }
            catch { hasException = true; }

            // assert
            Assert.That( hasException, Is.True );
        }

        [ AsyncTest ]
        public async IPandaTask ThrowsOnAwaitCancelledBeforeAwait()
        {
            // arrange
            bool hasException = false;
            var token = new CancellationToken( true );

            // act
            var task = CancellableBeforeAwait( token );
            try { await task; }
            catch { hasException = true; }

            // assert
            Assert.That( hasException, Is.True );
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
        }
        
        [ AsyncTest ]
        public async IPandaTask ThrowsOnAwaitCancelledAfterAwait()
        {
            // arrange
            bool hasException = false;
            var token = new CancellationToken( true );

            // act
            var task = CancellableAfterAwait( token );
            try { await task; }
            catch { hasException = true; }

            // assert
            Assert.That( hasException, Is.True );
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
        }
        
        [ AsyncTest ]
        public async IPandaTask ThrowsOnAwaitCancelledValueBeforeAwait()
        {
            // arrange
            bool hasException = false;
            var token = new CancellationToken( true );

            // act
            var task = CancellableWithValueBeforeAwait( token );
            try { await task; }
            catch { hasException = true; }

            // assert
            Assert.That( hasException, Is.True );
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
        }

        [ AsyncTest ]
        public async IPandaTask ThrowsOnAwaitCancelledValueAfterAwait()
        {
            // arrange
            bool hasException = false;
            var token = new CancellationToken( true );

            // act
            var task = CancellableWithValueAfterAwait( token ); 
            try { await task; }
            catch { hasException = true; }

            // assert
            Assert.That( hasException, Is.True );
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
        }

        private async IPandaTask CancellableBeforeAwait(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await NonSynchronousTask();
        }

        private async IPandaTask CancellableAfterAwait(CancellationToken token)
        {
            await NonSynchronousTask();
            token.ThrowIfCancellationRequested();
        }

        private async IPandaTask< int > CancellableWithValueBeforeAwait(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await NonSynchronousTask();
            return 1;
        }

        private async IPandaTask< int > CancellableWithValueAfterAwait(CancellationToken token)
        {
            await NonSynchronousTask();
            token.ThrowIfCancellationRequested();
            return 1;
        }

        private IPandaTask NonSynchronousTask()
        {
            return PandaTasksUtilitys.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
}