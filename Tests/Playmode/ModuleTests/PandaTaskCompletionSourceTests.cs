using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    class PandaTaskCompletionSourceTests
    {
        [ Test ]
        public void Create_Should_Succeed()
        {
            var source = new PandaTaskCompletionSource();

            Assert.That( source.Task, Is.Not.Null );
            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Pending ) );
        }

        [ Test ]
        public void Create_With_CancellationToken_Should_Succeed()
        {
            var source = new PandaTaskCompletionSource( new CancellationToken() );

            Assert.That( source.Task, Is.Not.Null );
            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Pending ) );
        }

        [ Test ]
        public void CreateTResult_Should_Succeed()
        {
            var source = new PandaTaskCompletionSource<int>();

            Assert.That( source.ResultTask, Is.Not.Null );
            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Pending ) );
        }

        [ Test ]
        public void CreateTResult_With_CancellationToken_Should_Succeed()
        {
            var source = new PandaTaskCompletionSource< int >(new CancellationToken());

            Assert.That( source.ResultTask, Is.Not.Null );
            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Pending ) );
        }

             [ Test ]
        public void SetError_Should_RejectTask_WithDefaultException()
        {
            var source =new PandaTaskCompletionSource();

            source.SetError();

            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.InstanceOf< TaskRejectedException >() );
        }
        
        [ Test ]
        public void TrySetError_Should_RejectTask_WithDefaultException()
        {
            var source =new PandaTaskCompletionSource();

            Assert.IsTrue( source.TrySetError() );
            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.InstanceOf< TaskRejectedException >() );
        }

        [ Test ]
        public void TrySetError_Should_Not_RejectTask_WithDefaultException()
        {
            var source =new PandaTaskCompletionSource();

            source.SetError();
            Assert.IsFalse(source.TrySetError());
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
        public void TrySetErrorTResult_Should_RejectTask_WithDefaultException()
        {
            var source = UnsafeCompletionSource< int >.Create();

            Assert.IsTrue( source.TrySetError() );
            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.ResultTask.Error, Is.InstanceOf< TaskRejectedException >() );
        }

        [ Test ]
        public void TrySetErrorTResult_Should_Not_RejectTask_WithDefaultException()
        {
            var source = UnsafeCompletionSource< int >.Create();

            source.SetError();
            Assert.IsFalse( source.TrySetError() );
        }

        [ Test ]
        public void SetError_Should_RejectTask_WithSpecificException()
        {
            var source =new PandaTaskCompletionSource();
            var exception = new ArgumentException();

            source.SetError( exception );

            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.EqualTo( exception ) );
        }
        
        [ Test ]
        public void TrySetError_Should_RejectTask_WithSpecificException()
        {
            var source =new PandaTaskCompletionSource();
            var exception = new ArgumentException();

            Assert.IsTrue( source.TrySetError( exception ) );
            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.EqualTo( exception ) );
        } 
        [ Test ]
        public void TrySetError_Should_Not_RejectTask_WithSpecificException()
        {
            var source =new PandaTaskCompletionSource();
            var exception = new ArgumentException();

            source.SetError( exception );
            Assert.IsFalse( source.TrySetError( exception ) );
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
        public void TrySetErrorTResult_Should_RejectTask_WithSpecificException()
        {
            var source = UnsafeCompletionSource< int >.Create();
            var exception = new ArgumentException();

            Assert.IsTrue(source.TrySetError( exception ));
            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.ResultTask.Error, Is.EqualTo( exception ) );
        }

        [ Test ]
        public void TrySetErrorTResult_Should_Not_RejectTask_WithSpecificException()
        {
            var source = UnsafeCompletionSource< int >.Create();
            var exception = new ArgumentException();

            source.SetError( exception );
            Assert.IsFalse( source.TrySetError( exception ) );
        }

        [ Test ]
        public void SetError_Should_Throw_WithNullException()
        {
            var source =new PandaTaskCompletionSource();

            Assert.That( () => source.SetError( null ), Throws.ArgumentNullException );
        }

        [ Test ]
        public void TrySetError_Should_Throw_WithNullException()
        {
            var source =new PandaTaskCompletionSource();

            Assert.That( () => source.TrySetError( null ), Throws.ArgumentNullException );
        }

        [ Test ]
        public void SetErrorTResult_Should_Throw_WithNullException()
        {
            var source =new PandaTaskCompletionSource< int >();

            Assert.That( () => source.SetError( null ), Throws.ArgumentNullException );
        }

        [ Test ]
        public void TrySetErrorTResult_Should_Throw_WithNullException()
        {
            var source =new PandaTaskCompletionSource< int >();

            Assert.That( () => source.TrySetError( null ), Throws.ArgumentNullException );
        }

        [ Test ]
        public void Resolve_Should_ResolveTask()
        {
            var source = new PandaTaskCompletionSource();

            source.Resolve();

            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }
        
        [ Test ]
        public void TryResolve_Should_ResolveTask()
        {
            var source = new PandaTaskCompletionSource();

            var result = source.TryResolve();

            Assert.IsTrue( result );
            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        } 
        
        [ Test ]
        public void TryResolve_Should_Not_ResolveTask()
        {
            var source = new PandaTaskCompletionSource();

            source.Resolve();
            Assert.IsFalse( source.TryResolve());
        }

        [ Test ]
        public void SetValueTResult_Should_ResolveTask()
        {
            var source = new PandaTaskCompletionSource< int >();

            source.SetValue( 123 );

            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
            Assert.That( source.ResultTask.Result, Is.EqualTo( 123 ) );
        } 
        
        [ Test ]
        public void TrySetValueTResult_Should_ResolveTask()
        {
            var source = new PandaTaskCompletionSource< int >();

            var result = source.TrySetValue( 123 );

            Assert.IsTrue( result );
            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
            Assert.That( source.ResultTask.Result, Is.EqualTo( 123 ) );
        }
        
        [ Test ]
        public void TrySetValueTResult_Should_Not_ResolveTask()
        {
            var source = new PandaTaskCompletionSource< int >();
            
            source.SetValue( 123 );
            Assert.IsFalse( source.TrySetValue( 123 ) );
        }

        [ Test ]
        public void CancelTask_Should_RejectTask_WithTaskCancelled()
        {
            var source = new PandaTaskCompletionSource();

            source.CancelTask();

            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.InstanceOf< TaskCanceledException >() );
        }
        
        [ Test ]
        public void TryCancelTask_Should_RejectTask_WithTaskCancelled()
        {
            var source = new PandaTaskCompletionSource();

            source.TryCancelTask();

            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.InstanceOf< TaskCanceledException >() );
        }    
        
        [ Test ]
        public void TryCancelTask_Should_Not_RejectTask()
        {
            var source = new PandaTaskCompletionSource();

            source.CancelTask();
            Assert.IsFalse( source.TryCancelTask() );
        }

        [ Test ]
        public void CancelTask_Should_RejectTask_WithCancellationToken()
        {
            var cancellationToken = new CancellationTokenSource();
            var source = new PandaTaskCompletionSource( cancellationToken.Token );

            cancellationToken.Cancel();

            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.InstanceOf< TaskCanceledException >() );
        }

        [ Test ]
        public void CancelTaskTResult_Should_RejectTask_WithTaskCancelled()
        {
            var source = new PandaTaskCompletionSource< int >();

            source.CancelTask();

            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.ResultTask.Error, Is.InstanceOf< TaskCanceledException >() );
        }
        
        [ Test ]
        public void TryCancelTaskTResult_Should_RejectTask()
        {
            var source = new PandaTaskCompletionSource< int >();

            Assert.IsTrue( source.TryCancelTask() );
            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.ResultTask.Error, Is.InstanceOf< TaskCanceledException >() );
        }
        
        [ Test ]
        public void TryCancelTaskTResult_Should_Not_RejectTask()
        {
            var source = new PandaTaskCompletionSource< int >();

            source.CancelTask();
            Assert.IsFalse( source.TryCancelTask() );
        }
        
        [ Test ]
        public void CancelTaskTResult_Should_RejectTask_WithCancellationToken()
        {
            var cancellationToken = new CancellationTokenSource();
            var source = new PandaTaskCompletionSource< int >(cancellationToken.Token);

            cancellationToken.Cancel();

            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.ResultTask.Error, Is.InstanceOf< TaskCanceledException >() );
        }
    }
}