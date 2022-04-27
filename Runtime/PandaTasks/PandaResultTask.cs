using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CrazyPanda.UnityCore.PandaTasks
{
	/// <summary>
	/// Class represents future with value type TResult
	/// </summary>
	/// <typeparam name="TResult">Type of future</typeparam>
#pragma warning disable CS0436 // Type conflicts with imported type
    [AsyncMethodBuilder( typeof( PandaTaskMethodBuilder<> ) )]
#pragma warning restore CS0436 // Type conflicts with imported type
    [ DebuggerNonUserCode ]
	public class PandaTask< TResult > : PandaTask, IPandaTask< TResult >
	{
		private TResult _result;

		public TResult Result
		{
			get
			{
				switch( Status )
				{
					//for not resolved task 
					case PandaTaskStatus.Pending: throw new InvalidOperationException( @"Cannot get result in pending status" );

					//for rejected throw same error
					case PandaTaskStatus.Rejected: ThrowIfError(); return default;

					//if have result return result
					case PandaTaskStatus.Resolved: return _result;

					//for new states
					default: throw new InvalidOperationException( @"Unsupported State" );
				}
			}
		}

        internal PandaTask() { }

        public IPandaTask< TResult > FailResult( Action< Exception > errorhandler )
		{
			Fail( errorhandler );
			return this;
		}

		public IPandaTask< TResult > Done( Action< TResult > completeHandler )
		{
			Done( () => completeHandler( Result ) );
			return this;
		}

		public IPandaTask< TResult > Catch( Func< Exception, IPandaTask< TResult > > onCatch )
		{
			return new ContinuationTaskFromPandaTask< TResult >( this, () => onCatch( Error ), true );
		}

		public IPandaTask< TResult > CatchResult( Action< Exception > onCatch )
		{
			return Catch( ex =>
			{
				onCatch( ex );
				return this;
			} );
		}

		public IPandaTask Then( Func< TResult, IPandaTask > onResolved )
		{
			return new ContinuationTaskFromPandaTask( this, () => onResolved( Result ) );
		}

		public IPandaTask< TResult > Then( Func< TResult, IPandaTask< TResult > > onResolved )
		{
			return new ContinuationTaskFromPandaTask< TResult >( this, () => onResolved( Result ) );
		}

		public IPandaTask< TResult > ThenResult( Action< TResult > onResolved )
		{
			return Then( res =>
			{
				onResolved( res );
				return this;
			} );
		}

		public IPandaTask< TNewResult > Then< TNewResult >( Func< TResult, IPandaTask< TNewResult > > onResolved )
		{
			return new ContinuationTaskFromPandaTask< TNewResult >( this, () => onResolved( Result ) );
		}

		/// <summary>
		/// Set value for task
		/// </summary>
		/// <param name="value">value to set</param>
		internal virtual void SetValue( TResult value )
		{
			_result = value;
			base.Resolve();
		}

		internal override void Resolve()
		{
			//resolve sets default value no exception
			SetValue( default );
		}
	}
}
