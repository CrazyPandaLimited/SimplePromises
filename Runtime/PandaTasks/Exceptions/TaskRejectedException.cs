﻿using System;


namespace CrazyPanda.UnityCore.PandaTasks
{
	/// <summary>
	/// Default exception for rejecting task
	/// </summary>
	public sealed class TaskRejectedException : OperationCanceledException
	{
		public TaskRejectedException( string message ) : base( message )
		{
		}

		public TaskRejectedException( string message, Exception innerException ) : base( message, innerException )
		{
		}
	}
}
