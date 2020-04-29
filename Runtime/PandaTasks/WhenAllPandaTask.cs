using System;
using System.Collections.Generic;
using System.Linq;
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
        //for safe memory without errors
        private int _failCount;
        private int _waitingCount;
        private readonly CancellationStrategy _strategy;
        private readonly IEnumerable< IPandaTask > _tasksCollection;
        #endregion

        #region Constructors
        internal WhenAllPandaTask( IEnumerable< IPandaTask > tasksCollection, CancellationStrategy strategy )
        {
            //check arguments
            _strategy = strategy;
            _tasksCollection = tasksCollection ?? throw new ArgumentNullException( nameof(tasksCollection) );

            //add handlers
            _waitingCount = 1;
            foreach( IPandaTask task in _tasksCollection )
            {
                //check null
                if( task == null )
                {
                    throw new ArgumentException( @"One of the tasks is null", nameof(tasksCollection) );
                }

                //start wait complete of tasks
                _failCount++;
                _waitingCount++;
                switch( task.Status )
                {
                    case PandaTaskStatus.Pending:
                        task.Done( CompleteTask ).Fail( CompleteWithError );
                        break;
                    case PandaTaskStatus.Rejected:
                        CompleteWithError( task.Error );
                        break;
                    case PandaTaskStatus.Resolved:
                        CompleteTask();
                        break;
                }
            }

            CompleteWithError( null );
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
            //remove task
            _waitingCount--;

            //check complete
            CheckCompletion();
        }

        /// <summary>
        /// Complete task with end check
        /// </summary>
        private void CompleteTask()
        {
            //remove task
            _failCount--;

            //check complete
            CompleteWithError( null );
        }

        private void CheckCompletion()
        {
            if( Status == PandaTaskStatus.Pending && _waitingCount == 0 )
            {
                //to protect from enumerator allocation without errors.
                if( _failCount == 0 )
                {
                    base.Resolve();
                }
                else
                {
                    var errors = _tasksCollection.Select( x => x.Error );
                    switch( _strategy )
                    {
                        case CancellationStrategy.Aggregate:
                            AggregateReject( errors );
                            break;
                        case CancellationStrategy.FullCancel:
                            TryCancel( errors );
                            break;
                        case CancellationStrategy.PartCancel:
                            TryCancel( errors.Where( x => !ReferenceEquals( x, null ) ) );
                            break;
                    }
                }
            }
        }

        private void TryCancel( IEnumerable< Exception > errors )
        {
            if( errors.All( x => x is TaskCanceledException ) )
            {
                Reject( new TaskCanceledException() );
            }
            else
            {
                AggregateReject( errors );
            }
        }

        private void AggregateReject( IEnumerable< Exception > errors )
        {
            var error = new AggregateException( errors.Where( x => !ReferenceEquals( x, null ) ) );
            Reject( error );
        }
        #endregion
    }
}