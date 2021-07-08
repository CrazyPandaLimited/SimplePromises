using System;
using UnityEngine;

using System.Collections.Generic;


namespace CrazyPanda.UnityCore.PandaTasks.Progress
{
	public sealed class CompositeProgressTracker : IProgressSource< float >
	{
		private IEnumerable< IProgressSource< float > > _trackersCollection;
		private int _count;

        public CompositeProgressTracker( IEnumerable< IProgressSource< float > > trackersCollection, Action< float > onProgressChanged )
        {
            OnProgressChanged = onProgressChanged;
            Initialize( trackersCollection );
        }
        
		public CompositeProgressTracker( IEnumerable< IProgressSource< float > > trackersCollection )
		{
            Initialize( trackersCollection );
		}

		public float Progress { get; private set; }

		public event Action< float > OnProgressChanged;

        private void Initialize( IEnumerable< IProgressSource< float > > trackersCollection )
        {
            //add events
            _trackersCollection = trackersCollection ?? throw new ArgumentNullException( nameof(trackersCollection) );

            foreach( IProgressSource< float > tracker in _trackersCollection )
            {
                _count++;
                tracker.OnProgressChanged += HandleProgressChanged;
            }

            //start update progress
            UpdateProgress();
        }
        
		/// <summary>
		/// Handler for progress updated
		/// </summary>
		private void HandleProgressChanged( float e )
		{
			UpdateProgress();
		}
        
		/// <summary>
		/// Update progress 
		/// </summary>
		private void UpdateProgress()
		{
			//get total progress
			float totalProgress = 0;
			foreach( IProgressSource< float > tracker in _trackersCollection )
			{
				totalProgress += tracker.Progress;
			}

			//get normalized progress
			totalProgress = _count > 0 ? Mathf.Clamp01( totalProgress / _count ) : 1f;

			//set progress if changed
			if( !Mathf.Approximately( Progress, totalProgress ) )
			{
				Progress = totalProgress;
				OnProgressChanged?.Invoke( Progress );
			}
		}
	}
}
