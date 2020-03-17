using NUnit.Framework;
using System;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    class PandaAsyncAwaitTests
    {
        [ AsyncTest ]
        public async IPandaTask ReturnsFromAwait()
        {
            // arrange
            async IPandaTask func() => await PandaTasksUtilitys.CompletedTask;

            // act
            await func();
        }

        [ AsyncTest ]
        public async IPandaTask ReturnsValueFromAwait()
        {
            // arrange
            async IPandaTask< int > func() => await PandaTasksUtilitys.GetCompletedTask( 1 );

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
            async IPandaTask func() => await PandaTasksUtilitys.GetTaskWithError( excpectException );

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
            async IPandaTask< int > func() => await PandaTasksUtilitys.GetTaskWithError< int >( excpectException );

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
            var excpectException = new Exception();
            async IPandaTask func()
            {
                throw excpectException;
                await NonSynchronousTask();
            }

            //act
            IPandaTask task = func();

            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( task.Error, Is.EqualTo( excpectException ) );
        }

        [ Test ]
        public void ThrowsOnAwaitBeforeAwaitResult()
        {
            // arrange
            var excpectException = new Exception();
            async IPandaTask< int > func()
            {
                throw excpectException;
                await NonSynchronousTask();
                return 1;
            }

            //act
            IPandaTask< int > task = func();

            // assert
            Assert.That( task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( task.Error, Is.EqualTo( excpectException ) );
        }

        [ AsyncTest ]
        public async IPandaTask TrowsOnAwaitAfterAwait()
        {
            // arrange
            var excpectException = new Exception();
            async IPandaTask< int > func()
            {
                await NonSynchronousTask();
                throw excpectException;
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
            Assert.That( realException, Is.EqualTo( excpectException ) );
        }

        [ AsyncTest ]
        public async IPandaTask TrowsOnAwaitAfterAwaitResult()
        {
            // arrange
            var excpectException = new Exception();
            async IPandaTask< int > func()
            {
                await NonSynchronousTask();
                throw excpectException;
                return 1;
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
            Assert.That( realException, Is.EqualTo( excpectException ) );
        }

        private IPandaTask NonSynchronousTask()
        {
            return PandaTasksUtilitys.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
}