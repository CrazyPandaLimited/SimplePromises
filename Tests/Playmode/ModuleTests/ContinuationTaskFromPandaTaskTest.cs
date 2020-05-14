using System;
using NUnit.Framework;


namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    public sealed class ContinuationTaskFromPandaTaskTests
	{
		private ContinuationTaskFromPandaTask _testTask;

		private IPandaTask _innerTaskMock;

		[ Test ]
		public void DisposeTest()
		{
			//arrange
			ContinuationTaskFromPandaTask testTask = ConstructTask();

			//act
			testTask.Dispose();

			//assert		
			Assert.AreEqual( testTask.Status, PandaTaskStatus.Rejected );
			Assert.IsInstanceOf< ObjectDisposedException >( testTask.Error );
		}

		[ Test ]
		public void ResolveTest()
		{
			//arrange
			ContinuationTaskFromPandaTask testTask = ConstructTask();

			bool doneCall = false;
			bool catchCall = false;
			testTask.Done( () => doneCall = true ).Fail( ex => catchCall = true );

			//act-assert
			Assert.False( doneCall );
			Assert.False( catchCall );

			Assert.Throws< InvalidOperationException >( testTask.Resolve );
		}

		[ Test ]
		public void ResolveFirstTest()
		{
			//arrange
			Tuple< ContinuationTaskFromPandaTask, PandaTask > taskPair = ConstructTask( false, () => new PandaTask() );

			bool doneCall = false;
			bool catchCall = false;
			taskPair.Item1.Done( () => doneCall = true ).Fail( ex => catchCall = true );

			//act
			taskPair.Item2.Resolve();

			//assert
			Assert.False( doneCall );
			Assert.False( catchCall );

			Assert.Null( taskPair.Item1.Error );
			Assert.AreEqual( taskPair.Item1.Status, PandaTaskStatus.Pending );
		}

		[ Test ]
		public void ResolveSecondTest()
		{
			//arrange
			PandaTask nextTask = new PandaTask();
			Tuple< ContinuationTaskFromPandaTask, PandaTask > taskPair = ConstructTask( false, () => nextTask );

			bool doneCall = false;
			bool catchCall = false;
			taskPair.Item1.Done( () => doneCall = true ).Fail( ex => catchCall = true );

			taskPair.Item2.Resolve();

			//act
			nextTask.Resolve();

			//assert
			Assert.True( doneCall );
			Assert.False( catchCall );

			Assert.Null( taskPair.Item1.Error );
			Assert.AreEqual( taskPair.Item1.Status, PandaTaskStatus.Resolved );
		}

		[ Test ]
		public void ResolveFromRejectTest()
		{
			//arrange
			Tuple< ContinuationTaskFromPandaTask, PandaTask > taskPair = ConstructTask( true, () => new PandaTask() );

			bool doneCall = false;
			Exception realException = null;
			taskPair.Item1.Done( () => doneCall = true ).Fail( ex => realException = ex );

			//act
			taskPair.Item2.Resolve();

			//assert
			Assert.True( doneCall );
			Assert.Null( realException );
			Assert.Null( taskPair.Item1.Error );
			Assert.AreEqual( taskPair.Item1.Status, PandaTaskStatus.Resolved );
		}

		[ Test ]
		public void ResolveNullContinuationTest()
		{
			//arrange
			Tuple< ContinuationTaskFromPandaTask, PandaTask > taskPair = ConstructTask( false );

			bool doneCall = false;
			Exception realException = null;
			taskPair.Item1.Done( () => doneCall = true ).Fail( ex => realException = ex );

			//act
			taskPair.Item2.Resolve();

			//assert
			Assert.False( doneCall );
			Assert.AreEqual( realException, taskPair.Item1.Error );

			Assert.IsInstanceOf< NullReferenceException >( taskPair.Item1.Error );
		}

		[ Test ]
		public void ResolveExceptionTest()
		{
			//arrange
			Exception testException = new Exception();
			Tuple< ContinuationTaskFromPandaTask, PandaTask > taskPair = ConstructTask( false, () => throw testException );

			bool doneCall = false;
			Exception realException = null;
			taskPair.Item1.Done( () => doneCall = true ).Fail( ex => realException = ex );

			//act
			taskPair.Item2.Resolve();

			//assert
			Assert.False( doneCall );
			Assert.AreEqual( realException, taskPair.Item1.Error );

			Assert.AreEqual( testException, taskPair.Item1.Error );
		}

		[ Test ]
		public void RejectFirstTest()
		{
			//arrange
			Tuple< ContinuationTaskFromPandaTask, PandaTask > taskPair = ConstructTask( false );

			
			bool doneCall = false;
			Exception gettedException = null;
			taskPair.Item1.Done( () => doneCall = true ).Fail( ex => gettedException = ex );

			//act
			Exception realException = new Exception();
			taskPair.Item2.Reject( realException );

			//assert
			Assert.False( doneCall );
			Assert.AreEqual( realException, gettedException );

			Assert.AreEqual( PandaTaskStatus.Rejected,  taskPair.Item1.Status);
			Assert.AreEqual( realException, taskPair.Item1.Error );
		}

		[ Test ]
		public void RejectFromCatchTest()
		{
			//arrange
			Tuple< ContinuationTaskFromPandaTask, PandaTask > taskPair = ConstructTask( true, () => new PandaTask() );

			//act
			Exception realException = new Exception();
			taskPair.Item2.Reject( realException );

			//assert
			Assert.Null( taskPair.Item1.Error );
			Assert.AreEqual( PandaTaskStatus.Pending, taskPair.Item1.Status );
		}

		[ Test ]
		public void RejectFromCatchReolveTest()
		{
			//arrange
			PandaTask nextTask = new PandaTask();
			Tuple< ContinuationTaskFromPandaTask, PandaTask > taskPair = ConstructTask( true, () => nextTask );

			taskPair.Item2.Reject( );

			//act
			nextTask.Resolve();

			//assert
			Assert.Null( taskPair.Item1.Error );
			Assert.AreEqual( PandaTaskStatus.Resolved, taskPair.Item1.Status );
		}

		private Tuple< ContinuationTaskFromPandaTask, PandaTask > ConstructTask( bool fromCatch = false, Func< IPandaTask > nextTaskCallback = null )
		{
			var firstTask = new PandaTask();
			if( nextTaskCallback == null )
			{
				nextTaskCallback = () => null;
			}

			return new Tuple< ContinuationTaskFromPandaTask, PandaTask >( new ContinuationTaskFromPandaTask( firstTask, nextTaskCallback, fromCatch ), firstTask );
		}

		private ContinuationTaskFromPandaTask ConstructTask()
		{
			return ConstructTask( false ).Item1;
		}

	}
}
