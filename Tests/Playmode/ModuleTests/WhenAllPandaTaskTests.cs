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

        private static object[][] _waitAllResolveTestCaseSource = new object[][]
        {
            new object[] { 1 },
            new object[] { 3 },
        };
        
        [ TestCaseSource( nameof(_waitAllResolveTestCaseSource) ) ]
        public void WaitAllResolveTest( int count )
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

        private static object[][] _waitAllSomeResolveTestCaseSource = new object[][]
        {
            new object[] { 1,0 },
            new object[] { 3,2 },
        };
        
        [ TestCaseSource( nameof(_waitAllSomeResolveTestCaseSource) ) ]
		public void WaitAllSomeResolveTest( int count, int resolveCount )
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

        
        private static object[][] _rejectSomeTestCaseSource = new object[][]
        {
            new object[] { 4,3 },
            new object[] { 4,1 },
        };
        
        [ TestCaseSource( nameof(_rejectSomeTestCaseSource) ) ]
		public void RejectSomeTest( int count, int rejectCount )
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

        private static object[][] _halfRejectTestCaseSource = new object[][]
        {
            new object[] { 2,1 },
            new object[] { 4,2 },
        };
        
        [ TestCaseSource( nameof(_halfRejectTestCaseSource) ) ]
        public void HalfRejectTest( int count, int rejectCount )
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

        private static object[][] _initFirstResolvedTestCaseSource = new object[][]
        {
            new object[] { 2 },
            new object[] { 5 },
        };
        
        [ TestCaseSource( nameof(_initFirstResolvedTestCaseSource) ) ]
		public void InitFirstResolvedTest( int count )
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

        private static object[][] _initResolvedTestCaseSource = new object[][]
        {
            new object[] { 1 },
            new object[] { 5 },
        };
        
        [ TestCaseSource( nameof(_initResolvedTestCaseSource) ) ]
		public void InitResolvedTest( int count )
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
