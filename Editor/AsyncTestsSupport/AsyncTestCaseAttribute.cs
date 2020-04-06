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
            var asyncTest = method.GetCustomAttributes< AsyncTestAttribute >( true ).FirstOrDefault();

            if( asyncTest == null )
            {
                var ret = new TestMethod( method, suite );

                ret.RunState = RunState.NotRunnable;
                ret.Properties.Set( "_SKIPREASON", "Method marked with AsyncTestCase must have AsyncTest also applied" );

                yield return ret;
                yield break;
            }

            var tests = base.BuildFrom( method, suite );
            var builder = new NUnitTestCaseBuilder();

            foreach( var m in tests )
            {
                m.parms.HasExpectedResult = true;
                var newMethod = builder.BuildTestMethod( new EnumeratorTaskWrapper( m.Method, asyncTest.Timeout ), suite, m.parms );

                // Unity takes arguments for test method from OriginalArguments (incorrect) instead of Arguments (correct)
                // We recreate TestCaseParameters, so Arguments from m.params become OriginalArguments in newMethod.params
                newMethod.parms = new TestCaseParameters( m.parms );
                newMethod.parms.HasExpectedResult = false;
                yield return newMethod;
            }
        }
    }
}