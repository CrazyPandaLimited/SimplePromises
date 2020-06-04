#if (UNITY_EDITOR && CRAZYPANDA_UNITYCORE_PROMISES_ENABLE_ASYNC_TESTS_EDITOR) || CRAZYPANDA_UNITYCORE_PROMISES_ENABLE_ASYNC_TESTS_PLAYMODE

using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace CrazyPanda.UnityCore.PandaTasks
{
    /// <summary>
    /// Wraps method returning <see cref="IPandaTask"/> or <see cref="Task"/> into a <see cref="IEnumerator"/> compatible with Unity's test conventions
    /// </summary>
    class EnumeratorTaskWrapper : IMethodInfo
    {
        private IMethodInfo _innerMethod;
        private readonly TimeSpan _timeout;

        public MethodInfo MethodInfo { get; }
        public ITypeInfo ReturnType { get; }

        public string Name => _innerMethod.Name;
        public bool IsAbstract => _innerMethod.IsAbstract;
        public bool IsPublic => _innerMethod.IsPublic;
        public bool ContainsGenericParameters => _innerMethod.ContainsGenericParameters;
        public bool IsGenericMethod => _innerMethod.IsGenericMethod;
        public bool IsGenericMethodDefinition => _innerMethod.IsGenericMethodDefinition;
        public ITypeInfo TypeInfo => _innerMethod.TypeInfo;

        public EnumeratorTaskWrapper( IMethodInfo method, TimeSpan timeout )
        {
            _innerMethod = method;
            _timeout = timeout;

            MethodInfo = new InvokeDelegatingMethodInfo( method.MethodInfo, Invoke );
            ReturnType = new TypeWrapper( typeof( IEnumerator ) );
        }

        public object Invoke( object fixture, params object[] args )
        {
            return RunEnumerator( fixture, args );
        }

        public T[] GetCustomAttributes<T>( bool inherit ) where T : class => _innerMethod.GetCustomAttributes<T>( inherit );
        public Type[] GetGenericArguments() => _innerMethod.GetGenericArguments();
        public IParameterInfo[] GetParameters() => _innerMethod.GetParameters();
        public bool IsDefined<T>( bool inherit ) => _innerMethod.IsDefined<T>( inherit );
        public IMethodInfo MakeGenericMethod( params Type[] typeArguments ) => _innerMethod.MakeGenericMethod( typeArguments );

        private IEnumerator RunEnumerator( object fixture, params object[] args )
        {
            // we should run all test code in the same thread because it may access some Unity stuff
            // so we use our custom SynchronizationContext to be able to control all continuations
            // this context will restore previous one on disposal
            using( var newSyncContext = new TestSynchronizationContext() )
            {
                var ret = _innerMethod.Invoke( fixture, args );                
                var context = TestContext.CurrentTestExecutionContext; // Unity uses this one, not TestExecutionContext.CurrentContext

                // wrap our async test inside a System.Threading.Task, so all continuations will go through our SynchronizationContext
                var waitTask = CreateWaitTask( ret );

                if( waitTask != null )
                {
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    // wait until task completes or time runs out
                    while( !waitTask.IsCompleted && sw.Elapsed < _timeout )
                    {
                        // fire all pending tasks
                        newSyncContext.Tick();
                        yield return null;
                    }

                    sw.Stop();

                    if( waitTask.IsFaulted )
                    {
                        // throw it the way that Unity's "Open error line" will direct to correct line and not here
                        ExceptionDispatchInfo.Capture( waitTask.Exception.InnerException ).Throw();
                    }
                    else if( waitTask.IsCompleted )
                    {
                        context.CurrentResult.SetResult( ResultState.Success );
                    }
                    else
                    {
                        context.CurrentResult.SetResult( ResultState.Failure, $"Test didn't complete after timeout of {_timeout.TotalSeconds:#.#} seconds" );
                    }
                }
                else
                {
                    context.CurrentResult.SetResult( ResultState.Failure, $"AsyncTest method must return IPandaTask or System.Threading.Task, but got {ret.GetType().Name ?? "<null>"}" );
                }
            }
        }

        private Task CreateWaitTask( object ret )
        {
            switch( ret )
            {
                case IPandaTask pandaTask:
                    Func<Task> wrap = async () => await pandaTask;
                    return wrap();
                case Task task:
                    return task;
                default:
                    return null;
            }
        }

        // helper class allowing to handle Invoke calls and direct them to our IEnumerator wrapper
        private class InvokeDelegatingMethodInfo : MethodInfo
        {
            private readonly MethodInfo _innerMethod;
            private readonly Func<object, object[], object> _invokeAction;

            public override Type DeclaringType => _innerMethod.DeclaringType;
            public override string Name => _innerMethod.Name;
            public override Type ReturnType => typeof( IEnumerator );

            public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();
            public override MethodAttributes Attributes => throw new NotImplementedException();
            public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();
            public override Type ReflectedType => throw new NotImplementedException();            

            public InvokeDelegatingMethodInfo( MethodInfo method, Func<object, object[], object> invokeAction )
            {
                _innerMethod = method;
                _invokeAction = invokeAction;
            }

            public override object Invoke( object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture )
            {
                return _invokeAction( obj, parameters );
            }

            public override MethodInfo GetBaseDefinition() => _innerMethod.GetBaseDefinition();
            public override object[] GetCustomAttributes( bool inherit ) => _innerMethod.GetCustomAttributes( inherit );
            public override object[] GetCustomAttributes( Type attributeType, bool inherit ) => _innerMethod.GetCustomAttributes( attributeType, inherit );

            public override MethodImplAttributes GetMethodImplementationFlags() => throw new NotImplementedException();
            public override ParameterInfo[] GetParameters() => throw new NotImplementedException();
            public override bool IsDefined( Type attributeType, bool inherit ) => throw new NotImplementedException();
        }
    }
}

#endif