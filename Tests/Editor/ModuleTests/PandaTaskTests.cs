using System;
using NUnit.Framework;
using StandardAssets.Editor;


namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
	public sealed class PandaTaskTests
	{
		private PandaTask _task;

		private bool _taskSuccess;
		private Exception _realException;

		[ SetUp ]
		public void Init()
		{
			//create task
			_task = new PandaTask();

			//exception handling
			_taskSuccess = false;
			_realException = null;
			_task.Fail(  x => _realException = _realException == null ? x : null ).Done( () => _taskSuccess = true );
		}

		[ Test ]
		public void ResolveTest()
		{
			//act
			_task.Resolve();

			//assert
			Assert.True( _taskSuccess );
			Assert.Null( _realException );
			Assert.AreEqual( PandaTaskStatus.Resolved, _task.Status );
		}

		[ Test ]
		public void DoubleResolveTest()
		{
			//arrange
			var task = new PandaTask();
			task.Resolve();

			//act-assert
			Assert.Throws< InvalidOperationException >( task.Resolve );
		}

		[ Test ]
		public void DoneAfterResolveTest()
		{
			//arrange
			_task.Resolve();
			_taskSuccess = false;

			//act
			bool successSecond = false;
			void SuccessActionSecond()
			{
				successSecond = true;
			}

			_task.Done( SuccessActionSecond );

			Assert.Null( _realException );
			Assert.IsTrue( successSecond );
			Assert.IsFalse( _taskSuccess );
		}

		[ Test ]
		public void RejectTest()
		{
			//act
			Exception testError = new Exception();
			_task.Reject( testError );

			//assert
			Assert.AreEqual( testError, _task.Error.GetBaseException() );
			Assert.AreEqual( PandaTaskStatus.Rejected, _task.Status );

			Assert.False( _taskSuccess );
			Assert.AreEqual( testError, _realException.GetBaseException() );
		}

		[ Test ]
		public void DefaultRejectTest()
		{
			//act
			_task.Reject();

			//assert
			Assert.AreEqual( PandaTaskStatus.Rejected, _task.Status );

			Assert.AreEqual( _task.Error, _realException );
			Assert.IsInstanceOf< TaskRejectedException >( _task.Error.GetBaseException() );
		}

		[ Test ]
		public void DoubleRejectTest()
		{
			//arrange
			_task.Reject();

			//act-assert
			Assert.Throws< InvalidOperationException >( _task.Reject );
		}

		[ Test ]
		public void RejectAfterResolveTest()
		{
			//arrange
			_task.Resolve();

			//act-assert
			Assert.Throws< InvalidOperationException >( _task.Reject );
		}

		[ Test ]
		public void FailAfterResolveTest()
		{
			//arrange
			Exception realException = new Exception();
			_task.Reject( realException );
			_realException = null;

			//act
			AggregateException secondException = null;
			_task.Fail( x => secondException = x );

			//assert
			Assert.False( _taskSuccess );

			Assert.Null( _realException );
			Assert.AreEqual( realException, secondException.GetBaseException() );
		}

		[ Test ]
		public void ThenWithTaskTest()
		{
			//arrange
			Func< IPandaTask > expectedCallback = () => null;
		
			//act
			IPandaTask newtask = _task.Then( expectedCallback );
			var castedTask = newtask as ContinuationTaskFromPandaTask;
			var realCallback = EditorReflection.GetValue< Func< IPandaTask >, ContinuationTaskFromPandaTask >( @"_nextActionDelegate", castedTask );

			//assert
			Assert.NotNull( castedTask );
			Assert.False( castedTask.fromCatch );

			Assert.AreEqual( expectedCallback, realCallback );
		}

		[ Test ]
		public void CatchWithTaskTest()
		{
			//arrange
			AggregateException realEception = null;
			IPandaTask nextTask = new PandaTask();

			IPandaTask NextTaskCallback( AggregateException ex )
			{
				realEception = ex;
				return nextTask;
			}

			Exception excpectException = new Exception();
			_task.Reject( excpectException );

			//act
			IPandaTask rejectTask = _task.Catch( NextTaskCallback );
			var castedTask = rejectTask as ContinuationTaskFromPandaTask;

			var callback = EditorReflection.GetValue< Func< IPandaTask >, ContinuationTaskFromPandaTask >( @"_nextActionDelegate", castedTask );
			IPandaTask realTask = callback();

			//assert
			Assert.NotNull( castedTask );
			Assert.True( castedTask.fromCatch );

			Assert.AreEqual( realTask, nextTask );
			Assert.AreEqual( excpectException, realEception.GetBaseException() );
		}

		[ Test ]
		public void DisposeTest()
		{
			//act
			_task.Dispose();

			//assert
			Assert.AreEqual( PandaTaskStatus.Rejected, _task.Status );
			Assert.IsInstanceOf< ObjectDisposedException >( _task.Error.GetBaseException() );
		}

		[ Test ]
		public void DoubleDisposeTest()
		{
			//arrange
			_task.Dispose();

			//act
			Assert.DoesNotThrow( _task.Dispose );
		}

		[ Test ]
		public void YieldInstructionTest()
		{
			//act-assert
			Assert.True( _task.keepWaiting );
			_task.Resolve();
			Assert.False( _task.keepWaiting );
		}
	}
}
