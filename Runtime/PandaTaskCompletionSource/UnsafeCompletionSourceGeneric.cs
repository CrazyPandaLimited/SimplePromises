using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.Utils;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// Structure that wraps <see cref="IPandaTask"/> instance. Construct new instances with <see cref="Create"/>.
    /// Use it in places where high performance is required to eliminate allocation of additional class instances.
    /// </summary>
    public readonly struct UnsafeCompletionSource< TResult >
    {
        private readonly PandaTask< TResult > _controlledTask;

        /// <summary>
        /// Task associated with <see cref="UnsafeCompletionSource{TResult}"/>
        /// </summary>
        public IPandaTask< TResult > ResultTask
        {
            get
            {
                CheckNonDefault();
                return _controlledTask;
            }
        }

        /// <summary>
        /// Creates new <see cref="UnsafeCompletionSource{TResult}"/>. Use it instead of new
        /// </summary>
        /// <returns></returns>
        public static UnsafeCompletionSource< TResult > Create()
        {
            return new UnsafeCompletionSource< TResult >( new PandaTask< TResult >() );
        }

        /// <summary>
        /// Creates new <see cref="UnsafeCompletionSource"/>. Use it instead of new
        /// </summary>
        /// <param name="cancellationToken">Token for task cancellation</param>
        /// <returns></returns>
        public static UnsafeCompletionSource<TResult> Create( CancellationToken cancellationToken )
        {
            return new UnsafeCompletionSource< TResult >( cancellationToken );
        }
        
        private UnsafeCompletionSource( PandaTask< TResult > controlledTask )
        {
            _controlledTask = controlledTask;
        }

        private UnsafeCompletionSource( CancellationToken cancellationToken )
        {
            if( cancellationToken.IsCancellationRequested )
            {
                _controlledTask = PandaTasksUtilities.GetCanceledTaskInternal< TResult >();
                return;
            }

            _controlledTask = new PandaTask< TResult >();
            
            if( cancellationToken.CanBeCanceled )
            {
                cancellationToken.Register( TryCancelTaskInternal );
            }
        }
        
        /// <summary>
        /// Complete task with error
        /// </summary>
        public void SetError( Exception ex )
        {
            CheckNonDefault();

            //set error
            _controlledTask.Reject( ex );
        }
        
        /// <summary>
        /// Try to complete task with error without reason
        /// </summary>
        public bool TrySetError(Exception ex )
        {
            if( _controlledTask.Status == PandaTaskStatus.Pending )
            {
                SetError( ex );
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Complete task with error without reason
        /// </summary>
        public void SetError()
        {
            CheckNonDefault();

            _controlledTask.Reject();
        }

        /// <summary>
        /// Try to complete task with error without reason
        /// </summary>
        public bool TrySetError()
        {
            if( _controlledTask.Status == PandaTaskStatus.Pending )
            {
                SetError();
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Complete task with success
        /// </summary>
        public void SetValue( TResult value )
        {
            CheckNonDefault();

            _controlledTask.SetValue( value );
        }

        /// <summary>
        /// Try to complete task with success
        /// </summary>
        public bool TrySetValue( TResult value )
        {
            if( _controlledTask.Status == PandaTaskStatus.Pending )
            {
                this.SetValue( value );
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Cancel task with TaskCanceled exception
        /// </summary>
        public void CancelTask()
        {
            CheckNonDefault();

            _controlledTask.Reject( new TaskCanceledException() );
        }

        /// <summary>
        /// Try to cancel task
        /// </summary>
        public bool TryCancelTask()
        {
            if( _controlledTask.Status == PandaTaskStatus.Pending )
            {
                this.CancelTask();
                return true;
            }

            return false;
        }
        
        [ MethodImpl( MethodImplOptions.AggressiveInlining ) ]
        private void CheckNonDefault()
        {
            if( _controlledTask == null )
            {
                throw new InvalidOperationException( $"UnsafeCompletionSource is not initialized. You should call UnsafeCompletionSource.Create to construct it" );
            }
        }

        private void TryCancelTaskInternal()
        {
            this.TryCancelTask();
        }
    }
}
