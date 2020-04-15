using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    public sealed class WhenAnyPandaTaskTests
    {
        [ Test ]
        public void ResolveTaskTest_1_0() => ResolveTaskTest( 1, 0 );
        [ Test ]
        public void ResolveTaskTest_2_1() => ResolveTaskTest( 2, 1 );
        [ Test ]
        public void ResolveTaskTest_2_0() => ResolveTaskTest( 2, 0 );
        
        private void ResolveTaskTest(int totalCount, int resolveIndex)
        {
            //arrange
            List< PandaTask > testTasks = ConstructAnyTask( totalCount, out WhenAnyPandaTask anyTask );

            //act
            testTasks[resolveIndex].Resolve();

            //assert
            Assert.AreEqual( PandaTaskStatus.Resolved, anyTask.Status );
            Assert.AreEqual( testTasks[ resolveIndex ], anyTask.Result );
        }
        
        [ Test ]
        public void RejectTaskTest_1_0() => RejectTaskTest( 1, 0 );
        [ Test ]
        public void RejectTaskTest_2_1() => RejectTaskTest( 2, 1 );
        [ Test ]
        public void RejectTaskTest_2_0() => RejectTaskTest( 2, 0 );
        
        private void RejectTaskTest( int totalCount, int rejectIndex )
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
        
        [ Test ]
        public void MultipleReolveTest_3_0() => MultipleReolveTest( 3, 0 );
        [ Test ]
        public void MultipleReolveTest_2_1() => MultipleReolveTest( 2, 1 );
        
        private void MultipleReolveTest( int totalCount, int resolveFromIndex )
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

        [ Test ]
        public void MultipleErrorTest_3_0() => MultipleErrorTest( 3, 0 );
        
        [ Test ]
        public void MultipleErrorTest_2_1() => MultipleErrorTest( 2, 1 );

        private void MultipleErrorTest( int totalCount, int resolveFromIndex )
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
