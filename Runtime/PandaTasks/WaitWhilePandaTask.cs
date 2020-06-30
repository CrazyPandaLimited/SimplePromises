using System;
using System.Threading;

namespace CrazyPanda.UnityCore.PandaTasks
{
    class WaitWhilePandaTask : PandaTask
    {
        private readonly Func< bool > _condition;

        public WaitWhilePandaTask( Func< bool > condition, CancellationToken cancellationToken )
        {
            _condition = condition;

            cancellationToken.Register( TryCancel );
            Tick( this );
        }

        private static void Tick( object t )
        {
            // this function will post itself to current SynchronizationContext while _condition is true
            // UnitySynchronizationContext will process posted tasks each frame so this is equivalent to Update()

            var task = t as WaitWhilePandaTask;
            try
            {
                if( task.Status == PandaTaskStatus.Pending )
                {
                    if( !task._condition() )
                        task.Resolve();
                    else
                        SynchronizationContext.Current.Post( Tick, task );
                }
            }
            catch( Exception ex )
            {
                task.Reject( ex );
            }
        }
    }
}
