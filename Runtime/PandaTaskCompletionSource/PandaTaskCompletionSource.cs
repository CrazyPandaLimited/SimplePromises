using System;
using System.Threading;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.Utils;


namespace CrazyPanda.UnityCore.PandaTasks
{
	public class PandaTaskCompletionSource
	{
		private readonly PandaTask _controlledTask;

        public PandaTaskCompletionSource()
        {
            _controlledTask = new PandaTask();
        }
        
        public PandaTaskCompletionSource( CancellationToken cancellationToken )
        {
            if( cancellationToken.IsCancellationRequested )
            {
                _controlledTask = PandaTasksUtilities.CanceledTaskInternal;
                return;
            }

            _controlledTask = new PandaTask();
            
            if( cancellationToken.CanBeCanceled )
            {
                cancellationToken.Register( TryCancelTaskInternal );
            }
        }
        
		/// <summary>
		/// Task associated with CompletionSource
		/// </summary>
		public PandaTask Task => _controlledTask;

		/// <summary>
		/// Complete task with error
		/// </summary>
		public void SetError( Exception ex )
		{
			//set error
			_controlledTask.Reject( ex );
		}
        
        /// <summary>
        /// Try to complete task with error without reason
        /// </summary>
        public bool TrySetError(Exception ex )
        {
            if( _controlledTask.Status == PandaTaskStatus.Pending )
            {
                SetError( ex );
                return true;
            }

            return false;
        }

		/// <summary>
		/// Complete task with error without reason
		/// </summary>
		public void SetError()
		{
			_controlledTask.Reject();
		}

        /// <summary>
        /// Try to complete task with error without reason
        /// </summary>
        public bool TrySetError()
        {
            if( _controlledTask.Status == PandaTaskStatus.Pending )
            {
                SetError();
                return true;
            }

            return false;
        }
        
		/// <summary>
		/// Complete task with success
		/// </summary>
		public void Resolve()
		{
			_controlledTask.Resolve();
		}

        /// <summary>
        /// Try to complete task with success
        /// </summary>
        public bool TryResolve()
        {
            if( _controlledTask.Status == PandaTaskStatus.Pending )
            {
                this.Resolve();
                return true;
            }

            return false;
        }

		/// <summary>
		/// Cancel task with TaskCanceled exception
		/// </summary>
		public void CancelTask()
		{
			_controlledTask.Reject( new TaskCanceledException() );
		}
        
        /// <summary>
        /// Try to cancel task
        /// </summary>
        public bool TryCancelTask()
        {
            if( _controlledTask.Status == PandaTaskStatus.Pending )
            {
                this.CancelTask();
                return true;
            }

            return false;
        }

        private void TryCancelTaskInternal()
        {
            this.TryCancelTask();
        }
    }
}
