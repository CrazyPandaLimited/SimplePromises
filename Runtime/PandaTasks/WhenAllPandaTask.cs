using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// Task waits for all inner tasks for complete.
    /// If one of task has error it will wait other before set reject status.
    /// </summary>
    internal sealed class WhenAllPandaTask : PandaTask
    {
        #region Private Fields
        private object _error;
        private bool _allFailed;
        private int _waitingCount;

        private readonly CancellationStrategy _strategy;
        #endregion

        #region Constructors
        internal WhenAllPandaTask( IEnumerable< IPandaTask > tasksCollection, CancellationStrategy strategy )
        {
            //check-set
            _strategy = strategy;
            if( tasksCollection == null )
            {
                throw new ArgumentNullException( nameof(tasksCollection) );
            }

            //To protect for first complete task.
            _waitingCount = 1;
            _allFailed = true;
            foreach( IPandaTask task in tasksCollection )
            {
                if( task == null )
                {
                    throw new ArgumentException( @"One of the tasks is null", nameof(tasksCollection) );
                }

                _waitingCount++;
                switch( task.Status )
                {
                    case PandaTaskStatus.Pending:
                        task.Done( CompleteSuccess ).Fail( CompleteWithError );
                        break;
                    case PandaTaskStatus.Rejected:
                        CompleteWithError( task.Error );
                        break;
                    case PandaTaskStatus.Resolved:
                        CompleteSuccess();
                        break;
                }
            }

            CheckCompletion();
        }
        #endregion

        #region Private Members
        internal override void Resolve()
        {
            throw new InvalidOperationException( @"ThenAllPandaTask cannot be resolved" );
        }

        /// <summary>
        /// Handle for error task complete
        /// </summary>
        private void CompleteWithError( Exception error )
        {
            //add error
            switch( _error )
            {
                case Exception oldError:
                    _error = new List< Exception > { error, oldError };
                    break;
                case List< Exception > errorsList:
                    errorsList.Add( error );
                    break;
                case null:
                    _error = error;
                    break;
            }

            //check complete
            CheckCompletion();
        }

        /// <summary>
        /// Complete task with end check
        /// </summary>
        private void CompleteSuccess()
        {
            _allFailed = false;
            CheckCompletion();
        }

        /// <summary>
        /// Check if all tasks is completed.
        /// </summary>
        private void CheckCompletion()
        {
            _waitingCount--;
            if( Status == PandaTaskStatus.Pending && _waitingCount == 0 )
            {
                switch( _error )
                {
                    case null:
                        base.Resolve();
                        break;
                    case Exception error:
                        bool aggregate = CanAggregate() && error is TaskCanceledException;
                        var rejectError = aggregate ? new TaskCanceledException() : (Exception)new AggregateException( error );
                        Reject( rejectError );
                        break;
                    case List< Exception > errorsList:
                        aggregate = CanAggregate() && errorsList.All( x => x is TaskCanceledException );
                        rejectError = aggregate ? new TaskCanceledException() : (Exception)new AggregateException( errorsList );
                        Reject( rejectError );
                        break;
                }
            }
        }

        [ MethodImpl( MethodImplOptions.AggressiveInlining ) ]
        private bool CanAggregate()
        {
            return _strategy == CancellationStrategy.PartCancel || _strategy == CancellationStrategy.FullCancel && _allFailed;
        }
        #endregion
    }
}