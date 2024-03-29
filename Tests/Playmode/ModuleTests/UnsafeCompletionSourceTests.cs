﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
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
        public void Create_With_CancellationToken_Should_Succeed()
        {
            var source = UnsafeCompletionSource.Create( new CancellationToken() );

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
        public void CreateTResult_With_CancellationToken_Should_Succeed()
        {
            var source = UnsafeCompletionSource< int >.Create(new CancellationToken());

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
        public void TrySetError_Should_RejectTask_WithDefaultException()
        {
            var source = UnsafeCompletionSource.Create();

            Assert.IsTrue( source.TrySetError() );
            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.InstanceOf< TaskRejectedException >() );
        }

        [ Test ]
        public void TrySetError_Should_Not_RejectTask_WithDefaultException()
        {
            var source = UnsafeCompletionSource.Create();

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
            var source = UnsafeCompletionSource.Create();
            var exception = new ArgumentException();

            source.SetError( exception );

            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.EqualTo( exception ) );
        }
        
        [ Test ]
        public void TrySetError_Should_RejectTask_WithSpecificException()
        {
            var source = UnsafeCompletionSource.Create();
            var exception = new ArgumentException();

            Assert.IsTrue( source.TrySetError( exception ) );
            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.EqualTo( exception ) );
        } 
        [ Test ]
        public void TrySetError_Should_Not_RejectTask_WithSpecificException()
        {
            var source = UnsafeCompletionSource.Create();
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
            var source = UnsafeCompletionSource.Create();

            Assert.That( () => source.SetError( null ), Throws.ArgumentNullException );
        }

        [ Test ]
        public void TrySetError_Should_Throw_WithNullException()
        {
            var source = UnsafeCompletionSource.Create();

            Assert.That( () => source.TrySetError( null ), Throws.ArgumentNullException );
        }

        [ Test ]
        public void SetErrorTResult_Should_Throw_WithNullException()
        {
            var source = UnsafeCompletionSource< int >.Create();

            Assert.That( () => source.SetError( null ), Throws.ArgumentNullException );
        }

        [ Test ]
        public void TrySetErrorTResult_Should_Throw_WithNullException()
        {
            var source = UnsafeCompletionSource< int >.Create();

            Assert.That( () => source.TrySetError( null ), Throws.ArgumentNullException );
        }

        [ Test ]
        public void Resolve_Should_ResolveTask()
        {
            var source = UnsafeCompletionSource.Create();

            source.Resolve();

            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        }
        
        [ Test ]
        public void TryResolve_Should_ResolveTask()
        {
            var source = UnsafeCompletionSource.Create();

            var result = source.TryResolve();

            Assert.IsTrue( result );
            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
        } 
        
        [ Test ]
        public void TryResolve_Should_Not_ResolveTask()
        {
            var source = UnsafeCompletionSource.Create();

            source.Resolve();
            Assert.IsFalse( source.TryResolve());
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
        public void TrySetValueTResult_Should_ResolveTask()
        {
            var source = UnsafeCompletionSource< int >.Create();

            var result = source.TrySetValue( 123 );

            Assert.IsTrue( result );
            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Resolved ) );
            Assert.That( source.ResultTask.Result, Is.EqualTo( 123 ) );
        }
        
        [ Test ]
        public void TrySetValueTResult_Should_Not_ResolveTask()
        {
            var source = UnsafeCompletionSource< int >.Create();
            
            source.SetValue( 123 );
            Assert.IsFalse( source.TrySetValue( 123 ) );
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
        public void TryCancelTask_Should_RejectTask_WithTaskCancelled()
        {
            var source = UnsafeCompletionSource.Create();

            source.TryCancelTask();

            Assert.That( source.Task.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.Task.Error, Is.InstanceOf< TaskCanceledException >() );
        }    
        
        [ Test ]
        public void TryCancelTask_Should_Not_RejectTask()
        {
            var source = UnsafeCompletionSource.Create();

            source.CancelTask();
            Assert.IsFalse( source.TryCancelTask() );
        }

        [ Test ]
        public void CancelTask_Should_RejectTask_WithCancellationToken()
        {
            var cancellationToken = new CancellationTokenSource();
            var source = UnsafeCompletionSource.Create( cancellationToken.Token );

            cancellationToken.Cancel();

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
        public void TryCancelTaskTResult_Should_RejectTask()
        {
            var source = UnsafeCompletionSource< int >.Create();

            Assert.IsTrue( source.TryCancelTask() );
            Assert.That( source.ResultTask.Status, Is.EqualTo( PandaTaskStatus.Rejected ) );
            Assert.That( source.ResultTask.Error, Is.InstanceOf< TaskCanceledException >() );
        }
        
        [ Test ]
        public void TryCancelTaskTResult_Should_Not_RejectTask()
        {
            var source = UnsafeCompletionSource< int >.Create();

            source.CancelTask();
            Assert.IsFalse( source.TryCancelTask() );
        }
        
        [ Test ]
        public void CancelTaskTResult_Should_RejectTask_WithCancellationToken()
        {
            var cancellationToken = new CancellationTokenSource();
            var source = UnsafeCompletionSource< int >.Create(cancellationToken.Token);

            cancellationToken.Cancel();

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
