﻿using System;

namespace CrazyPanda.UnityCore.PandaTasks.Progress
{
	public sealed class ProgressTracker< T > : IProgressTracker< T > where T : struct
	{
		#region Private Fields
		private T _progress;
		#endregion

		#region Public Events
		public event Action< T > OnProgressChanged;
		#endregion

		#region Constructor
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
                OnProgressChanged += ( progress ) => handler( progress );
            }

            //set start progress
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
					OnProgressChanged?.Invoke( _progress );
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
