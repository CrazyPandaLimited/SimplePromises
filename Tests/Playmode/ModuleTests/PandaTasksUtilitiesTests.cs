using System;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using CrazyPanda.UnityCore.PandaTasks.PerfTests;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    [ TestFixture( typeof( object ) ) ]
    public sealed class PandaTasksUtilitiesTests <T>
    {
        [Test]
        public void GetCanceledTask_Should_Return_Same_Instance() => 
            Assert.That( PandaTasksUtilities.GetCanceledTask<T>(), Is.EqualTo( PandaTasksUtilities.GetCanceledTask< T >() ) );
    }

    [ TestFixture( typeof( TestClass ), typeof( InheritedTestClass ) ) ]
    [ TestFixture( typeof( object ), typeof( TestClass ) ) ]
    public sealed class PandaTasksUtilitiesTests <T1,T2>
    {
        [ Test ]
        public void GetCanceledTask_Should_Return_Different_Instance() => 
            Assert.That( PandaTasksUtilities.GetCanceledTask< T1 >(), Is.Not.EqualTo( PandaTasksUtilities.GetCanceledTask< T2 >() ) );
    }

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

        [ Test ]
        public void Delay_Should_Throw_WithNegativeTime()
        {
            Assert.That( () => PandaTasksUtilities.Delay( -1 ), Throws.ArgumentException );
        }

        [ Test ]
        public void Delay_With_ZeroTime()
        {
            Assert.That(  PandaTasksUtilities.Delay( TimeSpan.Zero ).Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }

        [ Test ]
        public void DelayTask_Test()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                var taskToWait = PandaTasksUtilities.Delay( 50 );
                Thread.Sleep( 250 );
                context.ExecuteTasks();
                Assert.That(  taskToWait.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
            }
        }

        [ Test ]
        public void DelayTask_WithInversedTimeout_Test()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                var taskToWait1 = PandaTasksUtilities.Delay( 500 );
                var taskToWait2 = PandaTasksUtilities.Delay( 30 );
                Thread.Sleep( 250 );
                context.ExecuteTasks();
                Assert.That(  taskToWait2.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
                Assert.That(  taskToWait1.Status, Is.EqualTo( PandaTaskStatus.Pending ) );
            }
        }

        [ Test ]
        public void DelayTasks_Test()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                List< PandaTask > finishedTasks = new List< PandaTask >( 2 ); 

                var taskToWait1 = PandaTasksUtilities.Delay( 50 );
                taskToWait1.Done( () => finishedTasks.Add( taskToWait1 ) );
                
                var taskToWait2 = PandaTasksUtilities.Delay( 100 );
                taskToWait2.Done( () => finishedTasks.Add( taskToWait2 ) );

                Thread.Sleep( 250 );

                context.ExecuteTasks();

                var expectedResult = new List< PandaTask >() { taskToWait1, taskToWait2 };
                
                Assert.That( finishedTasks, Is.EquivalentTo( expectedResult  ) );
            }
        }
        
        [ Test ]
        public void DelayTasks_WithInversedTimeout_Test()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                List< PandaTask > finishedTasks = new List< PandaTask >( 2 ); 

                var taskToWait1 = PandaTasksUtilities.Delay( 100 );
                taskToWait1.Done( () => finishedTasks.Add( taskToWait1 ) );
                
                var taskToWait2 = PandaTasksUtilities.Delay( 50 );
                taskToWait2.Done( () => finishedTasks.Add( taskToWait2 ) );

                Thread.Sleep( 250 );

                context.ExecuteTasks();

                var expectedResult = new List< PandaTask >() { taskToWait1, taskToWait2 };
                
                Assert.That( finishedTasks, Is.EquivalentTo( expectedResult  ) );
            }
        }
        
        [ Test ]
        public void DelayTask_Cancellation_FromCompleteHandler()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                var task = PandaTasksUtilities.Delay( 50, cancellationTokenSource.Token );
                task.Done( () => cancellationTokenSource.Cancel() );

                Thread.Sleep( 250 );

                context.ExecuteTasks();
                Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
            }
        }

        [ Test ]
        public void DelayTask_Cancellation_After_Resolving_Another()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                
                var task1 = PandaTasksUtilities.Delay( 50 );
                task1.Done( () => cancellationTokenSource.Cancel() );
                
                var task2 = PandaTasksUtilities.Delay( 700, cancellationTokenSource.Token );

                Thread.Sleep( 250 );

                context.ExecuteTasks();

                Assert.That( task2.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            }
        }

        [ Test ]
        public void DelayTask_Throwing_Exception_FromCompleteHandler()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                var exceptionToThrow = new Exception( "some_exception" );
                
                var task1 = PandaTasksUtilities.Delay( 30 );
                var task2 = PandaTasksUtilities.Delay( 100 );

                task1.Done( () => throw exceptionToThrow );

                Thread.Sleep( 250 );

                try
                {
                    context.ExecuteTasks();
                }
                catch( Exception e )
                {
                    Assert.That( exceptionToThrow, Is.EqualTo( e ) );
                }
                
                context.ExecuteTasks();

                Assert.That( task2.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
            }
        }

        [ Test ]
        public void DelayTaskCreation_At_CompleteHandler()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                var task1 = PandaTasksUtilities.Delay( 30 );
                PandaTask task2 = null;

                task1.Done( () => task2 = PandaTasksUtilities.Delay( 50 ) );

                Thread.Sleep( 250 );
                context.ExecuteTasks();

                Thread.Sleep( 250 );
                context.ExecuteTasks();
                
                Assert.That( task2.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
            }
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

        [ Test ]
        public void Delay_Should_NotThrow_WhenCancelled_AfterCompletion()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                // arrange
                var tokenSource = new CancellationTokenSource();
                var task = PandaTasksUtilities.Delay( 50, tokenSource.Token );

                Thread.Sleep( 250 );

                while( context.HasPendingTasks() )
                {
                    context.ExecuteTasks();
                }
                
                tokenSource.Cancel();

                Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
            }
        }

        [ Test ]
        public void OrTimeout_Should_Resolve_With_IPandaTask()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                var task = ResultTask( 15 ).OrTimeout( 100 );
                
                Thread.Sleep( 250 );

                while( context.HasPendingTasks() )
                {
                    context.ExecuteTasks();
                }
                
                Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
            }
        }

        [ Test ]
        public void OrTimeout_Should_Resolve_With_IPandaTaskResult()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                var task = ResultTask( 25 ).OrTimeout( 500 );
                
                Thread.Sleep( 250 );

                while( context.HasPendingTasks() )
                {
                    context.ExecuteTasks();
                }
                
                Assert.That( task.Result, Is.EqualTo( 1 ) );
            }
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

        [Test]
        public void OrTimeout_Should_Work_Without_Exception()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext())
            {
                _ = PandaTasksUtilities.Delay(  50  ).OrTimeout(  100 );

                Thread.Sleep( 250 );

                try
                {
                    context.ExecuteTasks();
                }
                catch( Exception e )
                {
                    Assert.Fail( "Expected no exception, but got: " + e.Message );
                }
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

        [ Test ]
        public void OrTimeout_Should_Throw_With_Timeout_IPandaTask()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                var task = PandaTasksUtilities.Delay( 100 ).OrTimeout( 5 );

                Thread.Sleep( 250 );
                
                context.ExecuteTasks();

                Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
                Assert.That( task.Error, Is.Not.Null );
            }
        }

        [ Test ]
        public void OrTimeout_Should_Throw_With_Timeout_IPandaTaskResult()
        {
            using( var context = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                var task = ResultTask( 100 ).OrTimeout( 5 );

                Thread.Sleep( 250 );

                context.ExecuteTasks();

                Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
                Assert.That( task.Error, Is.Not.Null );
            }
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
