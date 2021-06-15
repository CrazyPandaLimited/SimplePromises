using System;
using System.Threading.Tasks;


namespace CrazyPanda.UnityCore.PandaTasks
{
	public class PandaTaskCompletionSource
	{
		private readonly PandaTask _controledTask = new PandaTask();

		/// <summary>
		/// Task associated with CompletionSource
		/// </summary>
		public IPandaTask Task => _controledTask;

		/// <summary>
		/// Complete task with error
		/// </summary>
		public void SetError( Exception ex )
		{
			//check argument
			if( ex == null )
			{
				throw new ArgumentNullException( nameof(ex) );
			}

			//set error
			_controledTask.Reject( ex );
		}

		/// <summary>
		/// Complete task with error without reason
		/// </summary>
		public void SetError()
		{
			_controledTask.Reject();
		}

		/// <summary>
		/// Complete task with success
		/// </summary>
		public void Resolve()
		{
			_controledTask.Resolve();
		}

		/// <summary>
		/// Cancel task with TaskCanceled exception
		/// </summary>
		public void CancelTask()
		{
			_controledTask.Reject( new TaskCanceledException() );
		}
	}
}
