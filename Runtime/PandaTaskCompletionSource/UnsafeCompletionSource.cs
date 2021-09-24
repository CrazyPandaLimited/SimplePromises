using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// Structure that wraps <see cref="IPandaTask"/> instance. Construct new instances with <see cref="Create"/>.
    /// Use it in places where high performance is required to eliminate allocation of additional class instances.
    /// </summary>
    public struct UnsafeCompletionSource : IEquatable<UnsafeCompletionSource>
    {
        private PandaTask _controlledTask;

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
            return new UnsafeCompletionSource
            {
                _controlledTask = new PandaTask()
            };
        }

        /// <summary>
        /// Complete task with error
        /// </summary>
        public void SetError( Exception ex )
        {
            CheckNonDefault();

            //check argument
            if( ex == null )
            {
                throw new ArgumentNullException( nameof(ex) );
            }

            //set error
            _controlledTask.Reject( ex );
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
        /// Complete task with success
        /// </summary>
        public void Resolve()
        {
            CheckNonDefault();

            _controlledTask.Resolve();
        }

        /// <summary>
        /// Cancel task with TaskCanceled exception
        /// </summary>
        public void CancelTask()
        {
            CheckNonDefault();

            _controlledTask.Reject( new TaskCanceledException() );
        }
        
        [ MethodImpl( MethodImplOptions.AggressiveInlining ) ]
        private void CheckNonDefault()
        {
            if( _controlledTask == null )
            {
                throw new InvalidOperationException( $"UnsafeCompletionSource is not initialized. You should call UnsafeCompletionSource.Create to construct it" );
            }
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
