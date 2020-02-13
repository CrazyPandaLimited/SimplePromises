using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// Return first of task that completed.
    /// When first of inner`s task`s was rejected, WhenAnyPandaTask will be rejected.
    /// </summary>
    [ DebuggerNonUserCode ]
    internal sealed class WhenAnyPandaTask : PandaTask< IPandaTask >
    {
        #region Constructors
        internal WhenAnyPandaTask( IEnumerable< IPandaTask > tasks )
        {
            //check
            if( tasks == null )
            {
                throw new ArgumentNullException( nameof(tasks) );
            }

            //subscribe
            foreach( IPandaTask innerTask in tasks )
            {
                if( innerTask == null )
                {
                    throw new ArgumentException( @"One of tasks is null", nameof(tasks) );
                }

                //for sync complete no allocations
                switch( innerTask.Status )
                {
                    case PandaTaskStatus.Pending:
                        innerTask.Done( () => HandleInnerTaskDone( innerTask ) ).Fail( HandleInnerTaskFailed );
                        break;
                    case PandaTaskStatus.Rejected:
                        HandleInnerTaskFailed( innerTask.Error );
                        break;
                    case PandaTaskStatus.Resolved:
                        HandleInnerTaskDone( innerTask );
                        break;
                }
            }
        }
        #endregion

        #region Internal Members
        internal override void Resolve()
        {
            throw new InvalidOperationException( $@"Impossible to resolve {nameof(WhenAnyPandaTask)}" );
        }

        internal override void Reject( Exception ex )
        {
            throw new InvalidOperationException( $@"Impossible to reject {nameof(WhenAnyPandaTask)}" );
        }

        internal override void SetValue( IPandaTask value )
        {
            throw new InvalidOperationException( $@"Impossible to set value for {nameof(WhenAnyPandaTask)}" );
        }
        #endregion

        #region Private Member
        /// <summary>
        /// Handler for one of task failed
        /// </summary>
        private void HandleInnerTaskFailed( Exception exception )
        {
            //multiple call protect
            if( Status == PandaTaskStatus.Pending )
            {
                base.Reject( exception );
            }
        }

        /// <summary>
        /// Handler for one of task completed
        /// </summary>
        private void HandleInnerTaskDone(IPandaTask task)
        {
            //multiple call protect
            if( Status == PandaTaskStatus.Pending )
            {
                base.SetValue( task );
            }
        }
        #endregion
    }
}
