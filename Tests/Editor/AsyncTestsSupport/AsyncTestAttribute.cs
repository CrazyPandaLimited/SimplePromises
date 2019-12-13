using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using NUnit.Framework.Internal.Commands;
using System;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// Marks method to be treated as async test.
    /// The method must return <see cref="IPandaTask"/> or <see cref="Task"/>.
    /// Result of the returned task affects test result.
    /// You can specify timeout after which test will be marked failed. By default it is set to 10 seconds.
    /// This may be usefull for occasions of very long requests, not called callbacks, etc.
    /// This way the whole test run will not be stuck waiting for a single test
    /// </summary>
    [ AttributeUsage( AttributeTargets.Method, AllowMultiple = false ) ]
    public class AsyncTestAttribute : Attribute, ISimpleTestBuilder, IWrapTestMethod, IImplyFixture
    {
        /// <summary>
        /// Test timeout.
        /// If test does not finish within the given time it will be marked failed.
        /// </summary>
        public TimeSpan Timeout { get; }

        /// <summary>
        /// Default constructor.
        /// Creates test with timeout of 10 seconds.
        /// </summary>
        public AsyncTestAttribute()
            : this( TimeSpan.FromSeconds( 10 ) )
        {
        }

        /// <summary>
        /// Creates test with specified timeout.
        /// </summary>
        /// <param name="timeout">Test will fail after given time</param>
        public AsyncTestAttribute( TimeSpan timeout )
        {
            Timeout = timeout;
        }

        TestMethod ISimpleTestBuilder.BuildFrom( IMethodInfo method, Test suite )
        {
            var parms = new TestCaseParameters() { HasExpectedResult = true };
            var ret = new NUnitTestCaseBuilder().BuildTestMethod( method, suite, parms );
            ret.parms.HasExpectedResult = false;

            if( !typeof( IPandaTask ).IsAssignableFrom( method.ReturnType.Type ) && !typeof( Task ).IsAssignableFrom( method.ReturnType.Type ) )
            {
                ret.RunState = RunState.NotRunnable;
                ret.Properties.Set( "_SKIPREASON", "Method marked with AsyncTest must return either IPandaTask or System.Threading.Task" );
            }

            return ret;
        }

        TestCommand ICommandWrapper.Wrap( TestCommand command )
        {
            return new AsyncTestCommand( command.Test, Timeout );
        }
    }
}