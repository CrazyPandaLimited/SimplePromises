using System;
using System.Runtime.CompilerServices;
using static CrazyPanda.UnityCore.PandaTasks.Consts;

namespace CrazyPanda.UnityCore.PandaTasks
{
#pragma warning disable CS0436 // Type conflicts with imported type
	[AsyncMethodBuilder( typeof( PandaTaskMethodBuilder<> ) )]
#pragma warning restore CS0436 // Type conflicts with imported type
    [ Obsolete( DeprecatedMessage, false) ]
	public interface IPandaTask< TResult > : IPandaTask
	{
		/// <summary>
		/// Result of task (throw exception if result unavailable)
		/// </summary>
		TResult Result { get; }

		/// <summary>
		/// Add complete with error task handler
		/// </summary>
		/// <param name="errorhandler">error handle callback</param>
		/// <returns>same Task</returns>
		IPandaTask< TResult > FailResult( Action< Exception > errorhandler );

		/// <summary>
		/// Add complete task handler
		/// </summary>
		/// <param name="completeHandler">complete handler</param>
		/// <returns>same Task</returns>
		IPandaTask< TResult > Done( Action< TResult > completeHandler );

		/// <summary>
		/// Chains current task with new task
		/// </summary>
		/// <param name="onResolved">resolved handler</param>
		/// <returns>new Task</returns>
		IPandaTask Then( Func< TResult, IPandaTask > onResolved );

		/// <summary>
		/// Chains current task with new task
		/// </summary>
		/// <param name="onResolved">resolved handler</param>
		/// <returns>new Task</returns>
		IPandaTask< TResult > Then( Func< TResult, IPandaTask< TResult > > onResolved );

		/// <summary>
		/// Chains current task with new task
		/// </summary>
		/// <param name="onResolved">resolved handler</param>
		/// <returns>new Task</returns>
		IPandaTask< TResult > ThenResult(  Action< TResult > onResolved );

		/// <summary>
		/// Chains current task with new task
		/// </summary>
		/// <param name="onResolved">resolved handler</param>
		/// <returns>new Task</returns>
		IPandaTask< TNewResult > Then< TNewResult >( Func< TResult, IPandaTask< TNewResult > > onResolved );

		/// <summary>
		/// Chains current task with new task on error case
		/// </summary>
		/// <param name="onCatch">error</param>
		/// <returns>new Task</returns>
		IPandaTask< TResult > Catch( Func< Exception, IPandaTask< TResult > > onCatch );

		/// <summary>
		/// Chains current task with new task on error case
		/// </summary>
		/// <param name="onCatch">error callback</param>
		/// <returns>new Task</returns>
		IPandaTask< TResult > CatchResult( Action< Exception > onCatch );
	}
}
