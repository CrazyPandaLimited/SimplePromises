using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    [ Category( "ModuleTests" ), Category( "LocalTests" ) ]
    class UnsafeCompletionSourceTests
    {
        [ Test ]
        public void Create_Should_Succeed()
        {
            var source = UnsafeCompletionSource.Create();

            Assert.That( source.Task, Is.Not.Null );
            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Pending ) );
        }

        [ Test ]
        public void CreateTResult_Should_Succeed()
        {
            var source = UnsafeCompletionSource< int >.Create();

            Assert.That( source.ResultTask, Is.Not.Null );
            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Pending ) );
        }

        [ Test ]
        public void SetError_Should_RejectTask_WithDefaultException()
        {
            var source = UnsafeCompletionSource.Create();

            source.SetError();

            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.InstanceOf< TaskRejectedException >() );
        }

        [ Test ]
        public void SetErrorTResult_Should_RejectTask_WithDefaultException()
        {
            var source = UnsafeCompletionSource< int >.Create();

            source.SetError();

            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.ResultTask.Error, Is.InstanceOf< TaskRejectedException >() );
        }

        [ Test ]
        public void SetError_Should_RejectTask_WithSpecificException()
        {
            var source = UnsafeCompletionSource.Create();
            var exception = new ArgumentException();

            source.SetError( exception );

            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.EqualTo( exception ) );
        }

        [ Test ]
        public void SetErrorTResult_Should_RejectTask_WithSpecificException()
        {
            var source = UnsafeCompletionSource< int >.Create();
            var exception = new ArgumentException();

            source.SetError( exception );

            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.ResultTask.Error, Is.EqualTo( exception ) );
        }

        [ Test ]
        public void SetError_Should_Throw_WithNullException()
        {
            var source = UnsafeCompletionSource.Create();

            Assert.That( () => source.SetError( null ), Throws.ArgumentNullException );
        }

        [ Test ]
        public void SetErrorTResult_Should_Throw_WithNullException()
        {
            var source = UnsafeCompletionSource< int >.Create();

            Assert.That( () => source.SetError( null ), Throws.ArgumentNullException );
        }

        [ Test ]
        public void Resolve_Should_ResolveTask()
        {
            var source = UnsafeCompletionSource.Create();

            source.Resolve();

            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }

        [ Test ]
        public void SetValueTResult_Should_ResolveTask()
        {
            var source = UnsafeCompletionSource< int >.Create();

            source.SetValue( 123 );

            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
            Assert.That( source.ResultTask.Result, Is.EqualTo( 123 ) );
        }

        [ Test ]
        public void CancelTask_Should_RejectTask_WithTaskCancelled()
        {
            var source = UnsafeCompletionSource.Create();

            source.CancelTask();

            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.InstanceOf< TaskCanceledException >() );
        }

        [ Test ]
        public void CancelTaskTResult_Should_RejectTask_WithTaskCancelled()
        {
            var source = UnsafeCompletionSource< int >.Create();

            source.CancelTask();

            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.ResultTask.Error, Is.InstanceOf< TaskCanceledException >() );
        }

        [ Test ]
        public void Uninitialized_Should_Throw()
        {
            var source = new UnsafeCompletionSource();

            Assert.That( () => source.Task, Throws.InvalidOperationException );
            Assert.That( () => source.SetError(), Throws.InvalidOperationException );
            Assert.That( () => source.SetError( new ArgumentException() ), Throws.InvalidOperationException );
            Assert.That( () => source.Resolve(), Throws.InvalidOperationException );
            Assert.That( () => source.CancelTask(), Throws.InvalidOperationException );
        }

        [ Test ]
        public void UninitializedTResult_Should_Throw()
        {
            var source = new UnsafeCompletionSource< int >();

            Assert.That( () => source.ResultTask, Throws.InvalidOperationException );
            Assert.That( () => source.SetError(), Throws.InvalidOperationException );
            Assert.That( () => source.SetError( new ArgumentException() ), Throws.InvalidOperationException );
            Assert.That( () => source.SetValue( 123 ), Throws.InvalidOperationException );
            Assert.That( () => source.CancelTask(), Throws.InvalidOperationException );
        }
    }
}
