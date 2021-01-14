using System;
using System.Threading;

namespace CrazyPanda.UnityCore.PandaTasks
{
    class DelayPandaTask : PandaTask
    {
        private readonly DateTime _endTime;

        public DelayPandaTask( TimeSpan delayTime, CancellationToken cancellationToken )
        {
            _endTime = DateTime.Now + delayTime;

            if( cancellationToken.CanBeCanceled )
            {
                cancellationToken.Register( TryCancel );
            }

            Tick( this );
        }

        private static void Tick( object t )
        {
            // this function will post itself to current SynchronizationContext until the time passes
            // UnitySynchronizationContext will process posted tasks each frame so this is equivalent to Update()

            var delayTask = t as DelayPandaTask;

            if( delayTask.Status == PandaTaskStatus.Pending )
            {
                if( DateTime.Now >= delayTask._endTime )
                    delayTask.Resolve();
                else
                    SynchronizationContext.Current.Post( Tick, t );
            }
        }
    }
}
