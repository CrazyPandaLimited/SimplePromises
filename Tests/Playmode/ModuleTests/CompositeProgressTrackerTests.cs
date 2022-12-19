using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    sealed class CompositeProgressTrackerTests
    {
        private static readonly object[][] _progressShouldBeCorrectTestCases = new object[][]
        {
            new object[] {Enumerable.Range( 1, 15 ).Select( _=> new MockProgressTracker() ).ToList()},
            new object[]{Enumerable.Range( 1, 1 ).Select( _=> new MockProgressTracker() ).ToList()},
            new object[]{Enumerable.Range( 1, 1 ).Select( _=> new MockProgressTracker() ).ToArray()},
            new object[]{Enumerable.Range( 1, 15 ).Select( _=> new MockProgressTracker() ).ToArray()},
            new object[]{new HashSet<IProgressTracker<float>>(Enumerable.Range( 1, 1 ).Select( _=> new MockProgressTracker() ))},
            new object[]{new HashSet<IProgressTracker<float>>(Enumerable.Range( 1, 15 ).Select( _=> new MockProgressTracker() ))},
            new object[]{new EnumerableMockProgressTracker(Enumerable.Range( 1, 15 ).Select( _=> new MockProgressTracker() ))}
        };

        [Test]
        public void OnProgressChanged_Should_Invoked()
        {
            const int progressTrackersCount = 16;

            var progressTrackersCollection = new List< IProgressTracker< float > >( progressTrackersCount );

            for( int i = 0; i < progressTrackersCount; i++ )
            {
                progressTrackersCollection.Add( new MockProgressTracker() );
            }

            bool trackerHandlerWasCalled = false;
            
            _ = new CompositeProgressTracker( progressTrackersCollection, _ => trackerHandlerWasCalled = true );

            const float progressToPass = 0.4f;
            
            foreach( var progressTracker in progressTrackersCollection )
            {
                progressTracker.ReportProgress( 0.4f );
            }
            
            Assert.IsTrue( trackerHandlerWasCalled );
        }

        [ TestCaseSource( nameof(_progressShouldBeCorrectTestCases) ) ]
        public void Progress_Should_Be_Correct( IEnumerable< IProgressTracker< float > > enumerable )
        {
            float progressFromHandler = 0;
            var compositeProgressTracker = new CompositeProgressTracker( enumerable, progress => progressFromHandler = progress );

            const float progressToPass = 0.4f;
            
            foreach( var progressTracker in enumerable )
            {
                progressTracker.ReportProgress( progressToPass );
            }
            
            Assert.That( progressFromHandler, Is.EqualTo( progressToPass ).Within( 0.0001f ) );
            Assert.That( compositeProgressTracker.Progress, Is.EqualTo( progressToPass ).Within( 0.0001f ) );
        }
        
        [Test]
        public void Progress_Should_Be_Correct_At_Empty_Collection()
        {
            float progressFromHandler = 0;
            var compositeProgressTracker = new CompositeProgressTracker( new List< IProgressSource< float > >(),
                                                                         progress => progressFromHandler = progress );

            Assert.That( progressFromHandler, Is.EqualTo( 1f ).Within( 0.0001f ) );
            Assert.That( compositeProgressTracker.Progress, Is.EqualTo( 1f ).Within( 0.0001f ) );
        }
        
        private sealed class MockProgressTracker : IProgressTracker<float>
        {
            public event Action< float > OnProgressChanged;

            public float Progress { get; private set; }

            public void ReportProgress( float value )
            {
                Progress = value; 
                OnProgressChanged?.Invoke( Progress );
            }
        }
        
        private sealed class EnumerableMockProgressTracker : IEnumerable<IProgressTracker<float>>
        {
            private List< IProgressTracker< float > > _progressTrackers = new List< IProgressTracker< float > >();

            public EnumerableMockProgressTracker( IEnumerable<IProgressTracker<float>> collection )
            {
                _progressTrackers.AddRange( collection );
            }

            public IEnumerator< IProgressTracker< float > > GetEnumerator()
            {
                return _progressTrackers.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}