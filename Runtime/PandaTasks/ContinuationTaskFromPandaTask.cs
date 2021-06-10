using System;
using System.Diagnostics;


namespace CrazyPanda.UnityCore.PandaTasks
{
    [ DebuggerNonUserCode ]
	internal sealed class ContinuationTaskFromPandaTask : PandaTask
	{
		#region Public Fields
		internal readonly bool FromCatch;
		#endregion

		#region Private Fields
		private readonly Func< IPandaTask > _nextActionDelegate;
		private ContinuationTaskState _state = ContinuationTaskState.WaitFirstComplete;
		#endregion

		#region Constructor
		internal ContinuationTaskFromPandaTask( IPandaTask currentPandaTask, Func< IPandaTask > nextAction, bool fromCatch = false )
		{
            //check - set
            if (currentPandaTask == null)
            {
                throw new ArgumentNullException(nameof(currentPandaTask));
            }

            FromCatch = fromCatch;
            _nextActionDelegate = nextAction ?? throw new ArgumentNullException( nameof(nextAction) );

			//start listen task. 
            switch( Status )
            {
                   case PandaTaskStatus.Pending: currentPandaTask.Done( HandleTaskCompleted ).Fail( HandleTaskFailed ); break;

                   //no allocations for completed tasks
                   case PandaTaskStatus.Rejected: HandleTaskFailed( currentPandaTask.Error ); break;
                   case PandaTaskStatus.Resolved: HandleTaskCompleted(); break;
            }
		}
		#endregion

		#region Public Fields
		public override void Dispose()
		{
			//to prevent supported state after dispose
			_state = ContinuationTaskState.Completed;
			base.Dispose();
		}
		#endregion

		#region Internal
		/// <summary>
		/// Resolve for continuation task unavailable
		/// </summary>
		internal override void Resolve()
		{
			throw new InvalidOperationException( @"Impossible to resolve continuation task" );
		}
		#endregion
		
		#region Private Members
		/// <summary>
		/// Handler for task fail
		/// </summary>
		/// <param name="exception">Error in task</param>
		private void HandleTaskFailed( Exception exception )
		{
			//check state
			CheckWaitingState();

			if( _state == ContinuationTaskState.WaitFirstComplete && FromCatch )
			{
				InitNextTask();
			}
			else
			{
				RejectInternal( exception );
			}
		}

		/// <summary>
		/// Handle task complete
		/// </summary>
		private void HandleTaskCompleted()
		{
			//check state
			CheckWaitingState();

			if( _state == ContinuationTaskState.WaitSecondComplete || FromCatch )
			{
				//if continuation task completed 
				ResolveInternal();
			}
			else
			{
				InitNextTask();
			}	
		}

		/// <summary>
		/// Resolve task safe
		/// </summary>
		private void InitNextTask()
		{
			//try get next task
			try
			{
				IPandaTask continuationPandaTask = _nextActionDelegate();

				//no task resolved
				if( continuationPandaTask != null )
				{
					_state = ContinuationTaskState.WaitSecondComplete; 
					continuationPandaTask.Done( HandleTaskCompleted ).Fail( HandleTaskFailed );
				}
				else
				{
					RejectInternal( new NullReferenceException( @"Continuation callback returns null task!" ) );
				}
			}
			catch( Exception ex )
			{
                if( Status != PandaTaskStatus.Pending )
                {
                    throw;
                }
                
				//reject on non system exceptions
				RejectInternal( ex );
			}
		}

		/// <summary>
		/// Set task resolved
		/// </summary>
		private void ResolveInternal()
		{
			//if continuation task completed 
			_state = ContinuationTaskState.Completed;
			base.Resolve();
		}

		/// <summary>
		/// Reject Task with state change
		/// </summary>
		/// <param name="exception">reject exception</param>
		private void RejectInternal( Exception exception )
		{
			_state = ContinuationTaskState.Completed;
			Reject( exception );
		}

		/// <summary>
		/// Check for waiting state
		/// </summary>
		private void CheckWaitingState()
		{
			if( _state == ContinuationTaskState.Completed )
			{
				throw new InvalidOperationException( $@"Cannot perform operation in {Status} status" );
			}
		}
		#endregion
	}
}
