using System;
using NUnit.Framework;
using System.Collections.Generic;


namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
	public sealed class PandaTasksUtilitysTests
	{
		[ Test ]
		public void NullSequenceTest()
		{
			//act-assert
			Assert.Throws< NullReferenceException >( () => PandaTasksUtilitys.Sequence( new PandaTask(), new PandaTask(), null ) );
		}

		[ TestCase( 4 ) ]
		public void ResolveSequnceTest( int count )
		{
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

		[ TestCase( 0, 4 ) ]
		[ TestCase( 2, 4 ) ]
		[ TestCase( 3, 4 ) ]
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

		[ TestCase( 0, 4 ) ]
		[ TestCase( 3, 4 ) ]
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
			Assert.AreEqual( exception, task.Error.GetBaseException() );
		}
	}
}
