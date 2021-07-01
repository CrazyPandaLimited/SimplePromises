using System;
using System.Threading;

namespace CrazyPanda.UnityCore.PandaTasks
{
    class WaitWhilePandaTask : PandaTask
    {
        private readonly Func< bool > _condition;
        private static readonly SendOrPostCallback _tickCallback = new SendOrPostCallback(Tick);

        public WaitWhilePandaTask( Func< bool > condition, CancellationToken cancellationToken )
        {
            _condition = condition;

            if( cancellationToken.CanBeCanceled )
            {
                cancellationToken.Register( TryCancel );
            }
            
            Tick( this );
        }

        private static void Tick( object t )
        {
            // this function will post itself to current SynchronizationContext while _condition is true
            // UnitySynchronizationContext will process posted tasks each frame so this is equivalent to Update()

            var task = t as WaitWhilePandaTask;
            if( task.Status == PandaTaskStatus.Pending )
            {
                bool willWait = false;

                try
                {
                    willWait = task._condition();
                   
                }
                catch( Exception ex )
                {
                    task.Reject( ex );
                    return;
                }

                if( !willWait )
                    task.Resolve();
                else
                    SynchronizationContext.Current.Post( _tickCallback, task );
            }
        }
    }
}
