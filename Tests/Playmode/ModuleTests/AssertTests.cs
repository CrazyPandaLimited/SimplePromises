using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Assert = CrazyPanda.UnityCore.PandaTasks.Asserts.Assert;

// we get this warning from these lines:
//  async () => throw ...
#pragma warning disable 1998

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    sealed class AssertTests
    {
        private const int MaxSecondsTestsExecutionTimeout = 30;

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task ThrowsAsync_Should_Succeed()
        {
            await Assert.ThrowsAsync( typeof( ArgumentException ), async () => throw new ArgumentException() );
        }
        
        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task ThrowsAsync_Should_Succeed_WithCustomExpressionConstraint()
        {
            await Assert.ThrowsAsync( new ExceptionTypeConstraint( typeof( ArgumentException ) ), async () => throw new ArgumentException() );
        }

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task ThrowsAsync_Should_ReturnCorrectException()
        {
            await ThrowsAsyncShouldReturnCorrectExceptionBaseTest( exception => Assert.ThrowsAsync( exception.GetType(), async () => throw exception ) );
        }

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task ThrowsAsync_Should_ThrowAssertionException_WhenNoAnyExceptionWasThrown()
        {
            var exceptionToThrow = new ArgumentException( "some_message", "some_name" );

            var result = await SafelyCatchAnNUnitExceptionAsync( () => Assert.ThrowsAsync(exceptionToThrow.GetType(), async () => { } ) );

            Assert.That( result, new ExceptionTypeConstraint( typeof( AssertionException ) ) );
            Assert.That( exceptionToThrow, Is.Not.EqualTo( result ) );
            Assert.That( result.Message, Is.EqualTo( $"  Expected: <{exceptionToThrow.GetType().FullName}>" + Environment.NewLine + 
                                                     "  But was:  null" + Environment.NewLine ) );
        }

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task ThrowsAsync_Should_ThrowAssertionException_WhenUnrelatedExceptionWasThrown()
        {
            var expectedExceptionType = typeof( NullReferenceException );
            var unrelatedException = new ArgumentException( "some_message", "some_name" );

            var result = await SafelyCatchAnNUnitExceptionAsync( () => Assert.ThrowsAsync( expectedExceptionType, 
                                                                                           async () =>  throw unrelatedException ) );

            Assert.That( result, new ExceptionTypeConstraint( typeof( AssertionException ) ) );
            Assert.That( expectedExceptionType, Is.Not.TypeOf( result.GetType() ) );
            Assert.That(result.Message, Does.StartWith( $"  Expected: <{expectedExceptionType.FullName}>" + Environment.NewLine + 
                                                        $"  But was:  <{unrelatedException.GetType().FullName}: {unrelatedException.Message}" + Environment.NewLine));
        }
        
        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task ThrowsAsync_Should_ThrowAssertionException_WhenCustomExpressionConstraintIncorrect()
        {
            var result = await SafelyCatchAnNUnitExceptionAsync( () => Assert.ThrowsAsync( new ExceptionTypeConstraint( typeof( ArgumentException )) , 
                                                                                           async () =>  throw new NullReferenceException() ) );

            Assert.That( result, new ExceptionTypeConstraint( typeof( AssertionException ) ) );
        }
        
        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task ThrowsAsync_Should_ReturnCorrectMessage_WhenAssertionIsFailed()
        {
            var expectedExceptionType = typeof( NullReferenceException );
            const string expectedMessage = "assertion_failed_message";

            var result = await SafelyCatchAnNUnitExceptionAsync( () => 
                                                   Assert.ThrowsAsync( expectedExceptionType, async () => { }, expectedMessage) );

           Assert.That( result.Message, Does.Contain( expectedMessage ) );
        }

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task GenericThrowsAsync_Should_Succeed()
        {
            await Assert.ThrowsAsync< ArgumentException >( async () => throw new ArgumentException() );
        }

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task GenericThrowsAsync_Should_ReturnCorrectException()
        {
            await ThrowsAsyncShouldReturnCorrectExceptionBaseTest( async exception => 
                                                await Assert.ThrowsAsync<ArgumentException>(  async () => throw exception ) );
        }

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task GenericThrowsAsync_Should_ThrowAssertionException_WhenNoAnyExceptionWasThrown()
        {
            var exceptionToThrow = new ArgumentException( "some_message", "some_name" );

            var result = await SafelyCatchAnNUnitExceptionAsync( () => Assert.ThrowsAsync< ArgumentException >( async () => { } ) );

            Assert.That( result, new ExceptionTypeConstraint( typeof( AssertionException ) ) );
            Assert.That( exceptionToThrow, Is.Not.EqualTo( result ) );
            Assert.That( result.Message, Is.EqualTo( $"  Expected: <{exceptionToThrow.GetType().FullName}>" + Environment.NewLine + 
                                                     "  But was:  null" + Environment.NewLine ) );
        }
        
        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task GenericThrowsAsync_Should_ThrowAssertionException_WhenUnrelatedExceptionWasThrown()
        {
            var expectedExceptionType = typeof( NullReferenceException );
            var unrelatedException = new ArgumentException( "some_message", "some_name" );

            var result = await SafelyCatchAnNUnitExceptionAsync( () => 
                                            Assert.ThrowsAsync<NullReferenceException>( async () =>  throw unrelatedException ) );

            Assert.That( result, new ExceptionTypeConstraint( typeof( AssertionException ) ) );
            Assert.That( expectedExceptionType, Is.Not.TypeOf( result.GetType() ) );
            Assert.That(result.Message, Does.StartWith( $"  Expected: <{expectedExceptionType.FullName}>" + Environment.NewLine + 
                                                        $"  But was:  <{unrelatedException.GetType().FullName}: {unrelatedException.Message}" + Environment.NewLine));
        }
        
        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task GenericThrowsAsync_Should_ReturnCorrectMessage_WhenAssertionIsFailed()
        {
            const string expectedMessage = "assertion_failed_message";

            var result = await SafelyCatchAnNUnitExceptionAsync( () => 
                                                   Assert.ThrowsAsync<NullReferenceException>( async () => { }, expectedMessage) );

            Assert.That( result.Message, Does.Contain( expectedMessage ) );
        }

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task CatchAsync_Should_Succeed()
        {
            await Assert.CatchAsync( typeof( ArgumentException ), async () => throw new ArgumentException() );
        }

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task CatchAsync_Should_Should_ReturnCorrectException()
        {
            await ThrowsAsyncShouldReturnCorrectExceptionBaseTest( exception => Assert.CatchAsync( exception.GetType(), async () => throw exception ) );
        }
        
        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task CatchAsync_Should_Succeed_WithBaseExceptionType()
        {
            await Assert.CatchAsync( typeof( Exception ), async () => throw new ArgumentException() );
        }

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task CatchAsync_Should_ReturnCorrectExceptionWhenBaseExceptionTypeWasUsed()
        {
            await ThrowsAsyncShouldReturnCorrectExceptionBaseTest( exception => Assert.CatchAsync( typeof(Exception), async () => throw exception ) );
        }

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task CatchAsync_Should_ReturnCorrectMessage_WhenAssertionIsFailed()
        {
            const string expectedMessage = "assertion_failed_message";

            var result = await SafelyCatchAnNUnitExceptionAsync( () => 
                                        Assert.CatchAsync( typeof(NullReferenceException), async () => { }, expectedMessage) );

            Assert.That( result.Message, Does.Contain( expectedMessage ) );
        }
        
        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task GenericCatchAsync_Should_Succeed()
        {
            await Assert.CatchAsync<ArgumentException>(  async () => throw new ArgumentException() );
        }

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task GenericCatchAsync_Should_Should_ReturnCorrectException()
        {
            await ThrowsAsyncShouldReturnCorrectExceptionBaseTest( async exception => await Assert.CatchAsync<ArgumentException>(  async () => throw exception ) );
        }
        
        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task GenericCatchAsync_Should_Succeed_WithBaseExceptionType()
        {
            await Assert.CatchAsync<Exception>( async () => throw new ArgumentException() );
        }

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task GenericCatchAsync_Should_Should_ReturnCorrectExceptionWhenBaseExceptionTypeWasUsed()
        {
            await ThrowsAsyncShouldReturnCorrectExceptionBaseTest( exception => Assert.CatchAsync<Exception>( async () => throw exception ) );
        }

        [ AsyncTest( MaxSecondsTestsExecutionTimeout ) ]
        public async Task GenericCatchAsync_Should_ReturnCorrectMessage_WhenAssertionIsFailed()
        {
            const string expectedMessage = "assertion_failed_message";
            var result = await SafelyCatchAnNUnitExceptionAsync( () => Assert.CatchAsync<NullReferenceException>(  async () => { }, expectedMessage) );

            Assert.That( result.Message, Does.Contain( expectedMessage ) );
        }
        
        private async Task ThrowsAsyncShouldReturnCorrectExceptionBaseTest( Func<Exception, Task< Exception > > code )
        {
            var exceptionToThrow = new ArgumentException( "some_message", "some_param" );
            var result = await code(exceptionToThrow) as ArgumentException;

            Assert.That( result, new ExceptionTypeConstraint( exceptionToThrow.GetType() ), string.Empty, null );
            Assert.IsNotNull( result, "No ArgumentException thrown" );
            Assert.That( exceptionToThrow.Message, Is.EqualTo( result.Message ) );
            Assert.That( exceptionToThrow.ParamName, Is.EqualTo( result.ParamName ) );
            Assert.That( exceptionToThrow, Is.EqualTo( result ) );
        }

        private async Task< Exception > SafelyCatchAnNUnitExceptionAsync( Func< Task > code )
        {
            Exception caughtException = null;

            try
            {
                await code();
            }
            catch( Exception ex )
            {
                caughtException = ex;
            }

            return caughtException;
        }
    }
}