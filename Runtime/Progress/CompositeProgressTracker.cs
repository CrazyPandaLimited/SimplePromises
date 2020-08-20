using System;
using UnityEngine;

using System.Collections.Generic;


namespace CrazyPanda.UnityCore.PandaTasks.Progress
{
	public sealed class CompositeProgressTracker : IProgressSource< float >
	{
		#region Private Fields
		private readonly IEnumerable< IProgressSource< float > > _trackersCollection;
		private readonly int _count;
		#endregion

		#region Constructors
		public CompositeProgressTracker( IEnumerable< IProgressSource< float > > trackersCollection )
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
		#endregion

		#region Public Properties
		public float Progress { get; private set; }
		#endregion

		#region Public Events
		public event EventHandler< ProgressChangedEventArgs< float > > OnProgressChanged = delegate { };
		#endregion

		#region Private Members
		/// <summary>
		/// Handler for progress updated
		/// </summary>
		private void HandleProgressChanged( object sender, ProgressChangedEventArgs< float > e )
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
			totalProgress = Mathf.Clamp01( totalProgress / _count );

			//set progress if changed
			if( !Mathf.Approximately( Progress, totalProgress ) )
			{
				Progress = totalProgress;
				OnProgressChanged( this, new ProgressChangedEventArgs< float >( Progress ) );
			}
		}
		#endregion
	}
}
