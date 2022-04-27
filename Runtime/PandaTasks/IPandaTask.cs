using System;
using System.Runtime.CompilerServices;
using static CrazyPanda.UnityCore.PandaTasks.Consts;

namespace CrazyPanda.UnityCore.PandaTasks
{
	/// <summary>
	/// Represents async operation without value
	/// </summary>
#pragma warning disable CS0436 // Type conflicts with imported type
	[AsyncMethodBuilder( typeof( PandaTaskMethodBuilder ) )]
#pragma warning restore CS0436 // Type conflicts with imported type
    [ Obsolete( DeprecatedMessage, false) ]
    public interface IPandaTask : IDisposable
	{
		/// <summary>
		/// Current status of task
		/// </summary>
		PandaTaskStatus Status { get; }

		/// <summary>
		/// Current error of task
		/// </summary>
        Exception Error { get; }

		/// <summary>
		/// Add complete task handler
		/// </summary>
		/// <param name="completeHandler">complete handler</param>
		/// <returns>same Task</returns>
		IPandaTask Done( Action completeHandler );

		/// <summary>
		/// Add complete with error task handler
		/// </summary>
		/// <param name="errorhandler">error handle callback</param>
		/// <returns>same Task</returns>
		IPandaTask Fail( Action< Exception > errorhandler );

		/// <summary>
		/// Chains current task with new task
		/// </summary>
		/// <param name="onResolved">resolved handler</param>
		/// <returns>new Task</returns>
		IPandaTask Then( Func< IPandaTask > onResolved );

		/// <summary>
		/// Chains current task with new result task
		/// </summary>
		/// <typeparam name="TResult">result type</typeparam>
		/// <param name="onResolved">resolved handler</param>
		/// <returns>new Task</returns>
		IPandaTask< TResult > Then< TResult >( Func< IPandaTask< TResult > > onResolved );

		/// <summary>
		/// Chains current task with new task
		/// </summary>
		/// <param name="onResolved">resolved handler</param>
		/// <returns>new Task</returns>
		IPandaTask Then( Action onResolved );

		/// <summary>
		/// Chains current task with new task on error case
		/// </summary>
		/// <param name="onCatch">error</param>
		/// <returns>new Task</returns>
		IPandaTask Catch( Func< Exception, IPandaTask > onCatch );

		/// <summary>
		/// Chains current task with new task on error case
		/// </summary>
		/// <param name="onCatch">error callback</param>
		/// <returns>new Task</returns>
		IPandaTask Catch( Action< Exception > onCatch );

        /// <summary>
        /// Throws error if task is rejected. Error is same as task Error field.
        /// </summary>
        void ThrowIfError();
    }
}
