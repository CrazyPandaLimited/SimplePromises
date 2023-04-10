using System;
using NUnit.Framework;


namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    public sealed class ResultContinuationTaskFromPandaTaskTest
	{
		[ Test ]
		public void CreateTest()
		{
			//act
			PandaTask<int> testTask = ConstructTask< int, int >();

			//assert
			Assert.AreEqual(PandaTaskStatus.Pending, testTask.Status );
			Assert.Throws< InvalidOperationException >( () =>
			{
				var _ = testTask.Result;

			} );

			Assert.Null( testTask.Error );
		}

		[ Test ]
		public void SetValueTest()
		{
			//arrange
			PandaTask<int> testTask = ConstructTask< int, int >();

			//act-assert
			Assert.Throws< InvalidOperationException >( () => testTask.SetValue( 42 ) );
		}

		[ Test ]
		public void DisposeTest()
		{
			//arrange
			PandaTask<int> testTask = ConstructTask< int, int >();

			//act
			testTask.Dispose();

			//assert
			Assert.AreEqual(PandaTaskStatus.Rejected, testTask.Status );

			Assert.Throws< ObjectDisposedException >( () =>
			{
				var _ = testTask.Result;
			}  );
			Assert.IsInstanceOf< ObjectDisposedException >( testTask.Error );
		}

		[ Test ]
		public void ResolveTest()
		{
			//arrange
			PandaTask<int> testTask = ConstructTask< int, int >();

			//act-assert
			Assert.Throws< InvalidOperationException >( testTask.Resolve );
		}

		[ Test ]
		public void ResolveTaskTest()
		{
			//arrange
            var firstTask = new PandaTask< int >();
            var secondTask = new PandaTask< int >();

			const int offset = 3;
			PandaTask< int > testTask = ConstructTask( firstTask, false, () =>
			{
				secondTask.SetValue( firstTask.Result + offset );
				return secondTask;
			} );

			//act
			firstTask.Resolve();

			//assert
			Assert.AreEqual( firstTask.Result + offset, testTask.Result );
			Assert.AreEqual( PandaTaskStatus.Resolved, testTask.Status );
		}

		[ Test ]
		public void ResolveFromRejectTest()
		{
			//arrange
			var firstTask = new PandaTask<int>();
			var secondTask = new PandaTask< int >();

			const int realResult = 3;
			PandaTask< int > testTask = ConstructTask( firstTask, true, () =>
			{
				secondTask.SetValue( realResult );
				return secondTask;
			} );

			//act
			firstTask.Reject();

			//assert
			Assert.AreEqual( realResult, testTask.Result );
			Assert.AreEqual( PandaTaskStatus.Resolved, testTask.Status );
		}

        [ Test ]
        public void RejectResultTest()
        {
            //arrange
            var firstTask = new PandaTask< int >();
            var secondTask = new PandaTask< int >();

            const int realResult = 3;
            bool callbackCalled = false;
            PandaTask< int > testTask = new ContinuationTaskFromPandaTask< int >( firstTask, () =>
            {
                callbackCalled = true;
                return secondTask;
            }, true );

            //act
            firstTask.SetValue( realResult );

            //assert
            Assert.False( callbackCalled );
            Assert.AreEqual( realResult, testTask.Result );
            Assert.AreEqual( PandaTaskStatus.Resolved, testTask.Status );
        }

        private PandaTask< TResult > ConstructTask< TFirstResult, TResult >( IPandaTask< TFirstResult > firstTask = null, bool fromCatch = false, Func< IPandaTask< TResult > > nextTaskCallback = null )
        {
            //create default task
            if( firstTask == null )
            {
                firstTask = new PandaTask< TFirstResult >();
            }

            if( nextTaskCallback == null )
            {
                nextTaskCallback = () => null;
            }

            return new ContinuationTaskFromPandaTask< TResult >( firstTask, nextTaskCallback, fromCatch );
        }
	}
}
