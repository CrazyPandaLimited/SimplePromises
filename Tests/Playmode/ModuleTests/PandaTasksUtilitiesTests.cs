using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    [ TestFixture( typeof( float ) ) ]
    [ TestFixture( typeof( object ) ) ]
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    public sealed class PandaTasksUtilitiesTests <T>
    {
        [Test]
        public void GetCanceledTask_Should_Return_Same_Instance() => 
            Assert.That( PandaTasksUtilities.GetCanceledTask<T>(), Is.EqualTo( PandaTasksUtilities.GetCanceledTask< T >() ) );
    }

    [ TestFixture( typeof( TestClass ), typeof( InheritedTestClass ) ) ]
    [ TestFixture( typeof( object ), typeof( TestClass ) ) ]
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    public sealed class PandaTasksUtilitiesTests <T1,T2>
    {
        [ Test ]
        public void GetCanceledTask_Should_Return_Different_Instance() => 
            Assert.That( PandaTasksUtilities.GetCanceledTask< T1 >(), Is.Not.EqualTo( PandaTasksUtilities.GetCanceledTask< T2 >() ) );
    }

    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    public sealed class PandaTasksUtilitiesTests
	{
        [ Test ]
		public void NullSequenceTest()
		{
			//act-assert
			Assert.Throws< ArgumentException >( () => PandaTasksUtilities.Sequence( new PandaTask(), new PandaTask(), null ) );
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
			IPandaTask task = PandaTasksUtilities.Sequence( tasksList );
			tasksList.ForEach( x => x.Resolve() );
		
			//assert
			Assert.AreEqual( PandaTaskStatus.Resolved, task.Status );
		}
		
        [TestCase(0, 4)]
        [TestCase(2, 4 )]
        [TestCase(3, 4 )]
		public void NoResolveSequnceTest( int completedCount, int count )
		{
			//arrange
			List< PandaTask > tasksList = new List< PandaTask >( count );
			for( int i = 0; i < count; i++ )
			{
				tasksList.Add( new PandaTask() );
			}
		
			IPandaTask task = PandaTasksUtilities.Sequence( tasksList );
		
			//act
			for( int i = 0; i < completedCount; i++ )
			{
				tasksList[i].Resolve();
			}
		
			//assert
			Assert.AreEqual( PandaTaskStatus.Pending, task.Status );
		}

        [TestCase(0, 4)]
        [TestCase(3, 4)]
		public void RejectSequnceTaskTest( int rejectIndex, int count )
		{
			//arrange
			List< PandaTask > tasksList = new List< PandaTask >( count );
			for( int i = 0; i < count; i++ )
			{
				tasksList.Add( new PandaTask() );
			}
		
			IPandaTask task = PandaTasksUtilities.Sequence( tasksList );
		
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
        public void WaitWhile_Should_Return_Resolved_WhenConditionIsTrueInitial()
        {
            // arrange
            bool a = true;
            var task = PandaTasksUtilities.WaitWhile( () => a == false );

            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }

        [ Test ]
        public void WaitWhile_Should_Return_Pending_WhenConditionIsFalseInitial()
        {
            // arrange
            bool a = false;
            var task = PandaTasksUtilities.WaitWhile( () => a == false );

            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Pending ) );
        }

        [ AsyncTest ]
        public async Task WaitWhile_Should_Resolve_AfterConditionChange()
        {
            // arrange
            bool a = false;
            var tokenSource = new CancellationTokenSource();
            var task = PandaTasksUtilities.WaitWhile( () => a == false, tokenSource.Token );

            // act            
            a = true;

            await task;
            
            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }

        [ Test ]
        public void WaitWhile_Should_Cancel()
        {
            // arrange
            bool a = false;
            var tokenSource = new CancellationTokenSource();
            var task = PandaTasksUtilities.WaitWhile( () => a == false, tokenSource.Token );

            // act            
            a = true;
            tokenSource.Cancel();
            
            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( task.Error, Is.InstanceOf< TaskCanceledException >() );
        }

        [ AsyncTest ]
        public async Task WaitWhile_Should_RaiseException_FromCondition()
        {
            // arrange
            var task = PandaTasksUtilities.WaitWhile( () => throw new InvalidOperationException() );

            try
            {
                await task;
            }
            catch( Exception e )
            {
            }
            
            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( task.Error, Is.InstanceOf< InvalidOperationException >() );
        }

        [ AsyncTest ]
        public async Task WaitWhile_Should_NotThrow_WhenCancelled_AfterCompletion()
        {
            // arrange
            bool a = false;
            var tokenSource = new CancellationTokenSource();
            var task = PandaTasksUtilities.WaitWhile( () => a == false, tokenSource.Token );

            // act
            a = true;

            await task;

            // assert
            tokenSource.Cancel();
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }
        
        [ Test ]
        public void ThrowIfError_Should_Throw()
        {
            //act-assert
            Exception testError = new Exception();
            Assert.Throws< Exception >( PandaTasksUtilities.GetTaskWithError( testError ).ThrowIfError );
        }

        [ AsyncTest ]
        public async Task Delay_Should_Resolve_AfterGivenTime()
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            await PandaTasksUtilities.Delay( 10 );

            sw.Stop();

            Assert.That( sw.ElapsedMilliseconds, Is.GreaterThanOrEqualTo( 10 ) );
        }

        [ Test ]
        public void Delay_Should_Throw_WithNegativeTime()
        {
            Assert.That( () => PandaTasksUtilities.Delay( -1 ), Throws.ArgumentException );
        }

        [Test]
        public void Delay_Should_Cancel()
        {
            // arrange
            var tokenSource = new CancellationTokenSource();
            var task = PandaTasksUtilities.Delay( 100, tokenSource.Token );

            // act
            tokenSource.Cancel();

            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( task.Error, Is.InstanceOf< TaskCanceledException >() );
        }

        [ AsyncTest ]
        public async Task Delay_Should_NotThrow_WhenCancelled_AfterCompletion()
        {
            // arrange
            var tokenSource = new CancellationTokenSource();
            var task = PandaTasksUtilities.Delay( 1, tokenSource.Token );

            // act
            await task;
            tokenSource.Cancel();

            // assert
            tokenSource.Cancel();
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }

        [ AsyncTest ]
        public async Task OrTimeout_Should_Resolve_With_IPandaTask()
        {
            await PandaTasksUtilities.Delay( 5 ).OrTimeout( 100 );
        }

        [ AsyncTest ]
        public async Task OrTimeout_Should_Resolve_With_IPandaTaskResult()
        {
            var result = await ResultTask( 5 ).OrTimeout( 100 );
            Assert.That( result, Is.EqualTo( 1 ) );
        }

        [ Test ]
        public void OrTimeout_Should_Resolve_With_Resolved_IPandaTask()
        {
            var task = PandaTasksUtilities.CompletedTask.OrTimeout( 10 );

            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }
        
        [ Test ]
        public void OrTimeout_Should_Resolve_With_Resolved_IPandaTaskResult()
        {
            var task = PandaTasksUtilities.GetCompletedTask( 1 ).OrTimeout( 10 );

            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
            Assert.That( task.Result, Is.EqualTo( 1 ) );
        }

        [AsyncTest]
        public async Task OrTimeout_Should_Work_Without_Exception()
        {
            var taskToWait = PandaTasksUtilities.Delay(  2  );

            var timeoutTask = taskToWait.OrTimeout(  10 );

            try
            {
                await timeoutTask;
            }
            catch( Exception e )
            {
                Assert.Fail("Expected no exception, but got: " + e.Message);                
            }
        }
        
        [ Test ]
        public void OrTimeout_Should_Reject_With_Rejected_IPandaTask()
        {
            var ex = new Exception();
            var task = PandaTasksUtilities.GetTaskWithError( ex ).OrTimeout( 10 );

            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( task.Error, Is.EqualTo( ex ) );
        }

        [ Test ]
        public void OrTimeout_Should_Reject_With_Rejected_IPandaTaskResult()
        {
            var ex = new Exception();
            var task = PandaTasksUtilities.GetTaskWithError<int>( ex ).OrTimeout( 10 );

            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( task.Error, Is.EqualTo( ex ) );
        }

        [ AsyncTest ]
        public async Task OrTimeout_Should_Throw_With_Timeout_IPandaTask()
        {
            var task = PandaTasksUtilities.Delay( 100 ).OrTimeout( 5 );
            try
            {
                await task;
            }
            catch(TimeoutException)
            {
                Assert.Pass();
            }

            Assert.Fail( "Timeout did not throw" );
        }

        [ AsyncTest ]
        public async Task OrTimeout_Should_Throw_With_Timeout_IPandaTaskResult()
        {
            var task = ResultTask( 100 ).OrTimeout( 5 );
            try
            {
                await task;
            }
            catch (TimeoutException)
            {
                Assert.Pass();
            }

            Assert.Fail( "Timeout did not throw" );
        }

        [Test]
        public void OrTimeout_Should_Throw_With_NegativeTime_IPandaTask()
        {
            Assert.That( () => PandaTasksUtilities.Delay( 5 ).OrTimeout( -1 ), Throws.ArgumentException );
        }

        [Test]
        public void OrTimeout_Should_Throw_With_NegativeTime_IPandaTaskResult()
        {
            Assert.That( () => ResultTask( 5 ).OrTimeout( -1 ), Throws.ArgumentException );
        }

        [ AsyncTest ]
        public async IPandaTask RethrowError_Should_Have_CorrectException_AfterWaitAll()
        {
            await RethrowErrorTest( () => PandaTasksUtilities.WaitAll( PandaTasksUtilities.Delay( 1 ) ), () => throw new TestException() );

            Assert.That( TestSynchronizationContext.HandleException(), Is.InstanceOf< TestException >() );
        }

        [ AsyncTest ]
        public async IPandaTask RethrowError_Should_Have_CorrectException_AfterWaitAny()
        {
            await RethrowErrorTest( () => PandaTasksUtilities.WaitAny( PandaTasksUtilities.Delay( 1 ) ), () => throw new TestException() );

            Assert.That( TestSynchronizationContext.HandleException(), Is.InstanceOf< TestException >() );
        }

        [ AsyncTest ]
        public async IPandaTask RethrowError_Should_Have_CorrectException_AfterWaitWhile()
        {
            int i = 0;
            await RethrowErrorTest( () => PandaTasksUtilities.WaitWhile( () => ++i < 5 ), () => throw new TestException() );

            Assert.That( TestSynchronizationContext.HandleException(), Is.InstanceOf< TestException >() );
        }

        [ AsyncTest ]
        public async IPandaTask RethrowError_Should_Have_CorrectException_AfterDelay()
        {
            await RethrowErrorTest( () => PandaTasksUtilities.Delay( 1 ), () => throw new TestException() );

            Assert.That( TestSynchronizationContext.HandleException(), Is.InstanceOf< TestException >() );
        }

        [AsyncTest]
        public async IPandaTask WaitAll_Should_Success()
        {
            var waitAllTask =  PandaTasksUtilities.WaitAll( new List<IPandaTask> { PandaTasksUtilities.Delay( 100 ), PandaTasksUtilities.Delay( 200 ) } );

            await waitAllTask;

            Assert.AreEqual( PandaTaskStatus.Resolved, waitAllTask.Status );
        }

        private async IPandaTask< int > ResultTask(int durationMilliseconds)
        {
            await PandaTasksUtilities.Delay( durationMilliseconds );
            return 1;
        }
        
        private async IPandaTask RethrowErrorTest( Func< IPandaTask > asyncPart, Action throwPart )
        {
            var t = Task();

            t.RethrowError();
            await PandaTasksUtilities.WaitWhile( () => t.Status == PandaTaskStatus.Pending );

            async IPandaTask Task()
            {
                await asyncPart();
                throwPart();
            }
        }

        private class TestException : Exception
        {
        }
    }
    
    internal class TestClass { }

    internal class InheritedTestClass : TestClass { }
}
