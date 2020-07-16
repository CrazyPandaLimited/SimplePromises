using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.PandaTasks
{
    [ DebuggerNonUserCode ]
	public static class PandaTasksUtilitys
	{
        #region Constructor
        static PandaTasksUtilitys()
        {
            //create resolved and canceled tasks. Both tasks will be created before first class access.
            var completedTask = new PandaTask();
            completedTask.Resolve();
            CompletedTask = completedTask;

            var canceledTask = new PandaTask();
            canceledTask.TryCancel();
            CanceledTask = canceledTask;
        }
        #endregion

		#region Public Members
        /// <summary>
        /// Completed Task
        /// </summary>
        public static readonly IPandaTask CompletedTask;

        /// <summary>
        /// Canceled Task
        /// </summary>
        public static readonly IPandaTask CanceledTask;
            
		/// <summary>
		/// Returns completed task with result
		/// </summary>
		public static IPandaTask< T > GetCompletedTask< T >( T result )
		{
			var resultTask = new PandaTask< T >();
			resultTask.SetValue( result );
			return resultTask;
		}

		/// <summary>
		/// Returns rejected task with error
		/// </summary>
		public static IPandaTask< T > GetTaskWithError< T >( Exception error )
		{
			var resultTask = new PandaTask< T >();
			resultTask.Reject( error );
			return resultTask;
		}

        /// <summary>
        /// Create task with TaskCanceledException.
        /// </summary>
        public static IPandaTask< T > GetCanceledTask< T >()
        {
            var resultTask = new PandaTask< T >();
            resultTask.Reject( new TaskCanceledException() );
            return resultTask;
        }

		/// <summary>
		/// Returns rejected task with error
		/// </summary>
		public static IPandaTask GetTaskWithError( Exception error )
		{
			var resultTask = new PandaTask();
			resultTask.Reject( error );
			return resultTask;
		}

		/// <summary>
		/// Construct sequence task from Task params
		/// </summary>
		public static IPandaTask Sequence( params IPandaTask[ ] tasks )
		{
			return Sequence( ( IEnumerable< IPandaTask > ) tasks );
		}

		/// <summary>
		/// Construct sequence task from Tasks collection
		/// </summary>
		public static IPandaTask Sequence( IEnumerable< IPandaTask > tasksCollection )
		{
			//check arguments
			if( tasksCollection == null )
			{
				throw new ArgumentNullException( nameof(tasksCollection) );
			}

			if( tasksCollection.Any( x => x == null ) )
            {
                throw new ArgumentException( @"One of task element is null", nameof(tasksCollection) );
            }

			IPandaTask sequenceTask = tasksCollection.Aggregate( ( x, y ) => x.Then( () => y ) );
			return sequenceTask;
		}

        /// <summary>
        /// Creates task for waiting all tasks.
        /// </summary>
        public static IPandaTask WaitAll( params IPandaTask[ ] tasks )
        {
            return new WhenAllPandaTask( tasks, CancellationStrategy.Aggregate );
        }

        /// <summary>
        /// Creates task for waiting all tasks.
        /// </summary>
        /// <param name="tasks">task`s collection</param>
        public static IPandaTask WaitAll( IEnumerable< IPandaTask > tasks )
        {
            return new WhenAllPandaTask( tasks, CancellationStrategy.Aggregate );
        }

        /// <summary>
        /// Creates task for waiting all tasks.
        /// </summary>
        /// <param name="strategy">strategy for TaskCanceledException`s aggregation</param>
        /// <param name="tasks">task`s collection</param>
        public static IPandaTask WaitAll( CancellationStrategy strategy, params IPandaTask[ ] tasks )
        {
            return new WhenAllPandaTask( tasks, strategy );
        }

        /// <summary>
        /// Creates task for waiting all tasks.
        /// </summary>
        /// <param name="strategy">strategy for TaskCanceledException`s aggregation</param>
        /// <param name="tasks">task`s collection</param>
        public static IPandaTask WaitAll( CancellationStrategy strategy, IEnumerable< IPandaTask > tasks )
        {
            return new WhenAllPandaTask( tasks, strategy );
        }

        /// <summary>
        /// Creates task for waiting first completed of rejected of tasks.
        /// </summary>
        public static IPandaTask WaitAny( params IPandaTask[ ] tasksCollection )
        {
            return new WhenAnyPandaTask( tasksCollection );
        }

        /// <summary>
        /// Creates task for waiting first completed of rejected of tasks
        /// </summary>
        public static IPandaTask WaitAny( IEnumerable< IPandaTask > tasksCollection )
        {
            return new WhenAnyPandaTask( tasksCollection );
        }

        /// <summary>
        /// Returns task waiting for specified condition to become false.
        /// </summary>
        /// <param name="condition">Condition to check</param>
        /// <exception cref="ArgumentNullException">Thrown if condition is null</exception>
        public static IPandaTask WaitWhile( Func< bool > condition )
        {
            return WaitWhile( condition, new CancellationToken() );
        }

        /// <summary>
        /// Returns task waiting for specified condition to become false.
        /// </summary>
        /// <param name="condition">Condition to check</param>
        /// <param name="token">Cancellation token</param>
        /// <exception cref="ArgumentNullException">Thrown if condition is null</exception>
        public static IPandaTask WaitWhile( Func<bool> condition, CancellationToken token )
        {
            if( condition == null )
            {
                throw new ArgumentNullException( nameof(condition), "Condition is null" );
            }

            if( token.IsCancellationRequested )
            {
                return CanceledTask;
            }

            return new WaitWhilePandaTask( condition, token );
        }

        /// <summary>
        /// Creates a task that will be completed after the specified amout of milliseconds
        /// </summary>
        /// <param name="milliseconds">Milliseconds to wait</param>
        /// <exception cref="ArgumentException">Thrown if time is negative</exception>
        public static IPandaTask Delay( int milliseconds )
        {
            return Delay( milliseconds, new CancellationToken() );
        }

        /// <summary>
        /// Creates a task that will be completed after the specified amount of time
        /// </summary>
        /// <param name="time">Time to wait</param>
        /// <exception cref="ArgumentException">Thrown if time is negative</exception>
        public static IPandaTask Delay( TimeSpan time )
        {
            return Delay( time, new CancellationToken() );
        }

        /// <summary>
        /// Creates a task that will be completed after the specified amout of milliseconds
        /// </summary>
        /// <param name="milliseconds">Milliseconds to wait</param>
        /// <param name="token">Cancellation token</param>
        /// <exception cref="ArgumentException">Thrown if time is negative</exception>
        public static IPandaTask Delay( int milliseconds, CancellationToken token )
        {
            return Delay( TimeSpan.FromMilliseconds( milliseconds ), token );
        }

        /// <summary>
        /// Creates a task that will be completed after the specified amount of time
        /// </summary>
        /// <param name="time">Time to wait</param>
        /// <param name="token">Cancellation token</param>
        /// <exception cref="ArgumentException">Thrown if time is negative</exception>
        public static IPandaTask Delay( TimeSpan time, CancellationToken token )
        {
            if( time.Ticks < 0 )
            {
                throw new ArgumentException( "Time can not be negative", nameof( time ) );
            }

            if( token.IsCancellationRequested )
            {
                return CanceledTask;
            }

            return new DelayPandaTask( time, token );
        }

        /// <summary>
        /// Helps to get a <see cref="IPandaTask" /> from callback based async method.
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="resultTask">Resulting task that you will await</param>
        /// <returns>Callback to pass as completion handler</returns>
        public static Action< T > CallbackTask< T >( out IPandaTask< T > resultTask )
        {
            var tcs = new PandaTaskCompletionSource< T >();
            resultTask = tcs.ResultTask;
            return tcs.SetValue;
        }

        /// <summary>
        /// Helps to get a <see cref="IPandaTask" /> from callback based async method.
        /// </summary>
        /// <param name="resultTask">Resulting task that you will await</param>
        /// <returns>Callback to pass as completion handler</returns>
        public static Action CallbackTask( out IPandaTask resultTask )
        {
            var tcs = new PandaTaskCompletionSource();
            resultTask = tcs.Task;
            return tcs.Resolve;
        }

        /// <summary>
        /// Return exception to stack. Must be in Unity thread!
        /// </summary>
        /// <param name="task">source task</param>
        /// <param name="ignoreCancel">ignore OperationCanceledException</param>
        public static void RethrowError( this IPandaTask task, bool ignoreCancel = false )
        {
            //throw
            task.Fail( x =>
            {
                //not throw if no task canceled exception mode.
                if( ignoreCancel && x is OperationCanceledException )
                {
                    return;
                }
				
                //throw to stack.
                task.ThrowIfError();
            } );
        }

        /// <summary>
        /// Adds timeout handling for given task. If it does not complete within given time an exception will be thrown
        /// </summary>
        /// <param name="task">Source task</param>
        /// <param name="milliseconds">Timeout in milliseconds</param>
        /// <param name="timeoutMessage">Message for exception</param>
        /// <exception cref="ArgumentException">Thrown if time is negative</exception>
        /// <exception cref="TimeoutException" />
        public static IPandaTask OrTimeout( this IPandaTask task, int milliseconds, string timeoutMessage = null )
        {
            return OrTimeout( task, TimeSpan.FromMilliseconds( milliseconds ), timeoutMessage );
        }

        /// <summary>
        /// Adds timeout handling for given task. If it does not complete within given time an exception will be thrown
        /// </summary>
        /// <param name="task">Source task</param>
        /// <param name="milliseconds">Timeout in milliseconds</param>
        /// <param name="timeoutMessage">Message for exception</param>
        /// <exception cref="ArgumentException">Thrown if time is negative</exception>
        /// <exception cref="TimeoutException" />
        public static IPandaTask< T > OrTimeout< T >( this IPandaTask< T > task, int milliseconds, string timeoutMessage = null )
        {
            return OrTimeout( task, TimeSpan.FromMilliseconds( milliseconds ), timeoutMessage );
        }

        /// <summary>
        /// Adds timeout handling for given task. If it does not complete within given time an exception will be thrown
        /// </summary>
        /// <param name="task">Source task</param>
        /// <param name="timeout">Timeout time</param>
        /// <param name="timeoutMessage">Message for exception</param>
        /// <exception cref="ArgumentException">Thrown if time is negative</exception>
        /// <exception cref="TimeoutException" />
        public static IPandaTask OrTimeout( this IPandaTask task, TimeSpan timeout, string timeoutMessage = null )
        {
            if( task.Status != PandaTaskStatus.Pending )
                return task;

            var delay = Delay( timeout );
            var t = new WhenAnyPandaTask( new[] { task, delay } );

            return t.Then( completedTask =>
            {
                if( completedTask == delay )
                {
                    throw new TimeoutException( ( timeoutMessage ?? $"Task timeout" ) + $" {timeout.TotalSeconds} s" );
                }

                return task;
            } );
        }

        /// <summary>
        /// Adds timeout handling for given task. If it does not complete within given time an exception will be thrown
        /// </summary>
        /// <param name="task">Source task</param>
        /// <param name="timeout">Timeout time</param>
        /// <param name="timeoutMessage">Message for exception</param>
        /// <exception cref="ArgumentException">Thrown if time is negative</exception>
        /// <exception cref="TimeoutException" />
        public static IPandaTask< T > OrTimeout< T >( this IPandaTask< T > task, TimeSpan timeout, string timeoutMessage = null )
        {
            if( task.Status != PandaTaskStatus.Pending )
                return task;

            var delay = Delay( timeout );
            var t = new WhenAnyPandaTask( new[] { task, delay } );

            return t.Then( completedTask =>
            {
                if( completedTask == delay )
                {
                    throw new TimeoutException( ( timeoutMessage ?? $"Task timeout" ) + $" {timeout.TotalSeconds} s" );
                }

                return task;
            } );
        }
        #endregion
    }
}
