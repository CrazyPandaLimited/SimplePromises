using NUnit.Framework;
using System;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    class PandaAsyncAwaitTests
    {
        [ AsyncTest ]
        public async IPandaTask ReturnsFromAwait()
        {
            // arrange
            async IPandaTask func() => await PandaTasksUtilities.CompletedTask;

            // act
            await func();
        }

        [ AsyncTest ]
        public async IPandaTask ReturnsValueFromAwait()
        {
            // arrange
            async IPandaTask< int > func() => await PandaTasksUtilities.GetCompletedTask( 1 );

            // act
            var value = await func();

            // assert
            Assert.That( value, Is.EqualTo( 1 ) );
        }

        [ AsyncTest ]
        public async IPandaTask ThrowsOnAwait()
        {
            // arrange
            var excpectException = new Exception();
            async IPandaTask func() => await PandaTasksUtilities.GetTaskWithError( excpectException );

            //act
            Exception realException = null;
            try
            {
                await func();
            }
            catch( Exception ex )
            {
                realException = ex;
            }

            //assert
            Assert.That( realException, Is.EqualTo( excpectException ) );
        }

        [ AsyncTest ]
        public async IPandaTask ThrowsOnAwaitResult()
        {
            // arrange
            var excpectException = new Exception();
            async IPandaTask< int > func() => await PandaTasksUtilities.GetTaskWithError< int >( excpectException );

            //act
            Exception realException = null;
            try
            {
                await func();
            }
            catch( Exception ex )
            {
                realException = ex;
            }

            //assert
            Assert.That( realException, Is.EqualTo( excpectException ) );
        }

        [ Test ]
        public void ThrowsOnAwaitBeforeAwait()
        {
            // arrange
            var expectException = new Exception();
            async IPandaTask func()
            {
                if( expectException != null )
                    throw expectException;
                await NonSynchronousTask();
            }

            //act
            var task = func();

            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( task.Error, Is.EqualTo( expectException ) );
        }

        [ Test ]
        public void ThrowsOnAwaitBeforeAwaitResult()
        {
            // arrange
            var expectException = new Exception();
            async IPandaTask< int > func()
            {
                if( expectException != null )
                    throw expectException;
                await NonSynchronousTask();
                return 1;
            }

            //act
            var task = func();

            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( task.Error, Is.EqualTo( expectException ) );
        }

        [ AsyncTest ]
        public async IPandaTask TrowsOnAwaitAfterAwait()
        {
            // arrange
            var expectException = new Exception();
            async IPandaTask< int > func()
            {
                await NonSynchronousTask();
                throw expectException;
            }

            //act
            Exception realException = null;
            try
            {
                await func();
            }
            catch( Exception ex )
            {
                realException = ex;
            }

            //assert
            Assert.That( realException, Is.EqualTo( expectException ) );
        }

        [ AsyncTest ]
        public async IPandaTask TrowsOnAwaitAfterAwaitResult()
        {
            // arrange
            var expectException = new Exception();
            async IPandaTask< int > func()
            {
                await NonSynchronousTask();
                throw expectException;
            }

            //act
            Exception realException = null;
            try
            {
                await func();
            }
            catch( Exception ex )
            {
                realException = ex;
            }

            //assert
            Assert.That( realException, Is.EqualTo( expectException ) );
        }

        private IPandaTask NonSynchronousTask()
        {
            return PandaTasksUtilities.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
}