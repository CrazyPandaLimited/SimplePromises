#if (UNITY_EDITOR && CRAZYPANDA_UNITYCORE_PROMISES_ENABLE_ASYNC_TESTS_EDITOR) || CRAZYPANDA_UNITYCORE_PROMISES_ENABLE_ASYNC_TESTS_PLAYMODE

using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using NUnit.Framework.Internal.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.TestTools;

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
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public class AsyncTestAttribute : Attribute, ISimpleTestBuilder, IWrapSetUpTearDown, IImplyFixture, IApplyToTest
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
            : this( 10 )
        {
        }

        /// <summary>
        /// Creates test with specified timeout.
        /// </summary>
        /// <param name="timeoutSeconds">Test will fail after given time</param>
        public AsyncTestAttribute( double timeoutSeconds = 10 )
        {
            Timeout = TimeSpan.FromSeconds( timeoutSeconds );
        }

        TestMethod ISimpleTestBuilder.BuildFrom( IMethodInfo method, Test suite )
        {
            var wrapperMethod = new EnumeratorTaskWrapper( method, Timeout );

            // Set HasExpectedResult=true so call to BuildTestMethod will succeed            
            var parms = new TestCaseParameters() { HasExpectedResult = true };
            var ret = new NUnitTestCaseBuilder().BuildTestMethod( wrapperMethod, suite, parms );

            // Set HasExpectedResult=false so it won't fail later because expected result is not same as actual
            ret.parms.HasExpectedResult = false;

            // check return type
            if( !typeof( IPandaTask ).IsAssignableFrom( method.ReturnType.Type ) && !typeof( Task ).IsAssignableFrom( method.ReturnType.Type ) )
            {
                ret.RunState = RunState.NotRunnable;
                ret.Properties.Set( "_SKIPREASON", "Method marked with AsyncTest must return either IPandaTask or System.Threading.Task" );
            }
            
            return ret;
        }

        TestCommand ICommandWrapper.Wrap( TestCommand command )
        {
#if UNITY_2019_3_OR_NEWER
            return command; // no longer need to wrap anything in 2019.3.x
#else
            return new UnityTestAttribute().Wrap( command );
#endif
        }

        void IApplyToTest.ApplyToTest( Test test )
        {
            // check if test cases are built using supported attributes
            if( HasAttribute< TestCaseAttribute >( test ) )
            {
                test.RunState = RunState.NotRunnable;
                test.Properties.Set( "_SKIPREASON", $"Replace TestCase's with AsyncTestCase's, because they are not supported on async tests" );
            }
            else if( HasAttribute< TestCaseSourceAttribute >( test ) )
            {
                test.RunState = RunState.NotRunnable;
                test.Properties.Set( "_SKIPREASON", $"Replace TestCaseSource's with AsyncTestCaseSource's, because they are not supported on async tests" );
            }
        }

        private bool HasAttribute< T >( Test test )
            where T : Attribute
        {
            var attrs = test.Method.GetCustomAttributes< T >( true );
            if( attrs.Length == 0 )
                return false;

            return attrs.Any( a => a.GetType() == typeof( T ) );
        }
    }
}
#else
using System;

namespace CrazyPanda.UnityCore.PandaTasks
{
    [ Obsolete( "You must enable appropriate define in order to use AsyncTestAttribute", true ) ]
    public class AsyncTestAttribute : Attribute
    {
    }
}
#endif