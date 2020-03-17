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
            canceledTask.Reject( new TaskCanceledException() );
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
        /// Creates task for waiting all tasks
        /// </summary>
        public static IPandaTask WaitAll( params IPandaTask[ ] tasks )
        {
            return new WhenAllPandaTask( tasks );
        }

        /// <summary>
        /// Creates task for waiting all tasks
        /// </summary>
        public static IPandaTask WaitAll( IEnumerable< IPandaTask > tasksCollection )
        {
            return new WhenAllPandaTask( tasksCollection );
        }

        /// <summary>
        /// Creates task for waiting first completed of rejected of tasks
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
        /// Returns task waiting for specified condition to become true.
        /// </summary>
        /// <param name="condition">Condition to check</param>
        /// <exception cref="ArgumentNullException">Thrown if condition is null</exception>
        public static IPandaTask WaitWhile( Func<bool> condition )
        {
            return WaitWhile(condition, new CancellationToken() );
        }

        /// <summary>
        /// Returns task waiting for specified condition to become true.
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

            var task = new PandaTask();

            // this function will post itself to current SynchronizationContext until the time passes
            // UnitySynchronizationContext will process posted tasks each frame so this is equivalent to Update()
            void Ticker(object t)
            {
                try
                {
                    if(token.IsCancellationRequested)
                        (t as PandaTask).Reject();
                    else if( !condition() )
                        (t as PandaTask).Resolve();
                    else
                        SynchronizationContext.Current.Post( Ticker, t );
                }
                catch( Exception ex )
                {
                    (t as PandaTask).Reject( ex );
                }
            }

            Ticker(task);
            return task;
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
            if(time.Ticks < 0)
            {
                throw new ArgumentException("Time can not be negative", nameof(time));
            }

            var start = DateTime.Now;
            var task = new PandaTask();

            // this function will post itself to current SynchronizationContext until the time passes
            // UnitySynchronizationContext will process posted tasks each frame so this is equivalent to Update()
            void Ticker( object t )
            {
                var now = DateTime.Now;

                if( token.IsCancellationRequested)
                    (t as PandaTask).Reject();
                if( now - start >= time )
                    (t as PandaTask).Resolve();
                else
                    SynchronizationContext.Current.Post( Ticker, t );
            }

            Ticker(task);
            return task;
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
        #endregion
    }
}
