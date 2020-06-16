#if (UNITY_EDITOR && CRAZYPANDA_UNITYCORE_PROMISES_ENABLE_ASYNC_TESTS_EDITOR) || CRAZYPANDA_UNITYCORE_PROMISES_ENABLE_ASYNC_TESTS_PLAYMODE

using System;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;

namespace CrazyPanda.UnityCore.PandaTasks.Asserts
{
    // Unity's builtin Assert class have no support for async Asserts, so we add them via this class
    // Add this line in your usings to activate it:
    //   using Assert = CrazyPanda.UnityCore.PandaTasks.AsyncAssert;
    public class Assert : NUnit.Framework.Assert
    {
        /// <summary>
        /// Verifies that an async delegate throws a particular exception when called. The returned exception may be
        /// null when inside a multiple assert block.
        /// </summary>
        /// <param name="expression">A constraint to be satisfied by the exception</param>
        /// <param name="code">A TestSnippet delegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static async Task< Exception > ThrowsAsync( IResolveConstraint expression, Func< Task > code, string message, params object[] args )
        {
            Exception caughtException = null;
            try
            {
                await code();
            }
            catch( Exception e )
            {
                caughtException = e;
            }

            That( caughtException, expression, message, args );

            return caughtException;
        }

        /// <summary>
        /// Verifies that an async delegate throws a particular exception when called. The returned exception may be
        /// <see langword="null"/> when inside a multiple assert block.
        /// </summary>
        /// <param name="expression">A constraint to be satisfied by the exception</param>
        /// <param name="code">A TestSnippet delegate</param>
        public static Task< Exception > ThrowsAsync( IResolveConstraint expression, Func< Task > code )
        {
            return ThrowsAsync( expression, code, string.Empty, null );
        }

        /// <summary>
        /// Verifies that an async delegate throws a particular exception when called. The returned exception may be
        /// <see langword="null"/> when inside a multiple assert block.
        /// </summary>
        /// <param name="expectedExceptionType">The exception Type expected</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Task< Exception > ThrowsAsync( Type expectedExceptionType, Func< Task > code, string message, params object[] args )
        {
            return ThrowsAsync( new ExceptionTypeConstraint( expectedExceptionType ), code, message, args );
        }

        /// <summary>
        /// Verifies that an async delegate throws a particular exception when called. The returned exception may be
        /// <see langword="null"/> when inside a multiple assert block.
        /// </summary>
        /// <param name="expectedExceptionType">The exception Type expected</param>
        /// <param name="code">A TestDelegate</param>
        public static Task< Exception > ThrowsAsync( Type expectedExceptionType, Func< Task > code )
        {
            return ThrowsAsync( new ExceptionTypeConstraint( expectedExceptionType ), code, string.Empty, null );
        }
        
        /// <summary>
        /// Verifies that an async delegate throws a particular exception when called. The returned exception may be
        /// <see langword="null"/> when inside a multiple assert block.
        /// </summary>
        /// <typeparam name="TActual">Type of the expected exception</typeparam>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static async Task<TActual> ThrowsAsync<TActual>(Func<Task> code, string message, params object[] args) where TActual : Exception
        {
            return ( TActual ) await ThrowsAsync(typeof (TActual), code, message, args);
        }
        
        /// <summary>
        /// Verifies that an async delegate throws a particular exception when called. The returned exception may be
        /// <see langword="null"/> when inside a multiple assert block.
        /// </summary>
        /// <typeparam name="TActual">Type of the expected exception</typeparam>
        /// <param name="code">A TestDelegate</param>
        public static Task< TActual > ThrowsAsync< TActual >( Func< Task > code ) where TActual : Exception
        {
            return ThrowsAsync< TActual >( code, string.Empty, null );
        }
        
        /// <summary>
        /// Verifies that an async delegate throws an exception when called and returns it. The returned exception may
        /// be <see langword="null"/> when inside a multiple assert block.
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        public static Task< Exception > CatchAsync( Func< Task > code )
        {
            return ThrowsAsync(new InstanceOfTypeConstraint(typeof(Exception)), code);
        }
        
        /// <summary>
        /// Verifies that an async delegate throws an exception of a certain Type or one derived from it when called and
        /// returns it. The returned exception may be <see langword="null"/> when inside a multiple assert block.
        /// </summary>
        /// <param name="expectedExceptionType">The expected Exception Type</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Task<Exception> CatchAsync(Type expectedExceptionType, Func<Task> code, string message, params object[] args)
        {
            return ThrowsAsync(new InstanceOfTypeConstraint(expectedExceptionType), code, message, args);
        }

        /// <summary>
        /// Verifies that an async delegate throws an exception of a certain Type or one derived from it when called and
        /// returns it. The returned exception may be <see langword="null"/> when inside a multiple assert block.
        /// </summary>
        /// <param name="expectedExceptionType">The expected Exception Type</param>
        /// <param name="code">A TestDelegate</param>
        public static Task<Exception> CatchAsync(Type expectedExceptionType, Func<Task> code)
        {
            return ThrowsAsync(new InstanceOfTypeConstraint(expectedExceptionType), code);
        }
        
        /// <summary>
        /// Verifies that an async delegate throws an exception of a certain Type or one derived from it when called and
        /// returns it. The returned exception may be <see langword="null"/> when inside a multiple assert block.
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static async Task<TActual> CatchAsync<TActual>(Func<Task> code, string message, params object[] args) where TActual : Exception
        {
            return (TActual) await ThrowsAsync(new InstanceOfTypeConstraint(typeof (TActual)), code, message, args);
        }

        /// <summary>
        /// Verifies that an async delegate throws an exception of a certain Type or one derived from it when called and
        /// returns it. The returned exception may be <see langword="null"/> when inside a multiple assert block.
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        public static Task<TActual> CatchAsync<TActual>(Func<Task> code) where TActual : Exception
        {
            return CatchAsync< TActual >( code, string.Empty );
        }
    }
}
#endif