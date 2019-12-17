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

            // we should run all test code in the same thread because it may access some Unity stuff
            // so we use our custom SynchronizationContext to be able to control all continuations
            // this context will restore previous one on disposal
            using( var newSyncContext = new TestSynchronizationContext() )
            {
                var ret = _method.Invoke( context.TestObject, _arguments );

                // wrap our async test inside a System.Threading.Task, so all continuations will go through our SynchronizationContext
                var waitTask = CreateWaitTask( ret );

                if( waitTask != null )
                {
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    SpinWait spin = default;

                    // wait till task completes our time runs out
                    while( !waitTask.IsCompleted && sw.Elapsed < _timeout )
                    {
                        // fire all pending tasks
                        newSyncContext.Tick();
                        spin.SpinOnce();
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

            return context.CurrentResult;
        }

        private Task CreateWaitTask( object ret )
        {
            switch( ret )
            {
                case IPandaTask pandaTask:
                    Func<Task> wrap = async () => await pandaTask;
                    return wrap();
                case Task task:
                    return task;
                default:
                    return null;
            }
        }
    }
}