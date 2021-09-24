using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    public sealed class WhenAllPandaTaskTests
    {
        private static IEnumerable TaskShouldCancelledTestCases
        {
             get
             {
                 yield return new TestCaseData( new[] { GetOperationCanceledTask() }, CancellationStrategy.FullCancel, typeof( TaskCanceledException ) );
                 yield return new TestCaseData( new[] { GetOperationCanceledTask() }, CancellationStrategy.PartCancel, typeof( TaskCanceledException ) );
                 yield return new TestCaseData( new[] { GetChildOperationCanceledTask() }, CancellationStrategy.FullCancel, typeof( TaskCanceledException ) );
                 yield return new TestCaseData( new[] { GetChildOperationCanceledTask() }, CancellationStrategy.PartCancel, typeof( TaskCanceledException ) );
                 yield return new TestCaseData( new[] { GetOperationCanceledTask(), GetChildOperationCanceledTask() }, CancellationStrategy.FullCancel, typeof( TaskCanceledException ) );
                 yield return new TestCaseData( new[] { GetOperationCanceledTask(), GetChildOperationCanceledTask() }, CancellationStrategy.PartCancel, typeof( TaskCanceledException ) );
                 
                 yield return new TestCaseData( new[] { GetOperationCanceledTask(), PandaTasksUtilities.CanceledTask }, CancellationStrategy.FullCancel, typeof( TaskCanceledException ) ).SetName( "FullOperationCancelTest" );
                 yield return new TestCaseData( new[] { GetOperationCanceledTask(), GetOperationCanceledTask() }, CancellationStrategy.FullCancel, typeof( TaskCanceledException ) ).SetName( "FullOperationOnlyCancelTest" );
                 yield return new TestCaseData( new[] { GetChildOperationCanceledTask(), GetChildOperationCanceledTask() }, CancellationStrategy.FullCancel, typeof( TaskCanceledException ) ).SetName( "FullChildOperationOnlyCancelTest" );
                 yield return new TestCaseData( new[] { PandaTasksUtilities.CanceledTask, PandaTasksUtilities.CompletedTask }, CancellationStrategy.FullCancel, typeof( AggregateException ) ).SetName( "FullCancelSomeSuccessTest" );
                 yield return new TestCaseData( new[] { GetOperationCanceledTask(), PandaTasksUtilities.CompletedTask }, CancellationStrategy.FullCancel, typeof( AggregateException ) ).SetName( "FullOperationCancelSomeSuccessTest" );
                 yield return new TestCaseData( new[] { GetChildOperationCanceledTask(), PandaTasksUtilities.CompletedTask }, CancellationStrategy.FullCancel, typeof( AggregateException ) ).SetName( "FullChildOperationCancelSomeSuccessTest" );
                 yield return new TestCaseData( new[] { PandaTasksUtilities.CanceledTask, PandaTasksUtilities.GetTaskWithError( new Exception() ) }, CancellationStrategy.FullCancel, typeof( AggregateException ) ).SetName( "FullCancelWrongTypeTest" );
                 yield return new TestCaseData( new []{PandaTasksUtilities.CanceledTask, PandaTasksUtilities.CanceledTask}, CancellationStrategy.PartCancel, typeof( TaskCanceledException ) ).SetName( "HalfCancelTest" );
                 yield return new TestCaseData( new[] { GetOperationCanceledTask(), PandaTasksUtilities.CanceledTask }, CancellationStrategy.PartCancel, typeof( TaskCanceledException ) ).SetName( "HalfOperationCancelTest" );
                 yield return new TestCaseData( new[] { GetOperationCanceledTask(), GetOperationCanceledTask() }, CancellationStrategy.PartCancel, typeof( TaskCanceledException ) ).SetName( "HalfOnlyOperationCancelTest" );
                 yield return new TestCaseData( new[] { GetChildOperationCanceledTask(), GetChildOperationCanceledTask() }, CancellationStrategy.PartCancel, typeof( TaskCanceledException ) ).SetName( "HalfOnlyChildOperationCancelTest" );
                 yield return new TestCaseData( new []{PandaTasksUtilities.CanceledTask, PandaTasksUtilities.CompletedTask}, CancellationStrategy.PartCancel, typeof( TaskCanceledException ) ).SetName( "HalfCancelSomeSuccessTest" );
                 yield return new TestCaseData( new[] { GetOperationCanceledTask(), PandaTasksUtilities.CompletedTask }, CancellationStrategy.PartCancel, typeof( TaskCanceledException ) ).SetName( "HalfOperationCancelSomeSuccessTest" );
                 yield return new TestCaseData( new[] { GetChildOperationCanceledTask(), PandaTasksUtilities.CompletedTask }, CancellationStrategy.PartCancel, typeof( TaskCanceledException ) ).SetName( "HalfChildOperationCancelSomeSuccessTest" );
                 yield return new TestCaseData( new[] { PandaTasksUtilities.CanceledTask, PandaTasksUtilities.GetTaskWithError( new Exception() ) }, CancellationStrategy.PartCancel, typeof( AggregateException ) ).SetName( "HalfCancelWrongTypeTest" );
             }
        }

        [ Test ]
        public void ResolveTest()
        {
            //arrange
            var task = new WhenAllPandaTask( new[] { new PandaTask() }, CancellationStrategy.Aggregate );

            //act-assert
            Assert.Throws< InvalidOperationException >( task.Resolve );
        }

        [ Test ]
        public void RejectTest()
        {
            //arrange
            var task = new WhenAllPandaTask( new[] { new PandaTask() }, CancellationStrategy.Aggregate );

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

            var task = new WhenAllPandaTask( tasksCollection, CancellationStrategy.Aggregate );

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

            var task = new WhenAllPandaTask( tasksCollection, CancellationStrategy.Aggregate );

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
            var task = new WhenAllPandaTask( new[] { innerTask }, CancellationStrategy.Aggregate );

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

            var task = new WhenAllPandaTask( tasksCollection, CancellationStrategy.Aggregate );

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

            var task = new WhenAllPandaTask( tasksCollection, CancellationStrategy.Aggregate );

            //act
            tasksCollection.ForEach( x => x.Reject( new Exception() ) );

            //assert
            Assert.AreEqual( PandaTaskStatus.Rejected, task.Status );
            Assert.IsInstanceOf< AggregateException >( task.Error );

            ReadOnlyCollection< Exception > realExceptions = (( AggregateException )task.Error).Flatten().InnerExceptions;
            CollectionAssert.AreEquivalent( tasksCollection.Select( x => x.Error ), realExceptions );
        }

        [ TestCase( 2, 1 ) ]
        [ TestCase( 4, 2 ) ]
        public void HalfRejectTest( int count, int rejectCount )
        {
            //arrange
            var tasksCollection = new List< PandaTask >( count );
            for( int i = 0; i < count; i++ )
            {
                tasksCollection.Add( new PandaTask() );
            }

            var task = new WhenAllPandaTask( tasksCollection, CancellationStrategy.Aggregate );

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

            ReadOnlyCollection< Exception > realExceptions = (( AggregateException )task.Error).Flatten().InnerExceptions;
            CollectionAssert.AreEquivalent( tasksCollection.Where( x => x.Status == PandaTaskStatus.Rejected ).Select( x => x.Error ), realExceptions );
        }

        [ TestCase( 2 ) ]
        [ TestCase( 5 ) ]
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
            var task = new WhenAllPandaTask( tasksCollection, CancellationStrategy.Aggregate );

            //assert
            Assert.AreEqual( PandaTaskStatus.Pending, task.Status );
        }

        [ TestCase( 1 ) ]
        [ TestCase( 5 ) ]
        public void InitResolvedTest( int count )
        {
            //arrange
            var tasksCollection = new List< IPandaTask >( count );
            for( int i = 0; i < count; i++ )
            {
                tasksCollection.Add( PandaTasksUtilities.CompletedTask );
            }

            //act
            var task = new WhenAllPandaTask( tasksCollection, CancellationStrategy.Aggregate );

            //assert
            Assert.AreEqual( PandaTaskStatus.Resolved, task.Status );
        }

        [ Test ]
        public void ZeroTasksTest()
        {
            //act
            var task = new WhenAllPandaTask( Enumerable.Empty< IPandaTask >(), CancellationStrategy.Aggregate );

            //assert
            Assert.AreEqual( PandaTaskStatus.Resolved, task.Status );
        }

        [ TestCaseSource( nameof(TaskShouldCancelledTestCases) ) ]
        public void Task_Should_Be_Cancelled( IEnumerable< IPandaTask > tasks, CancellationStrategy cancellationStrategy, Type exceptionType )
        {
            var task = new WhenAllPandaTask( tasks, cancellationStrategy );
            Assert.AreEqual( PandaTaskStatus.Rejected, task.Status );
            Assert.That( task.Error, Is.InstanceOf( exceptionType ) );
        }
         
        [ Test ]
        public void ChangeCollectionCompleteTest()
        {
            //arrange
            var source = UnsafeCompletionSource.Create();
            var tasks = new List< IPandaTask >
            {
                PandaTasksUtilities.CompletedTask,
                source.Task
            };

            var allTask = new WhenAllPandaTask( tasks, CancellationStrategy.Aggregate );
            tasks.Add( new PandaTask() );

            //act
            source.Resolve();

            //assert
            Assert.AreEqual( PandaTaskStatus.Resolved, allTask.Status );
        }

        [ Test ]
        public void ChangeCollectionCompleteErrorTest()
        {
            //arrange
            var source = UnsafeCompletionSource.Create();
            var tasks = new List< IPandaTask >
            {
                PandaTasksUtilities.CompletedTask,
                source.Task
            };

            var allTask = new WhenAllPandaTask( tasks, CancellationStrategy.Aggregate );
            tasks.Add( PandaTasksUtilities.CanceledTask );

            //act
            source.Resolve();

            //assert
            Assert.AreEqual( PandaTaskStatus.Resolved, allTask.Status );
        }

        [ Test ]
        public void ChangeCollectionCompleteWithErrorTest()
        {
            //arrange
            var source = UnsafeCompletionSource.Create();
            var tasks = new List< IPandaTask >
            {
                PandaTasksUtilities.CompletedTask,
                source.Task
            };

            var allTask = new WhenAllPandaTask( tasks, CancellationStrategy.Aggregate );
            tasks.Add( PandaTasksUtilities.CanceledTask );

            //act
            var testError = new Exception();
            source.SetError( testError );
 
            //assert
            Assert.AreEqual( PandaTaskStatus.Rejected, allTask.Status );
            Assert.IsInstanceOf< AggregateException >( allTask.Error );
            CollectionAssert.AreEqual( new[] { testError }, (( AggregateException )allTask.Error).InnerExceptions );
        }

        private static IPandaTask GetOperationCanceledTask() => PandaTasksUtilities.GetTaskWithError( new OperationCanceledException() );

        private static IPandaTask GetChildOperationCanceledTask() => PandaTasksUtilities.GetTaskWithError( new RandomChildOfOperationCanceledException() );

        private class RandomChildOfOperationCanceledException : OperationCanceledException { }
    }
}