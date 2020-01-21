using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// Async version of <see cref="TestCaseSourceAttribute"/>. Use this Attribute on async tests
    /// </summary>
    public class AsyncTestCaseSourceAttribute : TestCaseSourceAttribute, ITestBuilder
    {
        public AsyncTestCaseSourceAttribute( string sourceName )
            : base( sourceName )
        {
        }

        public AsyncTestCaseSourceAttribute( Type sourceType )
            : base( sourceType )
        {
        }

        public AsyncTestCaseSourceAttribute( Type sourceType, string sourceName )
            : base( sourceType, sourceName )
        {
        }

        public AsyncTestCaseSourceAttribute( Type sourceType, string sourceName, object[] methodParams )
            : base( sourceType, sourceName, methodParams )
        {
        }

        public new IEnumerable<TestMethod> BuildFrom( IMethodInfo method, Test suite )
        {
            var asyncTest = method.GetCustomAttributes<AsyncTestAttribute>( true ).FirstOrDefault();

            if( asyncTest == null )
            {
                var ret = new TestMethod( method, suite );

                ret.RunState = RunState.NotRunnable;
                ret.Properties.Set( "_SKIPREASON", "Method marked with AsyncTestCaseSource must have AsyncTest also applied" );

                yield return ret;
                yield break;
            }

            var tests = base.BuildFrom( method, suite );
            var builder = new NUnitTestCaseBuilder();

            foreach( var m in tests )
            {
                m.parms.HasExpectedResult = true;
                var newMethod = builder.BuildTestMethod( new EnumeratorTaskWrapper( m.Method, asyncTest.Timeout ), suite, m.parms );
                m.parms.HasExpectedResult = false;
                yield return newMethod;
            }
        }
    }
}