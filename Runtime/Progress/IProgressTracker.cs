namespace CrazyPanda.UnityCore.PandaTasks.Progress
{
	public interface IProgressTracker< T > : IProgressSource< T > where T : struct
	{
		/// <summary>
		/// Set progress to task
		/// </summary>
		/// <param name="value">value</param>
		void ReportProgress( T value );
	}
}
