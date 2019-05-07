using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;


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

		[ TestCase( 1 ) ]
		[ TestCase( 3 ) ]
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

		[ TestCase( 1, 0 ) ]
		[ TestCase( 3, 2 ) ]
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
				tasksCollection[i].Resolve();
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

		[ TestCase( 4, 3 ) ]
		[ TestCase( 4, 1 ) ]
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
				tasksCollection[ i ].Reject( );
			}

			//assert
			Assert.Null( task.Error );
			Assert.AreEqual( PandaTaskStatus.Pending, task.Status );
		}

		[ TestCase( 1 ) ]
		[ TestCase( 3 ) ]
		public void RejectAllTest( int count )
		{
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
			CollectionAssert.AreEquivalent( task.Error.Flatten().InnerExceptions, tasksCollection.Select( x => x.Error.GetBaseException() ) );
		}

		[ TestCase( 4, 3 ) ]
		[ TestCase( 4, 1 ) ]
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
			CollectionAssert.AreEquivalent( task.Error.Flatten().InnerExceptions, tasksCollection.Where( x => x.Status == PandaTaskStatus.Rejected ).Select( x => x.Error.GetBaseException() ) );
		}
	}
}
