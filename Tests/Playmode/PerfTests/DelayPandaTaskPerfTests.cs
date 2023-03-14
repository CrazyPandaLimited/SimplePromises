using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.TestsPerformance;
using NUnit.Framework;
using Debug = UnityEngine.Debug;

namespace CrazyPanda.UnityCore.PandaTasks.PerfTests
{
    sealed class DelayPandaTaskPerfTests
    {
        [ Test, Category("Performance") ]
        public void PandaTaskDelay_Perf_Test()
        {
#if !UNITY_EDITOR && !UNITY_WEBGL && UNITY_2020_2_OR_NEWER
            UnityEngine.Scripting.GarbageCollector.GCMode = UnityEngine.Scripting.GarbageCollector.Mode.Disabled;
#endif
            using( var syncContext = UnitySynchronizationContext.CreateSynchronizationContext() )
            {
                Stopwatch stopwatch = new Stopwatch();
                var step = 0;

                const int tasksCountToCreate = 500;
                const int taskStep = 30;

                var list = new List< PandaTask >( tasksCountToCreate );

                for( int i = 0; i < tasksCountToCreate; i++ )
                {
                    step += taskStep;
                    list.Add( PandaTasksUtilities.Delay( TimeSpan.FromMilliseconds( step ) ) );
                }

                var timeLeft = TimeSpan.Zero;
                int updatesCounter = 0;
                var taskToWait = PandaTasksUtilities.WaitAll( list );

                while( taskToWait.Status == PandaTaskStatus.Pending )
                {
                    if( syncContext.HasPendingTasks() )
                    {
                        var elapsedTime = stopwatch.Elapsed;

                        stopwatch.Start();
                        syncContext.ExecuteTasks();
                        timeLeft += stopwatch.Elapsed - elapsedTime;
                        stopwatch.Stop();
                        ++updatesCounter;
                    }

                    Thread.Sleep( 10 );
                }

                Debug.Log( $"Updates count: {updatesCounter}" );
                Debug.Log( $"Total time mls: {timeLeft.TotalMilliseconds}" );

                Assert.That( updatesCounter, Is.GreaterThan( 450 ) );
                Assert.That( updatesCounter, Is.LessThan( 550 ) );
                
#if UNITY_EDITOR || UNITY_STANDALONE
                Assert.That( timeLeft.TotalMilliseconds, Is.GreaterThan( 30 ) );
                Assert.That( timeLeft.TotalMilliseconds, Is.LessThan( 80 ) );
#elif UNITY_ANDROID && !UNITY_EDITOR
                Assert.That( timeLeft.TotalMilliseconds, Is.GreaterThan( 100 ) );
                Assert.That( timeLeft.TotalMilliseconds, Is.LessThan( 250 ) );
#endif
            }
#if !UNITY_EDITOR && !UNITY_WEBGL && UNITY_2020_2_OR_NEWER
            UnityEngine.Scripting.GarbageCollector.GCMode = UnityEngine.Scripting.GarbageCollector.Mode.Enabled;
#endif
        }

        [ Test ] [ CrazyPanda.UnityCore.TestsPerformance.Performance ]
        public void PandaTaskDelay_Creation_Perf_Test()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds( 2.5f );

            var report = Measure.Method( () =>
            {
                for( int i = 0; i < 250; ++i )
                {
                    PandaTasksUtilities.Delay( timeSpan );
                }
            } ).WarmUpCount( 5 ).IterationsCount( 500 ).Run();

            if( report.IsIterationAllocsMeasured )
            {
                Assert.That( report.IterationAllocs, Is.EqualTo( 250 ).Within( 2 ) );
            }
        }
    }
}