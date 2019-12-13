using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.PandaTasks
{
    internal class AsyncTestCommand : TestCommand
    {
        private readonly IMethodInfo _method;
        private readonly object[ ] _arguments;
        private readonly TimeSpan _timeout;

        public AsyncTestCommand( Test test, TimeSpan timeout )
            : base( test )
        {
            _method = test.Method;
            _arguments = (test as TestMethod).parms.Arguments;
            _timeout = timeout;
        }

        public override TestResult Execute( ITestExecutionContext context )
        {
            if( _method == null )
            {
                context.CurrentResult.SetResult( ResultState.Failure, $"TestMethod not set for {Test.MethodName}" );
                return context.CurrentResult;
            }

            var oldSyncContext = SynchronizationContext.Current;
            var newSyncContext = new TestSynchronizationContext();

            try
            {
                // we should run all test code in the same thread because it may access some Unity stuff
                // so we set our custom SynchronizationContext to be able to control all continuations
                SynchronizationContext.SetSynchronizationContext( newSyncContext );

                var ret = _method.Invoke( context.TestObject, _arguments );

                // wrap our async test inside a System.Threading.Task, so all continuations will go through our SynchronizationContext
                var waitTask = CreateWaitTask( ret );

                if( waitTask != null )
                {
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    // wait till task completes our time runs out
                    while( !waitTask.IsCompleted && sw.Elapsed < _timeout )
                    {
                        // fire all pending tasks
                        newSyncContext.Tick();
                        Thread.Sleep( 0 );
                    }

                    sw.Stop();

                    if( waitTask.IsFaulted )
                    {
                        // just throw exception, it will be printed to result by our caller
                        throw waitTask.Exception;
                    }
                    else if( waitTask.IsCompleted )
                    {
                        context.CurrentResult.SetResult( ResultState.Success );
                    }
                    else
                    {
                        context.CurrentResult.SetResult( ResultState.Failure, $"Test didn't complete after timeout of {_timeout.TotalSeconds:#.#} seconds" );
                    }
                }
                else
                {
                    context.CurrentResult.SetResult( ResultState.Failure, "AsyncTest method must return IPandaTask or System.Threading.Task" );
                }
            }
            catch( Exception )
            {
                // we need this catch block to guarantee that finally block will always be called
                throw;
            }
            finally
            {
                // restore default SynchronizationContext
                SynchronizationContext.SetSynchronizationContext( oldSyncContext );
            }

            return context.CurrentResult;
        }

        private Task CreateWaitTask( object ret )
        {
            switch( ret )
            {
                case IPandaTask pandaTask:
                    return Wrap( pandaTask );
                case Task task:
                    return task;
                default:
                    return null;
            }
        }

        private async Task Wrap( IPandaTask task )
        {
            await task;
        }
    }
}