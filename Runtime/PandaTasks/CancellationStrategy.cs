namespace CrazyPanda.UnityCore.PandaTasks
{
    public enum CancellationStrategy : byte
    {
        /// <summary>
        /// Return aggregate error for task anyway.
        /// </summary>
        Aggregate = 0,

        /// <summary>
        /// Task has TaskCanceledException when all failed tasks has TaskCanceledException.
        /// </summary>
        PartCancel = 1,

        /// <summary>
        /// Task has TaskCanceledException when all tasks has TaskCanceledException.
        /// </summary>
        FullCancel = 2
    }
}