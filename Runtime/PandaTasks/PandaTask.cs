using System;
using System.Diagnostics;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.PandaTasks
{
    [ DebuggerNonUserCode ]
	public class PandaTask : CustomYieldInstruction, IPandaTask
	{
		private Action _doneAction;
		private Action< Exception > _failAction;
        private ExceptionDispatchInfo _errorInfo;

        public Exception Error => _errorInfo?.SourceException;
		public PandaTaskStatus Status { get; set; }
        public sealed override bool keepWaiting => Status == PandaTaskStatus.Pending;

		public IPandaTask Done( Action completeHandler )
		{
			//check arguments
			if( completeHandler == null )
			{
				throw new ArgumentNullException( nameof(completeHandler) );
			}

			//switch action for status
			switch( Status )
			{
				//for resolved task fire handler sync
				case PandaTaskStatus.Resolved:
					completeHandler();
					break;

				//for pending tasks add to delegates
				case PandaTaskStatus.Pending:
					_doneAction += completeHandler;
					break;
			}

			return this;
		}

		public IPandaTask Fail( Action< Exception > errorHandler )
		{
			//check arguments
			if( errorHandler == null )
			{
				throw new ArgumentNullException( nameof(errorHandler) );
			}

			//switch action for status
			switch( Status )
			{
				//for rejected task fire handler sync
				case PandaTaskStatus.Rejected:
					errorHandler( Error );
					break;

				//for pending tasks add to delegates
				case PandaTaskStatus.Pending:
					_failAction += errorHandler;
					break;
			}

			return this;
		}

		public IPandaTask Then( Func< IPandaTask > onResolved )
		{
			//check arguments
			if( onResolved == null )
			{
				throw new ArgumentNullException( nameof(onResolved) );
			}

			//construct new task
			return new ContinuationTaskFromPandaTask( this, onResolved );
		}

		public IPandaTask< TResult > Then< TResult >( Func< IPandaTask< TResult > > onResolved )
		{
			return new ContinuationTaskFromPandaTask< TResult >( this, onResolved );
		}

		public IPandaTask Then( Action onResolved )
		{
			return Then( () =>
			{
				onResolved();
				return this;
			} );
		}

		public IPandaTask Catch( Func< Exception, IPandaTask > onCatch )
		{
			//check arguments
			if( onCatch == null )
			{
				throw new ArgumentNullException( nameof(onCatch) );
			}

			//construct new task
			return new ContinuationTaskFromPandaTask( this, () => onCatch( Error ), true );
		}

		public IPandaTask Catch( Action< Exception > onCatch )
		{
			return Catch( x =>
			{
				onCatch( x );
				return this;
			} );
		}

		public virtual void Dispose()
		{
			//multiple dispose must be supported by Dispose pattern
			if( Status == PandaTaskStatus.Pending )
			{
				Reject( new ObjectDisposedException( @"Reject Task with dispose" ) );
			}
		}

        /// <summary>
        /// Throw exception if task failed
        /// </summary>
        public void ThrowIfError()
        {
            if( Status == PandaTaskStatus.Rejected )
            {
                _errorInfo.Throw();
            }
        }

		/// <summary>
		/// Complete current task
		/// </summary>
		internal virtual void Resolve()
		{
			//check status
			ValidateStatus( PandaTaskStatus.Pending );

			//complete task
			Status = PandaTaskStatus.Resolved;

            //notify handlers
            _doneAction?.Invoke();
            _doneAction = null;
		}

		/// <summary>
		/// Complete with default error
		/// </summary>
		internal virtual void Reject()
		{
			Reject( new TaskRejectedException( @"Task rejected without reason submission" ) );
		}

		/// <summary>
		/// Complete with error
		/// </summary>
		internal virtual void Reject( Exception ex )
		{
			//check arguments
			if( ex == null )
			{
				throw new ArgumentNullException( nameof(ex) );
			}

			//check status
			ValidateStatus( PandaTaskStatus.Pending );

			//set error
			Status = PandaTaskStatus.Rejected;
            _errorInfo = ExceptionDispatchInfo.Capture( ex );

			//notify complete
			_failAction?.Invoke( Error );
			_failAction = null;
		}

        /// <summary>
        /// Helper method for creating Cancelled task
        /// </summary>
        internal void TryCancel()
        {
            if( Status == PandaTaskStatus.Pending )
            {
                Reject( new TaskCanceledException() );
            }
        }

		/// <summary>
        ///Validate status of task
        /// </summary>
        /// <param name="expectedStatus">expectedStatus</param>
        [ MethodImpl( MethodImplOptions.AggressiveInlining ) ]
        private void ValidateStatus( PandaTaskStatus expectedStatus )
        {
            if( expectedStatus != Status )
            {
                throw new InvalidOperationException( $@"Expect Task state: {expectedStatus} but was: {Status}" );
            }
        }
	}
}