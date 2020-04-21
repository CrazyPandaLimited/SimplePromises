using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
	public sealed class PandaTasksUtilitysTests
	{
		[ Test ]
		public void NullSequenceTest()
		{
			//act-assert
			Assert.Throws< ArgumentException >( () => PandaTasksUtilitys.Sequence( new PandaTask(), new PandaTask(), null ) );
		}

		[Test]
		public void ResolveSequnceTest( )
        {
            int count = 4;
			//arrange
			List< PandaTask > tasksList = new List< PandaTask >( count );
			for( int i = 0; i < count; i++ )
			{
				tasksList.Add( new PandaTask() );
			}
		
			//act
			IPandaTask task = PandaTasksUtilitys.Sequence( tasksList );
			tasksList.ForEach( x => x.Resolve() );
		
			//assert
			Assert.AreEqual( PandaTaskStatus.Resolved, task.Status );
		}
		
        private static object[][] _noResolveSequnceTestCaseSource = new object[][]
        {
            new object[] { 0, 4 },
            new object[] { 2, 4 },
            new object[] { 3, 4 },
        };
        
        [ TestCaseSource( nameof(_noResolveSequnceTestCaseSource) ) ]
		public void NoResolveSequnceTest( int completedCount, int count )
		{
			//arrange
			List< PandaTask > tasksList = new List< PandaTask >( count );
			for( int i = 0; i < count; i++ )
			{
				tasksList.Add( new PandaTask() );
			}
		
			IPandaTask task = PandaTasksUtilitys.Sequence( tasksList );
		
			//act
			for( int i = 0; i < completedCount; i++ )
			{
				tasksList[i].Resolve();
			}
		
			//assert
			Assert.AreEqual( PandaTaskStatus.Pending, task.Status );
		}
		
		
        private static object[][] _rejectSequnceTaskTestCaseSource = new object[][]
        {
            new object[] { 0, 4 },
            new object[] { 3, 4 },
        };
        
        [ TestCaseSource( nameof(_rejectSequnceTaskTestCaseSource) ) ]
		public void RejectSequnceTaskTest( int rejectIndex, int count )
		{
			//arrange
			List< PandaTask > tasksList = new List< PandaTask >( count );
			for( int i = 0; i < count; i++ )
			{
				tasksList.Add( new PandaTask() );
			}
		
			IPandaTask task = PandaTasksUtilitys.Sequence( tasksList );
		
			//act
			Exception exception = new Exception();
			tasksList[ rejectIndex ].Reject( exception );
			for( int i = 0; i < rejectIndex; i++ )
			{
				tasksList[ i ].Resolve();
			}
		
			//assert
			Assert.AreEqual( exception, task.Error );
		}

        [ Test ]
        public void WaitWhileWhenTrue()
        {
            // arrange
            bool a = true;
            var task = PandaTasksUtilitys.WaitWhile( () => a == false );

            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }

        [ Test ]
        public void WaitWhileWhenFalse()
        {
            // arrange
            bool a = false;
            var task = PandaTasksUtilitys.WaitWhile( () => a == false );

            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Pending ) );
        }

        [ AsyncTest ]
        public async Task WaitWhileCompleteAfterChange()
        {
            // arrange
            bool a = false;
            var tokenSource = new CancellationTokenSource();
            var task = PandaTasksUtilitys.WaitWhile( () => a == false, tokenSource.Token );

            // act            
            a = true;
            
            await PandaTasksUtilitys.Delay( 1 );
            
            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }

        [ AsyncTest ]
        public async Task WaitWhileCancel()
        {
            // arrange
            bool a = false;
            var tokenSource = new CancellationTokenSource();
            var task = PandaTasksUtilitys.WaitWhile( () => a == false, tokenSource.Token );

            // act            
            a = true;
            tokenSource.Cancel();
            
            await PandaTasksUtilitys.Delay( 1 );

            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( task.Error, Is.InstanceOf< TaskRejectedException >() );
        }

        [ AsyncTest ]
        public async Task WaitWhileRaiseException()
        {
            // arrange
            var task = PandaTasksUtilitys.WaitWhile( () => throw new InvalidOperationException() );

            // act
            
            await PandaTasksUtilitys.Delay( 1 );

            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( task.Error, Is.InstanceOf< InvalidOperationException >() );
        }
    }
}
