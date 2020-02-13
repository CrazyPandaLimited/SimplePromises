using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.PandaTasks
{
	/// <summary>
	/// Task waits for all inner tasks for complete.
	/// If one of task has error it will wait other before set reject status.
	/// </summary>
	internal sealed class WhenAllPandaTask : PandaTask
	{
		#region Private Fields
        //for safe memory without errors
        private object _errors;
		private int _waitingCount;
		private readonly bool _isInitialisationComplete;
		#endregion

		#region Constructors
		internal WhenAllPandaTask( IEnumerable< IPandaTask > tasksCollection )
		{
			//check arguments
			if( tasksCollection == null )
			{
				throw new ArgumentNullException( nameof(tasksCollection) );
			}

            //add handlers
            foreach( IPandaTask task in tasksCollection )
            {
                //check null
                if( task == null )
                {
                    throw new ArgumentException( @"One of the tasks is null", nameof(tasksCollection) );
                }

                //start wait complete of tasks
                _waitingCount++;
                switch( task.Status )
                {
                    case PandaTaskStatus.Pending:
                        task.Done( CompleteTask ).Fail( HandleTaskCompleteWithError );
                        break;
                    case PandaTaskStatus.Rejected:
                        HandleTaskCompleteWithError( task.Error );
                        break;
                    case PandaTaskStatus.Resolved:
                        CompleteTask();
                        break;
                }
            }

            _isInitialisationComplete = true;
            CheckCompletion();
        }
		#endregion

		#region Private Members
		internal override void Resolve()
		{
			throw new InvalidOperationException( @"ThenAllPandaTask cannot be resolved" );
		}

        /// <summary>
        /// Handle for error task complete
        /// </summary>
        private void HandleTaskCompleteWithError( Exception error )
        {
            switch( _errors )
            {
                case Exception firstError:
                    _errors = new List< Exception > { firstError, error };
                    break;
                case List< Exception > errorsList:
                    errorsList.Add( error );
                    break;
                case null:
                    _errors = error;
                    break;
            }

            CompleteTask();
        }

		/// <summary>
		/// Complete task with end check
		/// </summary>
		private void CompleteTask()
		{
			//remove task
			_waitingCount--;

			//check complete
			CheckCompletion();
		}

        private void CheckCompletion()
        {
            if( Status == PandaTaskStatus.Pending && _waitingCount == 0 && _isInitialisationComplete )
            {
                switch( _errors )
                {
                    case Exception firstError:
                        Reject( new AggregateException( firstError ) );
                        break;
                    case List< Exception > errorsList:
                        Reject( new AggregateException( errorsList ) );
                        break;
                    case null:
                        base.Resolve();
                        break;
                }
            }
        }
        #endregion
    }
}
