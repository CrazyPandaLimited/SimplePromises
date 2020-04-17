using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using System.Collections.Generic;
using System.Linq;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// Async version of <see cref="TestCaseAttribute"/>. Use this Attribute on async tests
    /// </summary>
    public class AsyncTestCaseAttribute : TestCaseAttribute, ITestBuilder
    {
        public AsyncTestCaseAttribute( params object[] arguments )
            : base( arguments )
        {
        }

        public AsyncTestCaseAttribute( object arg )
            : base( arg )
        {
        }

        public AsyncTestCaseAttribute( object arg1, object arg2 )
            : base( arg1, arg2 )
        {
        }

        public AsyncTestCaseAttribute( object arg1, object arg2, object arg3 )
            : base( arg1, arg2, arg3 )
        {
        }

        public new IEnumerable<TestMethod> BuildFrom( IMethodInfo method, Test suite )
        {
            var asyncTest = method.GetCustomAttributes<AsyncTestAttribute>( true ).FirstOrDefault();

            if( asyncTest == null )
            {
                var ret = new TestMethod( method, suite );

                ret.RunState = RunState.NotRunnable;
                ret.Properties.Set( "_SKIPREASON", "Method marked with AsyncTestCase must have AsyncTest also applied" );

                yield return ret;
                yield break;
            }

            var baseParameters = BaseGetParametersForTestCase( method );
            var builder = new NUnitTestCaseBuilder();

            baseParameters.HasExpectedResult = true;
            var newMethod = builder.BuildTestMethod( new EnumeratorTaskWrapper( method, asyncTest.Timeout ), suite, baseParameters );

            // Unity takes arguments for test method from OriginalArguments (incorrect) instead of Arguments (correct)
            // We recreate TestCaseParameters, so Arguments from baseParameters become OriginalArguments in newMethod.params
            newMethod.parms = new TestCaseParameters( baseParameters );
            newMethod.parms.HasExpectedResult = false;
            yield return newMethod;
        }

        private TestCaseParameters BaseGetParametersForTestCase( IMethodInfo method )
        {
            // sadly, this method is private, so we have to call it via reflection
            var baseMethod = typeof( TestCaseAttribute ).GetMethod( "GetParametersForTestCase", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );
            var ret = baseMethod.Invoke( this, new[] { method } );

            return ( TestCaseParameters ) ret;
        }
    }
}