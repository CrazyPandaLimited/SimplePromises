namespace CrazyPanda.UnityCore.PandaTasks
{
	/// <summary>
	/// Available status for PandaTask
	/// </summary>
	public enum PandaTaskStatus
	{
		/// <summary>
		/// Waiting for resolve task status
		/// </summary>
		Pending = 0,

		/// <summary>
		/// Task completed with error status
		/// </summary>
		Rejected = 1,

		/// <summary>
		/// Task successful completed status
		/// </summary>
		Resolved = 2,
	}
}
