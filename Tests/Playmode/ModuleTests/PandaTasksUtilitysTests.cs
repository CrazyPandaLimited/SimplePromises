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
    public sealed class PandaTasksUtilitysTests <T>
    {
        [Test]
        public void GetCanceledTask_Should_Return_Same_Instance() => 
            Assert.That( PandaTasksUtilitys.GetCanceledTask<T>(), Is.EqualTo( PandaTasksUtilitys.GetCanceledTask< T >() ) );
    }

    [ TestFixture( typeof( TestClass ), typeof( InheritedTestClass ) ) ]
    [ TestFixture( typeof( object ), typeof( float ) ) ]
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    public sealed class PandaTasksUtilitysTests <T1,T2>
    {
        [ Test ]
        public void GetCanceledTask_Should_Return_Different_Instance() => 
            Assert.That( PandaTasksUtilitys.GetCanceledTask< T1 >(), Is.Not.EqualTo( PandaTasksUtilitys.GetCanceledTask< T2 >() ) );
    }

    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
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
		
			IPandaTask task = PandaTasksUtilitys.Sequence( tasksList );
		
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
        public void WaitWhile_Should_Return_Resolved_WhenConditionIsTrueInitial()
        {
            // arrange
            bool a = true;
            var task = PandaTasksUtilitys.WaitWhile( () => a == false );

            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }

        [ Test ]
        public void WaitWhile_Should_Return_Pending_WhenConditionIsFalseInitial()
        {
            // arrange
            bool a = false;
            var task = PandaTasksUtilitys.WaitWhile( () => a == false );

            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Pending ) );
        }

        [ AsyncTest ]
        public async Task WaitWhile_Should_Resolve_AfterConditionChange()
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

        [ Test ]
        public void WaitWhile_Should_Cancel()
        {
            // arrange
            bool a = false;
            var tokenSource = new CancellationTokenSource();
            var task = PandaTasksUtilitys.WaitWhile( () => a == false, tokenSource.Token );

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
            var task = PandaTasksUtilitys.WaitWhile( () => throw new InvalidOperationException() );

            // act
            await PandaTasksUtilitys.Delay( 1 );

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
            var task = PandaTasksUtilitys.WaitWhile( () => a == false, tokenSource.Token );

            // act
            a = true;
            await PandaTasksUtilitys.Delay( 1 );

            // assert
            tokenSource.Cancel();
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }
        
        [ Test ]
        public void ThrowIfError_Should_Throw()
        {
            //act-assert
            Exception testError = new Exception();
            Assert.Throws< Exception >( PandaTasksUtilitys.GetTaskWithError( testError ).ThrowIfError );
        }

        [ AsyncTest ]
        public async Task Delay_Should_Resolve_AfterGivenTime()
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            await PandaTasksUtilitys.Delay( 10 );

            sw.Stop();

            Assert.That( sw.ElapsedMilliseconds, Is.GreaterThanOrEqualTo( 10 ) );
        }

        [ Test ]
        public void Delay_Should_Throw_WithNegativeTime()
        {
            Assert.That( () => PandaTasksUtilitys.Delay( -1 ), Throws.ArgumentException );
        }

        [Test]
        public void Delay_Should_Cancel()
        {
            // arrange
            var tokenSource = new CancellationTokenSource();
            var task = PandaTasksUtilitys.Delay( 100, tokenSource.Token );

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
            var task = PandaTasksUtilitys.Delay( 1, tokenSource.Token );

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
            await PandaTasksUtilitys.Delay( 5 ).OrTimeout( 100 );
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
            var task = PandaTasksUtilitys.CompletedTask.OrTimeout( 10 );

            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }
        
        [ Test ]
        public void OrTimeout_Should_Resolve_With_Resolved_IPandaTaskResult()
        {
            var task = PandaTasksUtilitys.GetCompletedTask( 1 ).OrTimeout( 10 );

            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
            Assert.That( task.Result, Is.EqualTo( 1 ) );
        }

        [ Test ]
        public void OrTimeout_Should_Reject_With_Rejected_IPandaTask()
        {
            var ex = new Exception();
            var task = PandaTasksUtilitys.GetTaskWithError( ex ).OrTimeout( 10 );

            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( task.Error, Is.EqualTo( ex ) );
        }

        [ Test ]
        public void OrTimeout_Should_Reject_With_Rejected_IPandaTaskResult()
        {
            var ex = new Exception();
            var task = PandaTasksUtilitys.GetTaskWithError<int>( ex ).OrTimeout( 10 );

            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( task.Error, Is.EqualTo( ex ) );
        }

        [ AsyncTest ]
        public async Task OrTimeout_Should_Throw_With_Timeout_IPandaTask()
        {
            var task = PandaTasksUtilitys.Delay( 100 ).OrTimeout( 5 );
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
            Assert.That( () => PandaTasksUtilitys.Delay( 5 ).OrTimeout( -1 ), Throws.ArgumentException );
        }

        [Test]
        public void OrTimeout_Should_Throw_With_NegativeTime_IPandaTaskResult()
        {
            Assert.That( () => ResultTask( 5 ).OrTimeout( -1 ), Throws.ArgumentException );
        }

        [ AsyncTest ]
        public async IPandaTask RethrowError_Should_Have_CorrectException_AfterWaitAll()
        {
            await RethrowErrorTest( () => PandaTasksUtilitys.WaitAll( PandaTasksUtilitys.Delay( 1 ) ), () => throw new TestException() );

            Assert.That( TestSynchronizationContext.HandleException(), Is.InstanceOf< TestException >() );
        }

        [ AsyncTest ]
        public async IPandaTask RethrowError_Should_Have_CorrectException_AfterWaitAny()
        {
            await RethrowErrorTest( () => PandaTasksUtilitys.WaitAny( PandaTasksUtilitys.Delay( 1 ) ), () => throw new TestException() );

            Assert.That( TestSynchronizationContext.HandleException(), Is.InstanceOf< TestException >() );
        }

        [ AsyncTest ]
        public async IPandaTask RethrowError_Should_Have_CorrectException_AfterWaitWhile()
        {
            int i = 0;
            await RethrowErrorTest( () => PandaTasksUtilitys.WaitWhile( () => ++i < 5 ), () => throw new TestException() );

            Assert.That( TestSynchronizationContext.HandleException(), Is.InstanceOf< TestException >() );
        }

        [ AsyncTest ]
        public async IPandaTask RethrowError_Should_Have_CorrectException_AfterDelay()
        {
            await RethrowErrorTest( () => PandaTasksUtilitys.Delay( 1 ), () => throw new TestException() );

            Assert.That( TestSynchronizationContext.HandleException(), Is.InstanceOf< TestException >() );
        }

        private async IPandaTask< int > ResultTask(int durationMilliseconds)
        {
            await PandaTasksUtilitys.Delay( durationMilliseconds );
            return 1;
        }
        
        private async IPandaTask RethrowErrorTest( Func< IPandaTask > asyncPart, Action throwPart )
        {
            var t = Task();

            t.RethrowError();
            await PandaTasksUtilitys.WaitWhile( () => t.Status == PandaTaskStatus.Pending );

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
