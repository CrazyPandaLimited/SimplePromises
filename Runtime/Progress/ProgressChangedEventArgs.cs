using System;


namespace CrazyPanda.UnityCore.PandaTasks.Progress
{
	public sealed class ProgressChangedEventArgs< T > : EventArgs where T : struct
	{
		#region Public Fields
		/// <summary>
		/// Current progress
		/// </summary>
		public readonly T progress;
		#endregion

		#region Constructors
		public ProgressChangedEventArgs( T value )
		{
			progress = value;
		}
		#endregion
	}
}
