using System;
using System.Collections;
using System.Collections.Generic;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.TestsPerformance;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    class CompositeProgressTrackerPerfTests
    {
        private const uint WarmupCount = 10;
        private const uint IterationsCount = 5000;

        [ Test ] [ Performance ]
        public void Ctor_Perf_Test_List_With_Single_Item()
        {
            var trackersCount = 1;
            Ctor_Perf_Test_With_Collection( new List< IProgressSource< float > >( trackersCount ), trackersCount, trackersCount );
        }

        [ Test ] [ Performance ]
        public void Ctor_Perf_Test_With_List()
        {
            const int trackersCount = 50;
            Ctor_Perf_Test_With_Collection( new List< IProgressSource< float > >( trackersCount ), trackersCount, trackersCount  );
        }

        [ Test ] [ Performance ]
        public void Ctor_Perf_Test_With_HashSet()
        {
            const int trackersCount = 50;
            Ctor_Perf_Test_With_Collection( new HashSet< IProgressSource< float > >(), trackersCount, trackersCount );
        }

        [ Test ] [ Performance ]
        public void Ctor_Perf_Test_HashSet_With_Single_Item()
        {
            const int trackersCount = 1;
            Ctor_Perf_Test_With_Collection( new HashSet< IProgressSource< float > >(), trackersCount, trackersCount );
        }

        [ Test ] [ Performance ]
        public void Ctor_Perf_Test_With_Array()
        {
            const int trackersCount = 50;
            Ctor_Perf_Test_With_Array( new IProgressSource< float >[ trackersCount ], trackersCount, trackersCount );
        }

        [ Test ] [ Performance ]
        public void Ctor_Perf_Test_Array_With_Single_Item()
        {
            const int trackersCount = 1;
            Ctor_Perf_Test_With_Array( new IProgressSource< float >[ trackersCount ], trackersCount, trackersCount );
        }

        [Test, Performance ]
        public void Ctor_Perf_Test_With_Enumerable()
        {
            var trackersCount = 50;
            
            var report = Measure.Method( () =>
                                {
                                    var progressTracker = new CompositeProgressTracker( new MockEnumerable( trackersCount ) );
                                } )
                                .WarmUpCount( WarmupCount )
                                .IterationsCount( IterationsCount )
                                .Run();

            if( report.IsIterationAllocsMeasured )
            {
                Assert.That( report.IterationAllocs, Is.EqualTo( trackersCount + 7 ).Within( 0.5f ) );
            }
        }
        
        private void Ctor_Perf_Test_With_Array( IProgressSource< float >[] array, int trackersCount, int expectedResult )
        {
            var report = Measure.Method( () =>
                                {
                                    for( int i = 0; i < trackersCount; ++i )
                                    {
                                        array[ i ] = new MockProgressTracker();
                                    }

                                    var progressTracker = new CompositeProgressTracker( array );
                                } )
                                .WarmUpCount( WarmupCount )
                                .IterationsCount( IterationsCount )
                                .Run();

            if( report.IsIterationAllocsMeasured )
            {
                Assert.That( report.IterationAllocs, Is.EqualTo( expectedResult + 2 ).Within( 0.5f ) );
            }
        }

        private void Ctor_Perf_Test_With_Collection( ICollection< IProgressSource< float > > trackersCollection, int trackersCount, int expectedResult )
        {
            var report = Measure.Method( () =>
                                {
                                    for( int i = 0; i < trackersCount; ++i )
                                    {
                                        trackersCollection.Add( new MockProgressTracker() );
                                    }

                                    var progressTracker = new CompositeProgressTracker( trackersCollection );

                                    trackersCollection.Clear();
                                } )
                                .WarmUpCount( WarmupCount )
                                .IterationsCount( IterationsCount )
                                .Run();

            if( report.IsIterationAllocsMeasured )
            {
                Assert.That( report.IterationAllocs, Is.EqualTo( expectedResult + 2 ).Within( 0.5f ) );
            }
        }

        private sealed class MockProgressTracker : IProgressSource< float >
        {
            public event Action< float > OnProgressChanged;
            public float Progress { get; }
        }

        private sealed class MockEnumerable : IEnumerable< IProgressSource< float > >
        {
            private int _trackersCount;
            private readonly List< MockProgressTracker > _cachedTrackers;

            public MockEnumerable( int trackersCount )
            {
                _trackersCount = trackersCount;
                _cachedTrackers = new List< MockProgressTracker >( trackersCount );

                for( int i = 0; i < trackersCount; i++ )
                {
                    _cachedTrackers.Add( new MockProgressTracker() );
                }
            }

            public IEnumerator< IProgressSource< float > > GetEnumerator()
            {
                for( var i = 0; i < _cachedTrackers.Count; i++ )
                {
                    yield return _cachedTrackers[ i ];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}