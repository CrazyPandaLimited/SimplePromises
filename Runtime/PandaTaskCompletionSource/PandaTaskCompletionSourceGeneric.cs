using System;
using System.Threading;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.Utils;


namespace CrazyPanda.UnityCore.PandaTasks
{
	public sealed class PandaTaskCompletionSource< TResult >
	{
		private readonly PandaTask< TResult > _controlledTask;

        public PandaTaskCompletionSource()
        {
            _controlledTask = new PandaTask< TResult >();
        }
        
        public PandaTaskCompletionSource( CancellationToken cancellationToken )
        {
            if( cancellationToken.IsCancellationRequested )
            {
                _controlledTask = PandaTasksUtilities.GetCanceledTaskInternal< TResult >();
                return;
            }

            _controlledTask = new PandaTask< TResult >();
            
            if( cancellationToken.CanBeCanceled )
            {
                cancellationToken.Register( TryCancelTaskInternal );
            }
        }
        
		/// <summary>
		/// Controlled Task
		/// </summary>
		public IPandaTask< TResult > ResultTask => _controlledTask;

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
		public void SetValue( TResult value )
		{
			_controlledTask.SetValue( value );
		}

        /// <summary>
        /// Try to complete task with success
        /// </summary>
        public bool TrySetValue( TResult value )
        {
            if( _controlledTask.Status == PandaTaskStatus.Pending )
            {
                this.SetValue( value );
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
