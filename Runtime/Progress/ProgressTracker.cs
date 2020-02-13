using System;

namespace CrazyPanda.UnityCore.PandaTasks.Progress
{
	public sealed class ProgressTracker< T > : IProgressTracker< T > where T : struct
	{
		#region Private Fields
		private T _progress;
		#endregion

		#region Public Events
		public event EventHandler< ProgressChangedEventArgs< T > > OnProgressChanged = delegate { };
		#endregion

		#region Constructor
		public ProgressTracker( Action< T > handler = null ) : this( default, handler )
		{
		}

		public ProgressTracker( T startValue, Action< T > handler = null )
		{
			//add handler if exist
			if( handler != null )
			{
				OnProgressChanged += ( sender, args ) => handler( args.progress );
			}

			//set start progress
			Progress = startValue;
		}
		#endregion

		#region Public Fields
		public T Progress
		{
			get => _progress;
			private set
			{
				if( !Equals( value, _progress ) )
				{
					_progress = value;
					OnProgressChanged( this, new ProgressChangedEventArgs< T >( _progress ) );
				}
			}
		}
		#endregion

		#region MyRegion
		public void ReportProgress( T value )
		{
			Progress = value;
		}
		#endregion
	}
}
