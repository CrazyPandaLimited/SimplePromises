using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
//using StandardAssets.Editor;


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
        
        private static object[][] _multipleDoneTestCaseSource = new object[][]
        {
            new object[] { 1 },
            new object[] { 3 },
        };
        
        [ TestCaseSource( nameof(_multipleDoneTestCaseSource) ) ]
		public void MultipleDoneTest( int callbackCount )
		{
			//arrange
			bool[ ] done = new bool[ callbackCount ];
			for( int i = 0; i < done.Length; i++ )
			{
				int index = i;
				_task.Done( () => done[ index ] = true );
			}
		
			//act
			_task.Resolve();
		
			//assert
			Assert.True( done.All( x => x ) );
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
			Assert.AreEqual( testError, _task.Error );
			Assert.AreEqual( PandaTaskStatus.Rejected, _task.Status );

			Assert.False( _taskSuccess );
			Assert.AreEqual( testError, _realException );
		}

		[ Test ]
		public void DefaultRejectTest()
		{
			//act
			_task.Reject();

			//assert
			Assert.AreEqual( PandaTaskStatus.Rejected, _task.Status );

			Assert.AreEqual( _task.Error, _realException );
			Assert.IsInstanceOf< TaskRejectedException >( _task.Error );
		}

        [ Test ]
        public void ThrowIfErrorFailTest()
        {
            //arrange
            var testError = new Exception();
            _task.Reject( testError );

            //act
            Exception realError = null;
            try
            {
                _task.ThrowIfError();
            }
            catch( Exception e )
            {
                realError = e;
            }

            //assert
            Assert.AreEqual( testError, realError );
        }

        [ Test ]
        public void ThrowIfErrorFailPendingTest()
        {
            //act-assert
            Assert.DoesNotThrow( _task.ThrowIfError );
        }

        
        [ Test ]
        public void ThrowIfErrorFailResolveTest()
        {
            //arrange
            _task.Resolve();

            //act-assert
            Assert.DoesNotThrow( _task.ThrowIfError );
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
			Exception secondException = null;
			_task.Fail( x => secondException = x );

			//assert
			Assert.False( _taskSuccess );

			Assert.Null( _realException );
			Assert.AreEqual( realException, secondException );
		}
        
		[ Test ]
		public void ThenWithTaskTest()
		{
			//arrange
			Func< IPandaTask > expectedCallback = () => null;
		
			//act
			IPandaTask newtask = _task.Then( expectedCallback );
			var castedTask = newtask as ContinuationTaskFromPandaTask;
			var realCallback = RuntimeReflection.GetValue< Func< IPandaTask >, ContinuationTaskFromPandaTask >( @"_nextActionDelegate", castedTask );
		
			//assert
			Assert.NotNull( castedTask );
			Assert.False( castedTask.FromCatch );
		
			Assert.AreEqual( expectedCallback, realCallback );
		}

		[ Test ]
		public void ThenTest()
		{
			//arrange
			bool thenFisrst = false;
			IPandaTask newtask = _task.Then( () => thenFisrst = !thenFisrst ).Done( () => thenFisrst = true );

			//act
			_task.Resolve();

			//assert
			Assert.IsTrue( thenFisrst );
			Assert.AreEqual( _task.Status, newtask.Status );
		}

		[ Test ]
		public void CatchTest()
		{
			//arrange
			bool catchFirst = false;
			Exception catchError = null;
			Exception doneError = null;

			IPandaTask newtask = _task.Catch( ex =>
			{
				catchError = ex;
				catchFirst = !catchFirst;
			} ).Fail( ex =>
			{
				doneError = ex;
				catchFirst = true;
			} );

			//act
			var testException = new Exception();
			_task.Reject( testException );

			//assert
			Assert.True(catchFirst);

			Assert.AreEqual( testException, doneError );
			Assert.AreEqual( testException, catchError );
			Assert.AreEqual( testException, newtask.Error );
		}

        
		[ Test ]
		public void CatchWithTaskTest()
		{
			//arrange
			Exception realEception = null;
			IPandaTask nextTask = new PandaTask();
		
			IPandaTask NextTaskCallback( Exception ex )
			{
				realEception = ex;
				return nextTask;
			}
		
			Exception excpectException = new Exception();
			_task.Reject( excpectException );
		
			//act
			IPandaTask rejectTask = _task.Catch( NextTaskCallback );
			var castedTask = rejectTask as ContinuationTaskFromPandaTask;
		
			var callback = RuntimeReflection.GetValue< Func< IPandaTask >, ContinuationTaskFromPandaTask >( @"_nextActionDelegate", castedTask );
			IPandaTask realTask = callback();
		
			//assert
			Assert.NotNull( castedTask );
			Assert.True( castedTask.FromCatch );
		
			Assert.AreEqual( realTask, nextTask );
			Assert.AreEqual( excpectException, realEception );
		}

		[ Test ]
		public void DisposeTest()
		{
			//act
			_task.Dispose();

			//assert
			Assert.AreEqual( PandaTaskStatus.Rejected, _task.Status );
			Assert.IsInstanceOf< ObjectDisposedException >( _task.Error );
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
        public void ThenResultResolveTest()
        {
            //arrange
            var firstTask = new PandaTask();
            var nextTask = new PandaTask< int >();
           
            IPandaTask< int > resultTask = firstTask.Then( () => nextTask );

            //act
            firstTask.Resolve();
            const int testValue = 1;
            nextTask.SetValue( testValue );

            //assert
            Assert.AreEqual( PandaTaskStatus.Resolved, resultTask.Status );
            Assert.AreEqual( testValue, resultTask.Result );
        }

        [ Test ]
        public void ThenResultRejectFirstTest()
        {
            //arrange
            var firstTask = new PandaTask();
            var nextTask = new PandaTask< int >();
           
            IPandaTask< int > resultTask = firstTask.Then( () => nextTask );

            //act
            var testError = new Exception();
            firstTask.Reject(testError);

            //assert
            Assert.AreEqual( PandaTaskStatus.Rejected, resultTask.Status );
            Assert.AreEqual( testError, resultTask.Error );
        }

        [ Test ]
        public void ThenResultRejectSecondTest()
        {
            //arrange
            var firstTask = new PandaTask();
            var nextTask = new PandaTask< int >();
           
            IPandaTask< int > resultTask = firstTask.Then( () => nextTask );

            //act
            firstTask.Resolve();
            var testError = new Exception();
            nextTask.Reject( testError );

            //assert
            Assert.AreEqual( PandaTaskStatus.Rejected, resultTask.Status );
            Assert.AreEqual( testError, resultTask.Error );
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
