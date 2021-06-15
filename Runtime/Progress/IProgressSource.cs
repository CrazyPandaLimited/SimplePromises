using System;


namespace CrazyPanda.UnityCore.PandaTasks.Progress
{
	public interface IProgressSource< T > where T : struct
	{
		/// <summary>
		/// Rise after progress changed
		/// </summary>
		event Action< T > OnProgressChanged;

		/// <summary>
		/// Get current progress of tracker
		/// </summary>
		T Progress { get; }
	}
}
