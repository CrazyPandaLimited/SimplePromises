using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.PandaTasks.Progress
{
	public sealed class ProgressTracker< T > : IProgressTracker< T > where T : struct
	{
		private T _progress;

		public event Action< T > OnProgressChanged;

        public ProgressTracker()
        {
            
        }

        public ProgressTracker( T startValue )
        {
            //set start progress
            Progress = startValue;
        }

        public ProgressTracker( T startValue, Action< T > handler ) : this( startValue )
        {
            //add handler if exist
            if( handler != null )
            {
                OnProgressChanged = ( progress ) => handler( progress );
            }

            //set start progress
        }
        

		public T Progress
		{
			get => _progress;
			private set
			{
				if( !EqualityComparer< T >.Default.Equals( value, _progress ) )
				{
					_progress = value;
					OnProgressChanged?.Invoke( _progress );
				}
			}
		}

		public void ReportProgress( T value )
		{
			Progress = value;
		}
	}
}
