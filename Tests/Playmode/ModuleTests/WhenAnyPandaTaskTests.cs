using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    public sealed class WhenAnyPandaTaskTests
    {
        private static object[][] _resolveTaskTestCaseSource = new object[][]
        {
            new object[] { 1, 0 },
            new object[] { 2, 1 },
            new object[] { 2, 0 }
        };
        
        [ TestCaseSource( nameof(_resolveTaskTestCaseSource) ) ]
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

        private static object[][] _rejectTaskTestCaseSource = new object[][]
        {
            new object[] { 1, 0 },
            new object[] { 2, 1 },
            new object[] { 2, 0 }
        };
        
        [ TestCaseSource( nameof(_rejectTaskTestCaseSource) ) ]
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
        
        private static object[][] _multipleReolveTestCaseSource = new object[][]
        {
            new object[] { 3, 0 },
            new object[] { 2, 1 },
        };
        
        [ TestCaseSource( nameof(_multipleReolveTestCaseSource) ) ]
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

        private static object[][] _multipleErrorTestCaseSource = new object[][]
        {
            new object[] { 3, 0 },
            new object[] { 2, 1 },
        };
        
        [ TestCaseSource( nameof(_multipleErrorTestCaseSource) ) ]
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
