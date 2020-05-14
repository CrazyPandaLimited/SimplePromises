using System;
using NUnit.Framework;


namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    public sealed class PandaResultTaskTests
	{
		[ Test ]
		public void CreateTest()
		{
			//act
			var testTask = new PandaTask< int >();

			//assert
			Assert.AreEqual( PandaTaskStatus.Pending, testTask.Status );
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
			var testTask = new PandaTask< int >();

			int callbackResult = 0;
			testTask.Done( x => callbackResult = x );

			bool rejected = false;
			testTask.Fail( x => rejected = true );

			//act
			const int testValue = 32;
			testTask.SetValue( testValue );

			//assert
			Assert.False( rejected );
			Assert.Null( testTask.Error );
			Assert.AreEqual( testValue, callbackResult );
			Assert.AreEqual( testValue, testTask.Result);
			Assert.AreEqual( PandaTaskStatus.Resolved,  testTask.Status);
		}

		[ Test ]
		public void DoubleSetValueTest()
		{
			//arrange
			var testTask = new PandaTask< int >();
			testTask.SetValue( 0 );

			//act-assert
			Assert.Throws< InvalidOperationException >( () => testTask.SetValue( 0 ) );
		}

		[ Test ]
		public void SetValueAfterRejectTest()
		{
			//arrange
			var testTask = new PandaTask< int >();
			testTask.Reject();
			
			//act-assert
			Assert.Throws< InvalidOperationException >( () => testTask.SetValue( 0 ) );
		}

		[ Test ]
		public void RejectTest()
		{
			//arrange
			var testTask = new PandaTask< int >();

			//act
			Exception testException = new Exception();
			testTask.Reject( testException );

			//assert
			Assert.AreEqual( testException, testTask.Error );

			Exception realException = null;
			try
			{
				var _ = testTask.Result;
			}
			catch( Exception exception )
            {
                realException = exception;
            }

			Assert.AreEqual( testException, realException );
		}

		[ Test ]
		public void DoubleRejectTest()
		{
			//arrange
			var testTask = new PandaTask< int >();
			testTask.Reject();

			//act-assert
			Assert.Throws< InvalidOperationException >( testTask.Reject );
		}

		[ Test ]
		public void RejectAfterSetTest()
		{
			//arrange
			var testTask = new PandaTask< int >();
			testTask.SetValue( 0 );

			//act-assert
			Assert.Throws< InvalidOperationException >( testTask.Reject );
		}

        [ Test ]
        public void ChainCatchTest()
        {
            //arrange
            var testTask = new PandaTask< int >();
            var testTask2 = new PandaTask< int >();
            var resultTask = testTask.Catch( x => testTask2 );

            //act
            const int testValue = 1;
            testTask.SetValue( testValue );

            //assert
            Assert.AreEqual( testValue, resultTask.Result );
        }

		[ Test ]
		public void DisposeTest()
		{
			//arrange
			var testTask = new PandaTask< int >();

			//act
			testTask.Dispose();

			//assert
			Assert.AreEqual( PandaTaskStatus.Rejected, testTask.Status );
			Assert.IsInstanceOf<ObjectDisposedException>( testTask.Error );
		}
	}
}
