using System;


namespace CrazyPanda.UnityCore.PandaTasks.Progress
{
	public interface IProgressSource< T > where T : struct
	{
		#region Public Events
		/// <summary>
		/// Rise after progress changed
		/// </summary>
		event EventHandler< ProgressChangedEventArgs< T > > OnProgressChanged;
		#endregion

		#region Public Members
		/// <summary>
		/// Get current progress of tracker
		/// </summary>
		T Progress { get; }
		#endregion
	}
}
