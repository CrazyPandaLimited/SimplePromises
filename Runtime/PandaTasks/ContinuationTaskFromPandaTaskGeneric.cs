using System;
using System.Diagnostics;

namespace CrazyPanda.UnityCore.PandaTasks
{
    [ DebuggerNonUserCode ]
    internal sealed class ContinuationTaskFromPandaTask< TResult > : PandaTask< TResult >
    {
        #region Private Fields
        private readonly IPandaTask _combinedTask;
        private IPandaTask< TResult > _continuationTask;
        #endregion

        #region Constructor
        internal ContinuationTaskFromPandaTask( IPandaTask currentTask, Func< IPandaTask< TResult > > continuationTaskCallback, bool fromReject = false )
        {
            //construct non generic continuation task
            _combinedTask = ConstrcutCombinedTask( currentTask, continuationTaskCallback, fromReject );
            _combinedTask.Fail( Reject ).Done( () => base.SetValue( _continuationTask.Result ) );
        }

        internal ContinuationTaskFromPandaTask( IPandaTask< TResult > currentTask, Func< IPandaTask< TResult > > continuationTaskCallback, bool fromReject = false )
        {
            _combinedTask = ConstrcutCombinedTask( currentTask, continuationTaskCallback, fromReject );
            _combinedTask.Fail( Reject ).Done( () =>
            {
                //this case then ignore catch after success of main task
                TResult result = _continuationTask != null ? _continuationTask.Result : currentTask.Result;
                base.SetValue( result );
            } );
        }
        #endregion

        #region Public Members
        public override void Dispose()
        {
            //dispose call _resultlessTask.Fail
            _combinedTask.Dispose();
        }
        #endregion

        #region Internal Members
        /// <summary>
        /// Resolve for continuation task unavailable
        /// </summary>
        internal override void Resolve()
        {
            throw new InvalidOperationException( @"Impossible to resolve continuation task" );
        }

        /// <summary>
        /// Set value for Continuation task unavailable
        /// </summary>
        internal override void SetValue( TResult value )
        {
            throw new InvalidOperationException( @"Impossible to set value for continuation task" );
        }
        #endregion

        #region Private Members
        private IPandaTask ConstrcutCombinedTask( IPandaTask currentTask, Func< IPandaTask< TResult > > continuationTaskCallback, bool fromReject )
        {
            //check arguments
            if( currentTask == null )
            {
                throw new ArgumentNullException( nameof(currentTask) );
            }

            if( continuationTaskCallback == null )
            {
                throw new ArgumentNullException( nameof(continuationTaskCallback) );
            }

            return new ContinuationTaskFromPandaTask( currentTask, () => _continuationTask = continuationTaskCallback(), fromReject );
        }
        #endregion
    }
}
