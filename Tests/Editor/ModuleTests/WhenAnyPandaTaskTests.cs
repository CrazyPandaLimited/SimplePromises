using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    public sealed class WhenAnyPandaTaskTests
    {
        [ TestCase( 1, 0 ) ]
        [ TestCase( 2, 1 ) ]
        [ TestCase( 2, 0 ) ]
        public void ResolveTaskTest(int totalCount, int resolveIndex)
        {
            //arrange
            List< PandaTask > testTasks = ConstructAnyTask( totalCount, out WhenAnyPandaTask anyTask );

            //act
            testTasks[resolveIndex].Resolve();

            //assert
            Assert.AreEqual( PandaTaskStatus.Resolved, anyTask.Status );
            Assert.AreEqual( testTasks[ resolveIndex ], anyTask.Result );
        }

        [ TestCase( 1, 0 ) ]
        [ TestCase( 2, 1 ) ]
        [ TestCase( 2, 0 ) ]
        public void RejectTaskTest( int totalCount, int rejectIndex )
        {
            //arrange
            List< PandaTask > testTasks = ConstructAnyTask( totalCount, out WhenAnyPandaTask anyTask );

            //act
            var realError = new Exception();
            testTasks[ rejectIndex ].Reject( realError );

            //assert
            Assert.AreEqual( PandaTaskStatus.Rejected, anyTask.Status );
            Assert.AreEqual( realError, anyTask.Error );
        }

        [ TestCase( 3, 0 ) ]
        [ TestCase( 2, 1 ) ]
        public void MultipleReolveTest( int totalCount, int resolveFromIndex )
        {
            //arrange
            List< PandaTask > testTasks = ConstructAnyTask( totalCount, out WhenAnyPandaTask anyTask );

            //act
            for( int i = resolveFromIndex; i < totalCount; i++ )
            {
                testTasks[ i ].Resolve();
            }

            //assert
            Assert.AreEqual( PandaTaskStatus.Resolved, anyTask.Status );
            Assert.AreEqual(  testTasks[ resolveFromIndex ], anyTask.Result );
        }

        [ TestCase( 3, 0 ) ]
        [ TestCase( 2, 1 ) ]
        public void MultipleErrorTest( int totalCount, int resolveFromIndex )
        {
            //arrange
            List< PandaTask > testTasks = ConstructAnyTask( totalCount, out WhenAnyPandaTask anyTask );

            //act
            var realError = new Exception();
            for( int i = resolveFromIndex; i < totalCount; i++ )
            {
                testTasks[ i ].Reject(realError);
            }

            //assert
            Assert.AreEqual( PandaTaskStatus.Rejected, anyTask.Status );
            Assert.AreEqual(  testTasks[ resolveFromIndex ].Error, anyTask.Error );
        }

        private List< PandaTask > ConstructAnyTask( int count, out WhenAnyPandaTask task )
        {
            var tasks = new List< PandaTask >(count);
            for( int i = 0; i < count; i++ )
            {
                tasks.Add( new PandaTask() );
            }

            task = new WhenAnyPandaTask( tasks );
            return tasks;
        }
    }
}
