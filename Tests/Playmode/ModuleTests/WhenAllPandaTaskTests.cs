using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
	public sealed class WhenAllPandaTaskTests
	{
		[ Test ]
		public void ResolveTest()
		{
			//arrange
			var task = new WhenAllPandaTask( new[ ] { new PandaTask() } );

			//act-assert
			Assert.Throws< InvalidOperationException >( task.Resolve );
		}

		[ Test ]
		public void RejectTest()
		{
			//arrange
			var task = new WhenAllPandaTask( new[ ] { new PandaTask() } );

			//act-assert
			Assert.Throws< InvalidOperationException >( task.Resolve );
		}

        
        [ Test ]
        public void WaitAllResolveTest_1() => WaitAllResolveTest( 1 );
        [ Test ]
        public void WaitAllResolveTest_3() => WaitAllResolveTest( 3 );
        
        private void WaitAllResolveTest( int count )
		{
			//arrange
			var tasksCollection = new List< PandaTask >( count );
			for( int i = 0; i < count; i++ )
			{
				tasksCollection.Add( new PandaTask() );
			}

			var task = new WhenAllPandaTask( tasksCollection );

			//act
			tasksCollection.ForEach( x => x.Resolve() );

			//assert
			Assert.Null( task.Error );
			Assert.AreEqual( PandaTaskStatus.Resolved, task.Status );
		}
        
        [ Test ]
        public void WaitAllSomeResolveTest_1_0() => WaitAllSomeResolveTest( 1, 0 );
        [ Test ]
        public void WaitAllSomeResolveTest_3_2() => WaitAllSomeResolveTest( 3, 2 );
        
		private void WaitAllSomeResolveTest( int count, int resolveCount )
		{
			//arrange
			var tasksCollection = new List< PandaTask >( count );
			for( int i = 0; i < count; i++ )
			{
				tasksCollection.Add( new PandaTask() );
			}

			var task = new WhenAllPandaTask( tasksCollection );

			//act
			for( int i = 0; i < resolveCount; i++ )
			{
				tasksCollection[ i ].Resolve();
			}

			//assert
			Assert.Null( task.Error );
			Assert.AreEqual( PandaTaskStatus.Pending, task.Status );
		}

		[ Test ]
		public void ResolveAfterDisposeTest()
		{
			//arrange
			var innerTask = new PandaTask();
			var task = new WhenAllPandaTask( new[ ] { innerTask } );

			task.Dispose();

			//act
			innerTask.Resolve();

			//assert		
			Assert.AreEqual( PandaTaskStatus.Rejected, task.Status );
			Assert.IsInstanceOf< ObjectDisposedException >( task.Error.GetBaseException() );
		}

        [ Test ]
        public void RejectSomeTest_4_3() => RejectSomeTest(4,3);
        
        [ Test ]
        public void RejectSomeTest_4_1() => RejectSomeTest(4,1);
        
		private void RejectSomeTest( int count, int rejectCount )
		{
			//arrange
			var tasksCollection = new List< PandaTask >( count );
			for( int i = 0; i < count; i++ )
			{
				tasksCollection.Add( new PandaTask() );
			}

			var task = new WhenAllPandaTask( tasksCollection );

			//act
			for( int i = 0; i < rejectCount; i++ )
			{
				tasksCollection[ i ].Reject();
			}

			//assert
			Assert.Null( task.Error );
			Assert.AreEqual( PandaTaskStatus.Pending, task.Status );
		}

        [ Test ]
        public void RejectAllTest()
        {
            var count = 3;
            //arrange
            var tasksCollection = new List< PandaTask >( count );
            for( int i = 0; i < count; i++ )
            {
                tasksCollection.Add( new PandaTask() );
            }

            var task = new WhenAllPandaTask( tasksCollection );

            //act
            tasksCollection.ForEach( x => x.Reject( new Exception() ) );

            //assert
            Assert.AreEqual( PandaTaskStatus.Rejected, task.Status );
            Assert.IsInstanceOf< AggregateException >( task.Error );

            ReadOnlyCollection< Exception > realExceptions = ( ( AggregateException ) task.Error ).Flatten().InnerExceptions;
            CollectionAssert.AreEquivalent( tasksCollection.Select( x => x.Error ), realExceptions );
        }

        [ Test ]
        public void HalfRejectTest_2_1() => HalfRejectTest( 2, 1 );

        [ Test ]
        public void HalfRejectTest_4_2() => HalfRejectTest( 4, 2 );
        
        private void HalfRejectTest( int count, int rejectCount )
        {
            //arrange
            var tasksCollection = new List< PandaTask >( count );
            for( int i = 0; i < count; i++ )
            {
                tasksCollection.Add( new PandaTask() );
            }

            var task = new WhenAllPandaTask( tasksCollection );

            //act

            for( int i = 0; i < count; i++ )
            {
                if( i < rejectCount )
                {
                    tasksCollection[ i ].Reject( new Exception() );
                }
                else
                {
                    tasksCollection[ i ].Resolve();
                }
            }

            //assert
            Assert.AreEqual( PandaTaskStatus.Rejected, task.Status );

            ReadOnlyCollection< Exception > realExceptions = ( ( AggregateException ) task.Error ).Flatten().InnerExceptions;
            CollectionAssert.AreEquivalent( tasksCollection.Where( x => x.Status == PandaTaskStatus.Rejected ).Select( x => x.Error ), realExceptions );
        }

        [ Test ]
        public void InitFirstResolvedTest_2() => InitFirstResolvedTest( 2 );
        
        [ Test ]
        public void InitFirstResolvedTest_5() => InitFirstResolvedTest( 5 );
        
		private void InitFirstResolvedTest( int count )
		{
			//arrange
			var tasksCollection = new List< PandaTask >( count );
			for( int i = 0; i < count; i++ )
			{
				tasksCollection.Add( new PandaTask() );
			}

			tasksCollection[ 0 ].Resolve();

			//act
			var task = new WhenAllPandaTask( tasksCollection );

			//assert
			Assert.AreEqual( PandaTaskStatus.Pending, task.Status );
		}

        [ Test ]
        public void InitResolvedTest_1() => InitResolvedTest( 1 );
        
        [ Test ]
        public void InitResolvedTest_5() => InitResolvedTest( 5 );
        
		private void InitResolvedTest( int count )
		{
			//arrange
			var tasksCollection = new List< IPandaTask >( count );
			for( int i = 0; i < count; i++ )
			{
				tasksCollection.Add( PandaTasksUtilitys.CompletedTask );
			}

			//act
			var task = new WhenAllPandaTask( tasksCollection );

			//assert
			Assert.AreEqual( PandaTaskStatus.Resolved, task.Status );
		}

		[ Test ]
		public void ZeroTasksTest()
		{
			//act
			var task = new WhenAllPandaTask( Enumerable.Empty< IPandaTask >() );

			//assert
			Assert.AreEqual( PandaTaskStatus.Resolved, task.Status );
		}
	}
}
