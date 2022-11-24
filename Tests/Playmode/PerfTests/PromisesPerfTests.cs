using System;
using CrazyPanda.UnityCore.TestsPerformance;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.PerfTests
{
    sealed class PromisesPerfTests
    {
        [ Performance ]
        [ TestCase( false ) ]
        [ TestCase( true ) ]
        public void RethrowError_Perf_Test( bool ignoreCancel )
        {
            var report = Measure.Method( () =>
                                {
                                    var task = new PandaTask();
                                    task.RethrowError( ignoreCancel );
                                } )
                                .WarmUpCount( 10 )
                                .IterationsCount( 5000 )
                                .Run();

            if( report.IsIterationAllocsMeasured )
            {
                Assert.That( report.IterationAllocs, Is.EqualTo( 0 ).Within( 1.5 ) );
            }
        }
    }
}