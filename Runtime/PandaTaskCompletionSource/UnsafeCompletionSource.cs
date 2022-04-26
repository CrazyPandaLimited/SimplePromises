using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.Utils;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// Structure that wraps <see cref="IPandaTask"/> instance. Construct new instances with <see cref="Create"/>.
    /// Use it in places where high performance is required to eliminate allocation of additional class instances.
    /// </summary>
    public readonly struct UnsafeCompletionSource : IEquatable<UnsafeCompletionSource>
    {
        private readonly PandaTask _controlledTask;

        /// <summary>
        /// Task associated with <see cref="UnsafeCompletionSource"/>
        /// </summary>
        public IPandaTask Task
        {
            get
            {
                CheckNonDefault();
                return _controlledTask;
            }
        }

        /// <summary>
        /// Creates new <see cref="UnsafeCompletionSource"/>. Use it instead of new
        /// </summary>
        /// <returns></returns>
        public static UnsafeCompletionSource Create()
        {
            return new UnsafeCompletionSource( new PandaTask() );
        } 
        
        /// <summary>
        /// Creates new <see cref="UnsafeCompletionSource"/>. Use it instead of new
        /// </summary>
        /// <param name="cancellationToken">Token for task cancellation</param>
        /// <returns></returns>
        public static UnsafeCompletionSource Create( CancellationToken cancellationToken )
        {
            return new UnsafeCompletionSource( cancellationToken );
        }

        private UnsafeCompletionSource( PandaTask controlledTask )
        {
            _controlledTask = controlledTask;
        }

        private UnsafeCompletionSource( CancellationToken cancellationToken )
        {
            if( cancellationToken.IsCancellationRequested )
            {
                _controlledTask = PandaTasksUtilities.CanceledTaskInternal;
                return;
            }

            _controlledTask = new PandaTask();
            
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
        public void Resolve()
        {
            CheckNonDefault();

            _controlledTask.Resolve();
        }

        /// <summary>
        /// Try to complete task with success
        /// </summary>
        public bool TryResolve()
        {
            if( _controlledTask.Status == PandaTaskStatus.Pending )
            {
                this.Resolve();
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
        
        public bool Equals( UnsafeCompletionSource other )
        {
            return Equals( _controlledTask, other._controlledTask );
        }

        public override bool Equals( object obj )
        {
            return obj is UnsafeCompletionSource other && Equals( other );
        }

        public override int GetHashCode()
        {
            return ( _controlledTask != null ? _controlledTask.GetHashCode() : 0 );
        }
    }
}
