using System;
using System.Threading.Tasks;


namespace CrazyPanda.UnityCore.PandaTasks
{
	public sealed class PandaTaskCompletionSource< TResult >
	{
		#region Private Fields
		private readonly PandaTask< TResult > _controlledTask;
		#endregion

		#region Constructor
		public PandaTaskCompletionSource()
		{
			_controlledTask = new PandaTask< TResult >();
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Controlled Task
		/// </summary>
		public IPandaTask< TResult > ResultTask => _controlledTask;
		#endregion

		#region Public Fields
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
			_controlledTask.Reject( ex );
		}

		/// <summary>
		/// Complete task with error without reason
		/// </summary>
		public void SetError()
		{
			_controlledTask.Reject();
		}

		/// <summary>
		/// Complete task with success
		/// </summary>
		public void SetValue( TResult value )
		{
			_controlledTask.SetValue( value );
		}

		/// <summary>
		/// Cancel task with TaskCanceled exception
		/// </summary>
		public void CancelTask()
		{
			_controlledTask.Reject( new TaskCanceledException() );
		}
		#endregion
	}
}
